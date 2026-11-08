#!/usr/bin/env python3
"""Validate committed Unity metadata and the engine-independent core boundary.

This is a static project check, not a substitute for Unity importing/compiling assets.
"""
import json
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]


def main():
    errors = []
    assets = ROOT / "Assets"
    guids = {}
    asset_count = 0
    for path in sorted(assets.rglob("*")):
        if path.name.startswith(".") or path.suffix == ".meta":
            continue
        asset_count += 1
        metadata = Path(str(path) + ".meta")
        if not metadata.is_file():
            errors.append(f"Missing metadata: {path.relative_to(ROOT)}")
            continue
        content = metadata.read_text()
        match = re.search(r"^guid: ([0-9a-f]{32})$", content, re.MULTILINE)
        if not match:
            errors.append(f"Invalid GUID: {metadata.relative_to(ROOT)}")
        elif match[1] in guids:
            errors.append(f"Duplicate GUID: {metadata} and {guids[match[1]]}")
        else:
            guids[match[1]] = metadata
        if path.is_dir() and "folderAsset: yes" not in content:
            errors.append(f"Missing folder marker: {metadata}")
    for metadata in assets.rglob("*.meta"):
        if not Path(str(metadata)[:-5]).exists():
            errors.append(f"Orphan metadata: {metadata.relative_to(ROOT)}")

    core_sources = list((assets / "Scripts/Core").rglob("*.cs"))
    for source in core_sources:
        if re.search(r"\b(UnityEngine|UnityEditor|MonoBehaviour)\b", source.read_text()):
            errors.append(f"Unity dependency in core: {source.relative_to(ROOT)}")
    assembly = json.loads((assets / "Scripts/Core/LastOutpost.Core.asmdef").read_text())
    if assembly.get("noEngineReferences") is not True:
        errors.append("Core assembly must disable engine references")
    if assembly.get("references"):
        errors.append("Core assembly unexpectedly depends on another Unity assembly")
    tests = list((assets / "Tests/EditMode").glob("*Tests.cs"))
    if not tests:
        errors.append("No shared NUnit test sources found")
    test_assembly = json.loads((assets / "Tests/EditMode/LastOutpost.Core.EditModeTests.asmdef").read_text())
    if test_assembly.get("includePlatforms") != ["Editor"]:
        errors.append("Core test assembly must be Editor-only")
    if "LastOutpost.Core" not in test_assembly.get("references", []):
        errors.append("Core test assembly must reference the real core")
    if "TestAssemblies" not in test_assembly.get("optionalUnityReferences", []):
        errors.append("Core test assembly is not marked as a Unity test assembly")
    manifest = json.loads((ROOT / "Packages/manifest.json").read_text())
    if "com.unity.test-framework" not in manifest["dependencies"]:
        errors.append("Unity Test Framework dependency is missing")
    if errors:
        print("\n".join(errors), file=sys.stderr)
        return 1
    print(f"PASS: {asset_count} Unity assets/folders have unique metadata; "
          f"{len(core_sources)} core source files have no Unity dependencies; "
          f"{len(tests)} shared NUnit fixtures found.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
