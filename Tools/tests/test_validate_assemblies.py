"""Mutate small real project fixtures to exercise the independent assembly audit."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

SPEC = importlib.util.spec_from_file_location("validate_assemblies", Path(__file__).resolve().parents[1] / "validate_assemblies.py")
MODULE = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(MODULE)


class AssemblyAuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.write("Packages/manifest.json", {"dependencies": {"com.unity.test-framework": "1.4.5"}})
        self.assembly("Core", noEngineReferences=True)

    def write(self, relative, value):
        path = self.root / relative
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(value) if isinstance(value, dict) else value, encoding="utf-8")
        return path

    def assembly(self, name, folder=None, **fields):
        return self.write(f"Assets/{folder or name}/{name}.asmdef", {"name": name, **fields})

    def errors(self):
        return "\n".join(MODULE.audit(self.root)["errors"])

    def test_valid_layered_graph_and_editor_test_dependency(self):
        self.assembly("Runtime", references=["Core"])
        self.assembly("Editor", references=["Runtime"], includePlatforms=["Editor"])
        self.assembly("Tests", references=["Editor"], optionalUnityReferences=["TestAssemblies"])
        self.assertEqual(self.errors(), "")

    def test_missing_reference_rejected(self):
        self.assembly("Runtime", references=["Missing"])
        self.assertIn("Unavailable assembly reference", self.errors())

    def test_duplicate_name_rejected(self):
        self.assembly("Core", folder="Duplicate")
        self.assertIn("Duplicate assembly name", self.errors())

    def test_cycle_rejected(self):
        self.assembly("A", references=["B"])
        self.assembly("B", references=["C"])
        self.assembly("C", references=["A"])
        self.assertIn("Assembly cycle: A -> B -> C -> A", self.errors())

    def test_self_reference_rejected(self):
        self.assembly("Self", references=["Self"])
        self.assertIn("Assembly cycle: Self -> Self", self.errors())

    def test_runtime_cannot_depend_on_editor(self):
        self.assembly("Editor", includePlatforms=["Editor"])
        self.assembly("Runtime", references=["Editor"])
        self.assertIn("references Editor-only", self.errors())

    def test_runtime_cannot_depend_on_tests(self):
        self.assembly("Tests", optionalUnityReferences=["TestAssemblies"])
        self.assembly("Runtime", references=["Tests"])
        self.assertIn("references test assembly", self.errors())

    def test_guid_references_and_asmref_resolve(self):
        self.write("Assets/Core/Core.asmdef.meta", "guid: " + "a" * 32 + "\n")
        self.assembly("Runtime", references=["GUID:" + "a" * 32])
        self.write("Assets/Shared/Shared.asmref", {"reference": "GUID:" + "a" * 32})
        self.assertEqual(self.errors(), "")
        self.assertEqual(MODULE.audit(self.root)["assemblies"]["Runtime"], ["Core"])

    def test_missing_guid_rejected(self):
        self.assembly("Runtime", references=["GUID:" + "b" * 32])
        self.assertIn("Unknown assembly GUID", self.errors())

    def test_asmref_cannot_share_folder_with_asmdef(self):
        self.write("Assets/Core/Bad.asmref", {"reference": "Core"})
        self.assertIn("asmref and asmdef share", self.errors())

    def test_malformed_asmref_rejected(self):
        self.write("Assets/Shared/Bad.asmref", {"reference": 42})
        self.assertIn("Missing asmref reference", self.errors())

    def test_platform_lists_mutually_exclusive(self):
        self.assembly("Bad", includePlatforms=["Editor"], excludePlatforms=["Android"])
        self.assertIn("mutually exclusive", self.errors())

    def test_android_cannot_depend_on_editor_or_excluded_android(self):
        self.assembly("Desktop", excludePlatforms=["Android"])
        self.assembly("Android", references=["Desktop"], includePlatforms=["Android"])
        self.assertIn("Platform mismatch Android -> Desktop", self.errors())

    def test_invalid_json_and_reference_schema_reported(self):
        self.write("Assets/Bad/Bad.asmdef", "{")
        self.assembly("Runtime", references="Core")
        self.assertIn("Bad.asmdef", self.errors())
        self.assertIn("references must be a string array", self.errors())

    def test_duplicate_references_rejected(self):
        self.assembly("Runtime", references=["Core", "Core"])
        self.assertIn("Duplicate references", self.errors())

    def test_test_runner_requires_manifest_package(self):
        self.assembly("Tests", references=["UnityEngine.TestRunner"], optionalUnityReferences=["TestAssemblies"])
        self.assertEqual(self.errors(), "")
        self.write("Packages/manifest.json", {"dependencies": {}})
        self.assertIn("requires com.unity.test-framework", self.errors())

    def test_runtime_editor_runner_reference_rejected(self):
        self.assembly("Runtime", references=["UnityEditor.TestRunner"])
        self.assertIn("references Editor test runner", self.errors())

    def test_package_lock_version_mismatch_rejected(self):
        self.write("Packages/packages-lock.json", {"dependencies": {"com.unity.test-framework": {"version": "1.4.5"}}})
        self.assertEqual(self.errors(), "")
        self.write("Packages/packages-lock.json", {"dependencies": {"com.unity.test-framework": {"version": "1.4.4"}}})
        self.assertIn("Package lock does not match", self.errors())
        self.write("Packages/packages-lock.json", {"dependencies": {"com.unity.test-framework": None}})
        self.assertIn("Package lock does not match", self.errors())

    def test_missing_lock_is_explicit_limitation_not_fake_failure(self):
        self.assertEqual(self.errors(), "")
        self.assertTrue(any("No package lockfile" in w for w in MODULE.audit(self.root)["warnings"]))

    def test_serialized_guid_resolves_or_reports_broken_reference(self):
        self.write("Assets/Actor.cs", "// fixture")
        self.write("Assets/Actor.cs.meta", "guid: " + "c" * 32 + "\n")
        self.write("Assets/Scene.unity", "%YAML 1.1\n m_Script: {fileID: 11500000, guid: " + "c" * 32 + ", type: 3}\n")
        self.assertEqual(self.errors(), "")
        self.write("Assets/Actor.cs.meta", "guid: " + "d" * 32 + "\n")
        self.assertIn("Unresolved serialized asset GUID", self.errors())

    def test_duplicate_and_orphan_metadata_rejected(self):
        for name in ("Core/Core.asmdef", "Missing.cs"):
            self.write(f"Assets/{name}.meta", "guid: " + "e" * 32 + "\n")
        self.assertIn("Duplicate asset GUID", self.errors())
        self.assertIn("Orphan metadata", self.errors())

    def test_library_assemblies_are_excluded(self):
        self.write("Library/Bad.asmdef", {"name": "Core"})
        self.assertEqual(self.errors(), "")


if __name__ == "__main__":
    unittest.main()
