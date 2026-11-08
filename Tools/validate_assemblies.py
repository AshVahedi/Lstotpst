#!/usr/bin/env python3
"""Independent Unity assembly/GUID audit. Does not import assets or bind Unity APIs.

Only known package assemblies are allowed; add explicit package provenance here
if a future asmdef references another package. No directories outside Assets are
treated as game assemblies (in particular Library and Graphify are excluded).
"""
import argparse
import json
from pathlib import Path
import re
import sys

PACKAGE_ASSEMBLIES = {
    "UnityEngine.TestRunner": "com.unity.test-framework",
    "UnityEditor.TestRunner": "com.unity.test-framework",
}
GUID = re.compile(r"^guid:\s*([0-9a-f]{32})\s*$", re.MULTILINE)
ASSET_GUID = re.compile(r"\bguid:\s*([0-9a-f]{32})\b")


def audit(root):
    root = Path(root)
    assets = root / "Assets"
    errors, warnings = [], []

    def read_json(path):
        try:
            value = json.loads(path.read_text(encoding="utf-8"))
            if not isinstance(value, dict):
                raise ValueError("expected a JSON object")
            return value
        except (OSError, ValueError) as exc:
            errors.append(f"{path.relative_to(root)}: {exc}")
            return {}

    manifest = read_json(root / "Packages/manifest.json")
    dependencies = manifest.get("dependencies", {})
    if not isinstance(dependencies, dict):
        errors.append("Package manifest dependencies must be an object")
        dependencies = {}
    lock_path = root / "Packages/packages-lock.json"
    if lock_path.exists():
        locked = read_json(lock_path).get("dependencies", {})
        for name, version in dependencies.items():
            # Registry versions are exact; file/git/builtin resolutions can differ.
            if re.fullmatch(r"\d+\.\d+\.\d+(?:[-+].+)?", str(version)):
                entry = locked.get(name) if isinstance(locked, dict) else None
                if not isinstance(entry, dict) or entry.get("version") != version:
                    errors.append(f"Package lock does not match manifest: {name} {version}")
    else:
        warnings.append("No package lockfile: Unity package resolution is unverified")

    guid_assets = {}
    for meta in sorted(assets.rglob("*.meta")):
        match = GUID.search(meta.read_text(encoding="utf-8"))
        if not match:
            errors.append(f"Invalid metadata GUID: {meta.relative_to(root)}")
            continue
        asset = Path(str(meta)[:-5])
        if not asset.exists():
            errors.append(f"Orphan metadata: {meta.relative_to(root)}")
        if match[1] in guid_assets:
            errors.append(f"Duplicate asset GUID: {match[1]}")
        guid_assets[match[1]] = asset

    definitions, path_names = {}, {}
    for path in sorted(assets.rglob("*.asmdef")):
        definition = read_json(path)
        name = definition.get("name")
        if not isinstance(name, str) or not name.strip():
            errors.append(f"Missing assembly name: {path.relative_to(root)}")
            continue
        if name in definitions:
            errors.append(f"Duplicate assembly name: {name}")
        for field in ("references", "includePlatforms", "excludePlatforms", "optionalUnityReferences", "defineConstraints"):
            values = definition.get(field, [])
            if not isinstance(values, list) or any(not isinstance(v, str) for v in values):
                errors.append(f"{name}.{field} must be a string array")
                definition[field] = []
        if definition.get("includePlatforms") and definition.get("excludePlatforms"):
            errors.append(f"{name}: includePlatforms and excludePlatforms are mutually exclusive")
        if len(set(definition.get("references", []))) != len(definition.get("references", [])):
            errors.append(f"Duplicate references in {name}")
        definitions[name] = definition
        path_names[path] = name
    if not definitions:
        errors.append("No Unity assembly definitions found")

    def resolve(reference, owner):
        if reference.startswith("GUID:"):
            asset = guid_assets.get(reference[5:])
            name = path_names.get(asset)
            if name is None:
                errors.append(f"Unknown assembly GUID: {owner} -> {reference}")
            return name
        if reference in definitions:
            return reference
        package = PACKAGE_ASSEMBLIES.get(reference)
        if package and package in dependencies:
            return None
        errors.append(f"Unavailable assembly reference: {owner} -> {reference}")
        return None

    def editor_only(definition):
        return definition.get("includePlatforms") == ["Editor"]

    def test_only(definition):
        return ("TestAssemblies" in definition.get("optionalUnityReferences", []) or
                "UNITY_INCLUDE_TESTS" in definition.get("defineConstraints", []))

    edges = {name: [] for name in definitions}
    for name, definition in definitions.items():
        if test_only(definition) and "com.unity.test-framework" not in dependencies:
            errors.append(f"Test assembly {name} requires com.unity.test-framework")
        for reference in definition.get("references", []):
            target = resolve(reference, name)
            if target is None:
                if reference == "UnityEditor.TestRunner" and not (editor_only(definition) or test_only(definition)):
                    errors.append(f"Runtime assembly {name} references Editor test runner")
                continue
            edges[name].append(target)
            if not editor_only(definition) and editor_only(definitions[target]) and not test_only(definition):
                errors.append(f"Runtime assembly {name} references Editor-only assembly {target}")
            if not test_only(definition) and test_only(definitions[target]):
                errors.append(f"Non-test assembly {name} references test assembly {target}")
            # A dependent assembly must be available on every platform its owner targets.
            # Test assemblies use Unity's test-build rules and are checked in the Editor.
            source_platforms = set(definition.get("includePlatforms", []))
            target_platforms = set(definitions[target].get("includePlatforms", []))
            target_excluded = set(definitions[target].get("excludePlatforms", []))
            source_excluded = set(definition.get("excludePlatforms", []))
            if not test_only(definition):
                incompatible = (source_platforms - target_platforms if source_platforms and target_platforms else set())
                incompatible |= source_platforms & target_excluded
                if not source_platforms and target_platforms:
                    incompatible.add("unrestricted owner / restricted dependency")
                if not source_platforms and target_excluded - source_excluded:
                    incompatible |= target_excluded - source_excluded
                if incompatible:
                    errors.append(f"Platform mismatch {name} -> {target}: {sorted(incompatible)}")

    visiting, visited = [], set()

    def visit(name):
        if name in visiting:
            errors.append("Assembly cycle: " + " -> ".join(visiting[visiting.index(name):] + [name]))
            return
        if name in visited:
            return
        visiting.append(name)
        for target in edges[name]:
            visit(target)
        visiting.pop()
        visited.add(name)

    for name in edges:
        visit(name)
    for path in sorted(assets.rglob("*.asmref")):
        reference = read_json(path).get("reference")
        if not isinstance(reference, str) or not reference:
            errors.append(f"Missing asmref reference: {path.relative_to(root)}")
        else:
            resolve(reference, str(path.relative_to(root)))
        if any(sibling.suffix == ".asmdef" for sibling in path.parent.iterdir()):
            errors.append(f"asmref and asmdef share a folder: {path.parent.relative_to(root)}")

    serialized_count = 0
    for pattern in ("*.unity", "*.prefab", "*.asset", "*.mat"):
        for path in assets.rglob(pattern):
            serialized_count += 1
            raw = path.read_bytes()
            if not raw.startswith(b"%YAML"):
                warnings.append(f"Cannot statically inspect non-text asset: {path.relative_to(root)}")
                continue
            for guid in set(ASSET_GUID.findall(raw.decode("utf-8"))):
                # Unity's built-in resources use a zero prefix; package GUIDs require
                # a resolved package cache/Editor, so unknowns must be reviewed there.
                if guid.startswith("0" * 16) or guid in guid_assets:
                    continue
                errors.append(f"Unresolved serialized asset GUID: {path.relative_to(root)} -> {guid}")
    if not list(assets.rglob("*.unity")):
        warnings.append("No committed scenes: run the Editor generator before Play Mode/build")
    return {"errors": errors, "warnings": warnings, "assemblies": edges,
            "asmref_count": len(list(assets.rglob("*.asmref"))),
            "serialized_assets_checked": serialized_count,
            "scope": "Static project-local references only; Unity API/import/fileID validation not performed"}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("root", nargs="?", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--json", type=Path, help="Write evidence for CI artifacts")
    args = parser.parse_args()
    result = audit(args.root.resolve())
    if args.json:
        args.json.parent.mkdir(parents=True, exist_ok=True)
        args.json.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    for warning in result["warnings"]:
        print("NOTE: " + warning)
    for error in result["errors"]:
        print("ERROR: " + error, file=sys.stderr)
    print(f"{'FAIL' if result['errors'] else 'PASS'}: {len(result['assemblies'])} assembly definitions, "
          f"{result['asmref_count']} assembly references, {len(result['errors'])} errors")
    return int(bool(result["errors"]))


if __name__ == "__main__":
    sys.exit(main())
