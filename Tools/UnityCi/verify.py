#!/usr/bin/env python3
"""Fail closed on missing Unity infrastructure/evidence; never manufacture results.

Only credential presence is inspected. No credential value is returned, logged,
archived or passed on the command line. Unity still performs actual activation.
"""
import argparse
import json
import os
from pathlib import Path
import platform
import re
import sys
import xml.etree.ElementTree as ET
import zipfile

UNITY_VERSION = "6000.3.0f1"
SCENE = "Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity"
GENERATED = "Assets/LastOutpostGenerated"


def licensing_mode(environment):
    present = lambda name: bool(environment.get(name, "").strip())
    explicit = [mode for mode, key in (("file", "UNITY_LICENSE"), ("serial", "UNITY_SERIAL"),
                                      ("floating", "UNITY_LICENSE_SERVER")) if present(key)]
    if len(explicit) > 1:
        raise ValueError("Configure one licensing method: file, serial, floating, or Personal account")
    mode = explicit[0] if explicit else "personal"
    if mode in ("serial", "personal") and not (present("UNITY_EMAIL") and present("UNITY_PASSWORD")):
        raise ValueError("Unity activation is unconfigured: set UNITY_EMAIL and UNITY_PASSWORD in GitHub Actions secrets; "
                         "add UNITY_SERIAL only for a Pro serial seat, or use a supported file/floating license")
    return mode


def preflight(root, environment):
    version_file = root / "ProjectSettings/ProjectVersion.txt"
    match = re.search(r"^m_EditorVersion:\s*(\S+)\s*$", version_file.read_text(), re.MULTILINE)
    if not match or match[1] != UNITY_VERSION:
        raise ValueError(f"Project Editor must remain {UNITY_VERSION}; no automatic version substitution")
    mode = licensing_mode(environment)
    return {"classification": "INFRASTRUCTURE_CONFIGURED_NOT_ACTIVATED", "unity_version": UNITY_VERSION,
            "licensing_method": mode, "unity_editor_executed": False,
            "note": "Presence check only. GameCI must activate Unity and execute subsequent stages."}


def test_results(path, minimum):
    """Count executed NUnit leaf cases, rather than discoveries/suite aggregates."""
    root = ET.parse(path).getroot()
    if root.tag != "test-run":
        raise ValueError("Expected Unity Test Framework NUnit test-run XML")
    counts = {"passed": 0, "failed": 0, "skipped": 0, "inconclusive": 0}
    mapping = {"Passed": "passed", "Failed": "failed", "Skipped": "skipped", "Inconclusive": "inconclusive"}
    for case in root.iter("test-case"):
        result = case.get("result")
        if result not in mapping:
            raise ValueError(f"Unexpected test-case result: {result}")
        counts[mapping[result]] += 1
    counts["executed"] = counts["passed"] + counts["failed"] + counts["inconclusive"]
    if root.get("result") != "Passed" or counts["failed"] or counts["skipped"] or counts["inconclusive"]:
        raise ValueError(f"Unity suite did not pass completely: {counts}")
    if counts["executed"] < minimum:
        raise ValueError(f"Only {counts['executed']} Unity cases executed; expected at least {minimum}")
    return counts


def generated_project(root, archive):
    # Validate GUIDs and package lock content using the independent Task 003 audit.
    import importlib.util
    spec = importlib.util.spec_from_file_location("assembly_audit", Path(__file__).resolve().parents[1] / "validate_assemblies.py")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    audit = module.audit(root)
    if audit["errors"]:
        raise ValueError("Generated project reference audit failed: " + "; ".join(audit["errors"]))
    expected = [SCENE, "Packages/packages-lock.json", "ProjectSettings/EditorBuildSettings.asset",
                GENERATED + "/Configuration/PrototypeSettings.asset",
                *[GENERATED + "/Prefabs/" + name + ".prefab" for name in ("Platform", "Basic", "Fast", "Boss")]]
    for relative in expected:
        if not (root / relative).is_file():
            raise ValueError("Unity did not produce required output: " + relative)
    build_settings = (root / "ProjectSettings/EditorBuildSettings.asset").read_text()
    entries = re.findall(r"- enabled:\s*(\d+)\s+path:\s*([^\r\n]+)", build_settings)
    if [(enabled, path) for enabled, path in entries if path.strip() == SCENE] != [("1", SCENE)]:
        raise ValueError("Generated scene must appear exactly once and enabled in build settings")
    archive.parent.mkdir(parents=True, exist_ok=True)
    # An explicit allowlist excludes Library, activation state, credentials and keystores.
    paths = [p for p in (root / GENERATED).rglob("*") if p.is_file()]
    if any(p.suffix not in (".meta", ".unity", ".prefab", ".asset", ".mat", ".png") for p in paths):
        raise ValueError("Unexpected file in generated output; review before archiving")
    paths += [root / (GENERATED + ".meta"), root / "Packages/packages-lock.json"]
    paths += [p for p in (root / "ProjectSettings").iterdir() if p.is_file()]
    with zipfile.ZipFile(archive, "w", compression=zipfile.ZIP_DEFLATED) as output:
        for path in sorted(paths):
            if not path.is_file():
                raise ValueError("Missing generated metadata: " + str(path.relative_to(root)))
            output.write(path, path.relative_to(root).as_posix())
    return {"scene": SCENE, "archive": archive.name, "files": len(paths),
            "serialized_assets_checked": audit["serialized_assets_checked"]}


def prepare_android(root):
    settings = root / "ProjectSettings/ProjectSettings.asset"
    content = settings.read_text()
    checks = {
        "portrait": r"^  defaultScreenOrientation: 1$",
        "application identifier": r"^    Android: com\.lastoutpost\.prototype$",
        "minimum API 26": r"^  AndroidMinSdkVersion: 26$",
        "ARM64": r"^  AndroidTargetArchitectures: 2$",
        "IL2CPP": r"^  scriptingBackend:\s*\n    Android: 1$",
        "version 0.1.0": r"^  bundleVersion: 0\.1\.0$",
    }
    for name, pattern in checks.items():
        if not re.search(pattern, content, re.MULTILINE):
            raise ValueError("Android setting requires Editor review: " + name)
    if re.search(r"^  AndroidUseCustomKeystore: 1$", content, re.MULTILINE):
        raise ValueError("Development CI must not use a custom signing keystore")
    # GameCI's SDK bootstrap reads this scalar before Unity starts; it cannot
    # install `platforms;android-0`. Pin API 35 only in the disposable CI checkout.
    content, changed = re.subn(r"^  AndroidTargetSdkVersion: (?:0|35)$", "  AndroidTargetSdkVersion: 35", content, flags=re.MULTILINE)
    if changed != 1:
        raise ValueError("Expected automatic or API 35 target SDK; no implicit target upgrade")
    settings.write_text(content)
    return {"target_api": 35, "scope": "CI workspace only; committed target remains automatic",
            "backend": "IL2CPP", "architecture": "ARM64", "development": True, "version": "0.1.0"}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("stage", choices=("preflight", "tests", "generated", "android"))
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--xml", type=Path)
    parser.add_argument("--minimum", type=int, default=1)
    parser.add_argument("--archive", type=Path)
    args = parser.parse_args()
    result = {"stage": args.stage, "source_commit": os.environ.get("OUTPOST_SOURCE_SHA", "unknown"),
              "runner_os": platform.system(), "runner_image": os.environ.get("ImageOS", "unknown")}
    exit_code = 0
    try:
        if args.stage == "preflight":
            result.update(preflight(args.root, os.environ))
        elif args.stage == "tests":
            if args.xml is None or args.minimum < 1:
                raise ValueError("Provide --xml and a positive minimum executed test count")
            result.update(test_results(args.xml, args.minimum))
        elif args.stage == "generated":
            if args.archive is None:
                raise ValueError("Provide --archive")
            result.update(generated_project(args.root, args.archive))
        else:
            result.update(prepare_android(args.root))
        result["status"] = "passed"
    except (OSError, ValueError, ET.ParseError) as error:
        result.update(status="blocked" if args.stage == "preflight" else "failed", reason=str(error))
        if args.stage == "preflight":
            result.update(classification="CLOUD VERIFICATION BLOCKED", unity_editor_executed=False)
        exit_code = 1
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps(result, indent=2))
    if args.stage == "preflight" and os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(os.environ["GITHUB_STEP_SUMMARY"], "a") as summary:
            summary.write("### Unity infrastructure preflight\n\n" + result.get("classification", "") + "\n\n" +
                          result.get("reason", result.get("note", "")) + "\n\nNo Unity operation is proven by this preflight.\n")
    return exit_code


if __name__ == "__main__":
    sys.exit(main())
