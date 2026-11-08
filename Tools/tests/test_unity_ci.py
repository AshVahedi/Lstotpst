"""Exercise CI gates using fixtures, never call those fixtures Unity execution."""
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
import zipfile

SCRIPT = Path(__file__).resolve().parents[1] / "UnityCi/verify.py"
SPEC = importlib.util.spec_from_file_location("unity_ci", SCRIPT)
CI = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(CI)


class UnityCiTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.write("ProjectSettings/ProjectVersion.txt", "m_EditorVersion: 6000.3.0f1\n")

    def write(self, path, value):
        path = self.root / path
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(value)
        return path

    def xml(self, body, result="Passed"):
        return self.write("results.xml", f'<test-run result="{result}">{body}</test-run>')

    def generated(self):
        self.write("Packages/manifest.json", '{"dependencies":{}}')
        self.write("Packages/packages-lock.json", '{"dependencies":{}}')
        self.write("Assets/Core/Core.asmdef", '{"name":"Core"}')
        self.write(CI.GENERATED + ".meta", "guid: " + "a" * 32 + "\n")
        self.write(CI.SCENE, "%YAML 1.1\n--- !u!1 &1\nGameObject: {}\n")
        self.write("ProjectSettings/EditorBuildSettings.asset", "  m_Scenes:\n  - enabled: 1\n    path: " + CI.SCENE + "\n")
        self.write(CI.GENERATED + "/Configuration/PrototypeSettings.asset", "%YAML 1.1\n")
        for name in ("Platform", "Basic", "Fast", "Boss"):
            self.write(CI.GENERATED + "/Prefabs/" + name + ".prefab", "%YAML 1.1\n")

    def android_settings(self):
        return self.write("ProjectSettings/ProjectSettings.asset", """PlayerSettings:
  defaultScreenOrientation: 1
  applicationIdentifier:
    Android: com.lastoutpost.prototype
  AndroidMinSdkVersion: 26
  AndroidTargetSdkVersion: 0
  AndroidTargetArchitectures: 2
  AndroidUseCustomKeystore: 0
  bundleVersion: 0.1.0
  scriptingBackend:
    Android: 1
""")

    def test_supported_license_configurations(self):
        account = {"UNITY_EMAIL": "fixture-account", "UNITY_PASSWORD": "fixture-password"}
        for environment, mode in ((account, "personal"), ({**account, "UNITY_SERIAL": "fixture-serial"}, "serial"),
                                  ({"UNITY_LICENSE": "fixture-license"}, "file"),
                                  ({"UNITY_LICENSE_SERVER": "fixture-server"}, "floating")):
            with self.subTest(mode=mode):
                result = CI.preflight(self.root, environment)
                self.assertEqual(result["licensing_method"], mode)
                self.assertFalse(result["unity_editor_executed"])
                for value in environment.values():
                    self.assertNotIn(value, json.dumps(result))

    def test_empty_or_incomplete_account_is_blocked(self):
        for environment in ({}, {"UNITY_EMAIL": "fixture"}, {"UNITY_PASSWORD": "fixture"}, {"UNITY_SERIAL": "fixture"}):
            with self.subTest(keys=list(environment)):
                with self.assertRaisesRegex(ValueError, "unconfigured"):
                    CI.licensing_mode(environment)

    def test_ambiguous_specific_license_methods_rejected(self):
        with self.assertRaisesRegex(ValueError, "one licensing method"):
            CI.licensing_mode({"UNITY_SERIAL": "fixture", "UNITY_LICENSE": "fixture"})

    def test_editor_version_is_not_silently_substituted(self):
        self.write("ProjectSettings/ProjectVersion.txt", "m_EditorVersion: 6000.3.1f1\n")
        with self.assertRaisesRegex(ValueError, "no automatic version substitution"):
            CI.preflight(self.root, {"UNITY_LICENSE": "fixture"})

    def test_cli_emits_nonzero_blocked_evidence_without_credential_values(self):
        environment = {key: value for key, value in os.environ.items() if not key.startswith("UNITY_")}
        environment["UNITY_EMAIL"] = "PRIVATE_FIXTURE_SENTINEL"
        output = self.root / "preflight.json"
        result = subprocess.run([sys.executable, str(SCRIPT), "preflight", "--root", str(self.root), "--output", str(output)],
                                env=environment, capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        evidence = json.loads(output.read_text())
        self.assertEqual(evidence["classification"], "CLOUD VERIFICATION BLOCKED")
        self.assertFalse(evidence["unity_editor_executed"])
        self.assertNotIn("PRIVATE_FIXTURE_SENTINEL", result.stdout + result.stderr + output.read_text())

    def test_counts_executed_leaf_cases_not_reported_discoveries(self):
        path = self.xml('<test-suite total="999"><test-case result="Passed"/><test-case result="Passed"/></test-suite>')
        self.assertEqual(CI.test_results(path, 2), dict(passed=2, failed=0, skipped=0, inconclusive=0, executed=2))
        with self.assertRaisesRegex(ValueError, "Only 2"):
            CI.test_results(path, 3)

    def test_failed_skipped_or_inconclusive_cases_block_required_gate(self):
        for status in ("Failed", "Skipped", "Inconclusive"):
            with self.subTest(status=status):
                with self.assertRaisesRegex(ValueError, "did not pass completely"):
                    CI.test_results(self.xml(f'<test-case result="{status}"/>'), 1)

    def test_setup_failure_with_passing_leaf_is_not_a_pass(self):
        with self.assertRaisesRegex(ValueError, "did not pass completely"):
            CI.test_results(self.xml('<test-case result="Passed"/>', "Failed"), 1)

    def test_empty_discovered_only_report_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "Only 0"):
            CI.test_results(self.xml('<test-suite total="86"/>'), 1)

    def test_unrecognized_xml_or_result_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "Expected Unity"):
            CI.test_results(self.write("other.xml", "<testsuite/>"), 1)
        with self.assertRaisesRegex(ValueError, "Unexpected test-case"):
            CI.test_results(self.xml('<test-case result="Discovered"/>'), 1)

    def test_generated_archive_is_allowlisted_and_preserves_file_content(self):
        self.generated()
        self.write("Library/license.ulf", "PRIVATE_FIXTURE_SENTINEL")
        self.write("_activate-license~/UnityLicenseFile.ulf", "PRIVATE_FIXTURE_SENTINEL")
        archive = self.root / "TestResults/generated.zip"
        result = CI.generated_project(self.root, archive)
        self.assertEqual(result["scene"], CI.SCENE)
        with zipfile.ZipFile(archive) as zipped:
            self.assertIn(CI.SCENE, zipped.namelist())
            self.assertIn("Packages/packages-lock.json", zipped.namelist())
            self.assertEqual(zipped.read(CI.SCENE), (self.root / CI.SCENE).read_bytes())
            self.assertFalse(any("Library" in path or "activate-license" in path for path in zipped.namelist()))
        self.write(CI.GENERATED + "/never-archive.ulf", "PRIVATE_FIXTURE_SENTINEL")
        with self.assertRaisesRegex(ValueError, "Unexpected file"):
            CI.generated_project(self.root, archive)

    def test_missing_package_lock_does_not_become_generated_success(self):
        self.generated()
        (self.root / "Packages/packages-lock.json").unlink()
        with self.assertRaisesRegex(ValueError, "did not produce required output"):
            CI.generated_project(self.root, self.root / "generated.zip")

    def test_duplicate_or_disabled_generated_build_scene_is_rejected(self):
        self.generated()
        for entries in (("1", "1"), ("0",)):
            self.write("ProjectSettings/EditorBuildSettings.asset", "".join("  - enabled: " + flag + "\n    path: " + CI.SCENE + "\n" for flag in entries))
            with self.assertRaisesRegex(ValueError, "exactly once and enabled"):
                CI.generated_project(self.root, self.root / "generated.zip")

    def test_broken_serialized_reference_blocks_generated_evidence(self):
        self.generated()
        self.write(CI.SCENE, "%YAML 1.1\n m_Script: {guid: " + "b" * 32 + "}\n")
        with self.assertRaisesRegex(ValueError, "reference audit failed"):
            CI.generated_project(self.root, self.root / "generated.zip")

    def test_ci_target_sdk_pin_changes_only_target_scalar_and_is_repeatable(self):
        settings = self.android_settings()
        before = settings.read_text()
        result = CI.prepare_android(self.root)
        self.assertEqual(result["target_api"], 35)
        self.assertEqual(settings.read_text(), before.replace("AndroidTargetSdkVersion: 0", "AndroidTargetSdkVersion: 35"))
        CI.prepare_android(self.root)
        self.assertEqual(settings.read_text(), before.replace("AndroidTargetSdkVersion: 0", "AndroidTargetSdkVersion: 35"))

    def test_invalid_android_architecture_does_not_mutate_settings(self):
        settings = self.android_settings()
        before = settings.read_text().replace("AndroidTargetArchitectures: 2", "AndroidTargetArchitectures: 1")
        settings.write_text(before)
        with self.assertRaisesRegex(ValueError, "ARM64"):
            CI.prepare_android(self.root)
        self.assertEqual(settings.read_text(), before)

    def test_custom_signing_is_rejected(self):
        settings = self.android_settings()
        settings.write_text(settings.read_text().replace("AndroidUseCustomKeystore: 0", "AndroidUseCustomKeystore: 1"))
        with self.assertRaisesRegex(ValueError, "custom signing"):
            CI.prepare_android(self.root)


if __name__ == "__main__":
    unittest.main()
