#!/usr/bin/env python3
"""Run the supported Graphify CLI locally, then validate the portable code snapshot.

No game source is modified. No semantic/model backend is enabled. The native AST
manifest supplies content freshness across commits, branches and cloned checkouts.
"""
from __future__ import annotations

import argparse
import hashlib
from importlib.metadata import PackageNotFoundError, version
import json
import os
from pathlib import Path, PurePosixPath
import subprocess
import sys

REPOSITORY = Path(__file__).resolve().parents[2]
GRAPHIFY_VERSION = "0.9.104"
ARTIFACTS = ("graph.json", "graph.html", "GRAPH_REPORT.md", "manifest.json", ".graphify_build.json")


def tool_environment() -> dict[str, str]:
    environment = os.environ.copy()
    environment.update({
        "GRAPHIFY_BACKEND": "none",
        "GRAPHIFY_QUERY_LOG_DISABLE": "1",
        "GRAPHIFY_MAX_WORKERS": "1",
        "GRAPHIFY_OUT": "graphify-out",
        "GRAPHIFY_VIZ_NODE_LIMIT": "5000",
        "PYTHONUTF8": "1",
    })
    # The wrapper's explicit --full flag is the only way to request a forced rebuild.
    environment.pop("GRAPHIFY_FORCE", None)
    return environment


def ensure_version() -> None:
    try:
        installed = version("graphifyy")
    except PackageNotFoundError as error:
        raise ValueError("Graphify is not installed in this interpreter; run python3 Tools/Graphify/setup.py, then activate .venv-graphify") from error
    if installed != GRAPHIFY_VERSION:
        raise ValueError(f"Expected graphifyy {GRAPHIFY_VERSION}, found {installed}; install Tools/Graphify/requirements-lock.txt")


def detect_code(root: Path) -> dict:
    from graphify.detect import detect

    detection = detect(root)
    if detection.get("walk_errors"):
        raise ValueError(f"Source traversal failed: {detection['walk_errors']}")
    return detection


def validate_snapshot(root: Path) -> dict:
    """Fail on stale input hashes, missing C# files, broken edges, or excluded sources.

    MD5 is used only to compare Graphify's existing native cache fingerprints, not
    as a security mechanism. SHA provenance and Unity compilation are not inferred.
    """
    ensure_version()
    root = root.resolve()
    output = root / "graphify-out"
    for name in ARTIFACTS:
        path = output / name
        if not path.is_file() or path.stat().st_size == 0:
            raise ValueError(f"Missing or empty Graphify artifact: {name}")
    configuration = json.loads((output / ".graphify_build.json").read_text(encoding="utf-8"))
    if configuration.get("code_only") is not True:
        raise ValueError("Snapshot is not marked code-only; review before replacing a semantic graph")
    graph = json.loads((output / "graph.json").read_text(encoding="utf-8"))
    metadata = graph.get("graph", {})
    if metadata.get("schema_version") != 1 or metadata.get("graphify_version") != GRAPHIFY_VERSION:
        raise ValueError("Unexpected Graphify schema or producer version")
    nodes, edges = graph.get("nodes", []), graph.get("links", [])
    ids = {node["id"] for node in nodes}
    if not nodes or not edges or len(ids) != len(nodes):
        raise ValueError("Graph must be nonempty with unique node IDs")
    for edge in edges:
        if edge.get("source") not in ids or edge.get("target") not in ids:
            raise ValueError("Graph has a dangling edge")

    detection = detect_code(root)
    code = {}
    for name in detection["files"].get("code", []):
        path = Path(name).resolve()
        relative = path.relative_to(root).as_posix()
        code[relative] = path
    if not code:
        raise ValueError("No analyzable code inputs")
    manifest = json.loads((output / "manifest.json").read_text(encoding="utf-8"))
    for relative, path in code.items():
        actual = hashlib.md5(path.read_bytes(), usedforsecurity=False).hexdigest()
        if manifest.get(relative, {}).get("ast_hash") != actual:
            raise ValueError(f"Stale/missing native AST fingerprint: {relative}; run the update command")
    # A deleted/excluded file must not linger as a live graph source.
    sources = set()
    for node in nodes:
        source = node.get("source_file")
        if not source:
            continue  # Graphify may add external/import concept nodes with no local source.
        path = PurePosixPath(source)
        if path.is_absolute() or ".." in path.parts or source not in code:
            raise ValueError(f"Excluded, deleted, or nonportable source in graph: {source}")
        sources.add(source)
    missing_csharp = sorted(name for name in code if name.endswith(".cs") and name not in sources)
    if missing_csharp:
        raise ValueError(f"C# inputs missing from graph: {missing_csharp}")
    report = (output / "GRAPH_REPORT.md").read_text(encoding="utf-8")
    if "Token cost: 0 input" not in report or "0 output" not in report:
        raise ValueError("Expected the zero-model-token code-only report")
    return {
        "nodes": len(nodes), "edges": len(edges), "code_inputs": len(code),
        "csharp_files": sum(name.endswith(".cs") for name in code),
        "unclassified_files": len(detection.get("unclassified", [])),
        "sensitive_files_skipped": detection.get("skipped_sensitive", []),
        "built_at_commit": graph.get("built_at_commit"),
        "graphify_version": metadata["graphify_version"], "fresh_inputs": True,
    }


def command(root: Path, *arguments: str) -> None:
    subprocess.run([sys.executable, "-m", "graphify", *arguments], cwd=root,
                   env=tool_environment(), check=True)


def update(root: Path, full: bool = False) -> dict:
    ensure_version()
    root = root.resolve()
    output = root / "graphify-out"
    if full or not (output / "graph.json").is_file():
        arguments = ["extract", ".", "--code-only", "--max-workers", "1"]
        if full:
            arguments.append("--force")
        command(root, *arguments)
    else:
        configuration = json.loads((output / ".graphify_build.json").read_text(encoding="utf-8"))
        if configuration.get("code_only") is not True:
            raise ValueError("Existing graph is not code-only; use a separate semantic output or review an explicit --full rebuild")
        command(root, "update", ".")
    # extract intentionally emits JSON only. update normally renders all outputs,
    # but can be a no-op on a fresh clone whose report/HTML were not copied.
    if full or not all((output / name).is_file() for name in ("GRAPH_REPORT.md", "graph.html")):
        command(root, "cluster-only", ".", "--no-label")
    return validate_snapshot(root)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=REPOSITORY)
    parser.add_argument("--full", action="store_true", help="Explicitly rebuild the owned code-only snapshot")
    parser.add_argument("--check", action="store_true", help="Validate source fingerprints and artifacts without rebuilding")
    arguments = parser.parse_args()
    if arguments.full and arguments.check:
        parser.error("--full and --check cannot be combined")
    os.environ.update({key: value for key, value in tool_environment().items()
                       if key.startswith("GRAPHIFY_") or key == "PYTHONUTF8"})
    try:
        result = validate_snapshot(arguments.root) if arguments.check else update(arguments.root, arguments.full)
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        print(f"Graphify workflow failed: {error}", file=sys.stderr)
        return 1
    print(json.dumps(result, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
