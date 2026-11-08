"""Real, local Graphify CLI integration tests; fixtures never change game source."""
from __future__ import annotations

import json
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile
import unittest

TOOLS = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(TOOLS))
import update as workflow


class GraphifyWorkflowTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="last-outpost-graphify-test-")
        self.root = Path(self.temporary.name)
        for name in (".graphifyignore", ".graphifyrc"):
            shutil.copyfile(workflow.REPOSITORY / name, self.root / name)
        self.source = self.root / "Assets/Scripts/Probe.cs"
        self.source.parent.mkdir(parents=True)
        self.source.write_text("namespace MapTest { public sealed class Probe { public int Before() => 1; } }\n")
        self.caller = self.source.with_name("Caller.cs")
        self.caller.write_text("namespace MapTest { public sealed class Caller { public int Read() { return new Probe().Before(); } } }\n")

    def tearDown(self):
        self.temporary.cleanup()

    def run_update(self, full=False):
        # Suppress verbose child logs on success, retain complete diagnostics on failure.
        arguments = [sys.executable, str(TOOLS / "update.py"), "--root", str(self.root)]
        if full:
            arguments.append("--full")
        process = subprocess.run(arguments, env=workflow.tool_environment(), text=True, capture_output=True)
        self.assertEqual(process.returncode, 0, process.stdout + process.stderr)
        # A fresh process per root follows Graphify's stat-index cache contract.
        start = process.stdout.rfind("\n{\n") + 1
        return json.loads(process.stdout[start:])

    def assert_invalid_snapshot(self, message):
        process = subprocess.run([sys.executable, str(TOOLS / "update.py"), "--root", str(self.root), "--check"],
                                 env=workflow.tool_environment(), text=True, capture_output=True)
        self.assertNotEqual(process.returncode, 0, process.stdout)
        self.assertIn(message, process.stderr)

    def graph(self):
        return json.loads((self.root / "graphify-out/graph.json").read_text())

    def test_full_build_produces_artifacts_and_queryable_source_locations(self):
        result = self.run_update(full=True)
        self.assertEqual(result["csharp_files"], 2)
        for name in workflow.ARTIFACTS:
            self.assertGreater((self.root / "graphify-out" / name).stat().st_size, 0)
        query = subprocess.run([sys.executable, "-m", "graphify", "query", "Probe", "--budget", "500"],
                               cwd=self.root, env=workflow.tool_environment(), text=True, capture_output=True)
        self.assertEqual(query.returncode, 0, query.stderr)
        self.assertIn("Probe.cs", query.stdout)
        self.assertIn("loc=L", query.stdout)

    def test_ignore_policy_excludes_unity_outputs_venvs_caches_and_own_graph(self):
        directories = ("Library", "Temp", "Builds", "Build", "Obj", "Tests/bin", "Tests/obj",
                       ".venv-graphify", "graphify-out", "Assets/LastOutpostGenerated",
                       "Assets/LastOutpostGeneratorTestOutput", ".codex/skills/graphify")
        for directory in directories:
            path = self.root / directory / "Ignored.cs"
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_text("public sealed class MustNotBeIndexed {}\n")
        result = self.run_update(full=True)
        self.assertEqual(result["csharp_files"], 2)
        self.assertFalse(any("Ignored.cs" in node.get("source_file", "") for node in self.graph()["nodes"]))

    def test_incremental_edit_replaces_old_symbols_and_deletion_prunes_old_file(self):
        self.run_update(full=True)
        self.source.write_text(self.source.read_text().replace("Before", "After"))
        self.run_update()
        labels = {node.get("label", "") for node in self.graph()["nodes"] if node.get("source_file") == "Assets/Scripts/Probe.cs"}
        self.assertTrue(any("After" in label for label in labels))
        self.assertFalse(any("Before" in label for label in labels))
        self.caller.unlink()
        result = self.run_update()
        self.assertEqual(result["csharp_files"], 1)
        self.assertFalse(any(node.get("source_file") == "Assets/Scripts/Caller.cs" for node in self.graph()["nodes"]))

    def test_unchanged_warm_update_preserves_artifact_contents(self):
        self.run_update(full=True)
        self.run_update()  # Establish the native update path's bookkeeping after initial extract.
        output = self.root / "graphify-out"
        before = {name: (output / name).read_bytes() for name in workflow.ARTIFACTS}
        self.run_update()
        after = {name: (output / name).read_bytes() for name in workflow.ARTIFACTS}
        self.assertEqual(before, after)

    def test_body_change_with_same_symbols_is_stale_until_refreshed(self):
        self.run_update(full=True)
        self.source.write_text(self.source.read_text().replace("=> 1", "=> 2"))
        self.assert_invalid_snapshot("fingerprint")
        self.assertTrue(self.run_update()["fresh_inputs"])

    def test_dangling_edges_fail_validation(self):
        self.run_update(full=True)
        graph = self.graph()
        graph["links"][0]["target"] = "absent-node"
        (self.root / "graphify-out/graph.json").write_text(json.dumps(graph))
        self.assert_invalid_snapshot("dangling edge")

    def test_empty_graph_cannot_report_success(self):
        self.run_update(full=True)
        graph = self.graph()
        graph["nodes"], graph["links"] = [], []
        (self.root / "graphify-out/graph.json").write_text(json.dumps(graph))
        self.assert_invalid_snapshot("nonempty")


if __name__ == "__main__":
    unittest.main()
