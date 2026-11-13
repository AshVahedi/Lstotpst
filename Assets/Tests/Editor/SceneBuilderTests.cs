using System.Linq;
using System.IO;
using System.Collections.Generic;
using LastOutpost.Editor;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using LastOutpost.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LastOutpost.Tests.Editor
{
    /// <summary>
    /// UTF invokes prebuild setup after creating its own bootstrap scene and restores the
    /// user's scene setup after postbuild cleanup. Never save/close an arbitrary user scene.
    /// </summary>
    public sealed class GeneratorTestWorkspace : IPrebuildSetup, IPostBuildCleanup
    {
        private const string Root = "Assets/LastOutpostGeneratorTestWorkspace";
        private const string WorkspacePath = Root + "/Runner.unity";
        private static bool ownsWorkspace;

        public void Setup()
        {
            ownsWorkspace = false;
            Assert.That(Directory.Exists(Root), Is.False, "Preexisting runner workspace must be inspected, not overwritten.");
            Scene bootstrap = SceneManager.GetActiveScene();
            // This hook runs only within UTF's bootstrap lifecycle. Reject unexpected state
            // instead of saving a named/modified scene that could contain unrelated work.
            Assert.That(SceneManager.sceneCount, Is.EqualTo(1));
            Assert.That(bootstrap.path, Is.Empty);
            Assert.That(bootstrap.isDirty, Is.False);
            Assert.That(bootstrap.GetRootGameObjects().All(root => root.GetComponent<Camera>() != null ||
                root.GetComponent<Light>() != null), Is.True, "Unexpected content in the test runner bootstrap.");
            AssetDatabase.CreateFolder("Assets", "LastOutpostGeneratorTestWorkspace");
            ownsWorkspace = true;
            Assert.That(EditorSceneManager.SaveScene(bootstrap, WorkspacePath), Is.True);
        }

        public void Cleanup()
        {
            if (!ownsWorkspace) return;
            Scene bootstrap = SceneManager.GetSceneByPath(WorkspacePath);
            if (bootstrap.IsValid() && bootstrap.isLoaded)
            {
                Assert.That(SceneManager.sceneCount, Is.EqualTo(1), "Do not close unexpected additional scenes.");
                // The sole scene here is the runner-owned workspace, never the user's scene.
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            AssetDatabase.DeleteAsset(Root);
            ownsWorkspace = false;
        }
    }

    [PrebuildSetup(typeof(GeneratorTestWorkspace)), PostBuildCleanup(typeof(GeneratorTestWorkspace))]
    public sealed class SceneBuilderTests
    {
        private const string TestRoot = "Assets/LastOutpostGeneratorTestOutput";
        private EditorBuildSettingsScene[] originalBuildScenes;
        private bool ownsOutput;
        private Scene originalActive, workspace;
        private Scene[] originalScenes;
        private bool[] originalDirty;
        private readonly List<Scene> ownedScenes = new List<Scene>();

        [SetUp]
        public void SetUp()
        {
            originalBuildScenes = EditorBuildSettings.scenes;
            ownsOutput = false;
            ownedScenes.Clear();
            originalActive = SceneManager.GetActiveScene();
            originalScenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
            originalDirty = originalScenes.Select(scene => scene.isDirty).ToArray();
            Assert.That(AssetDatabase.IsValidFolder(TestRoot), Is.False,
                "Test output already exists; inspect it before removing or rerunning this fixture.");
            Assert.That(Directory.Exists(TestRoot), Is.False, "Do not overwrite an unimported preexisting output folder.");
            Assert.That(originalScenes.Any(scene => scene.isLoaded && string.IsNullOrEmpty(scene.path)), Is.False,
                "Save Untitled scenes manually before this fixture. Tests never save or close user scenes.");
            ownsOutput = true;
            AssetDatabase.CreateFolder("Assets", "LastOutpostGeneratorTestOutput");
            workspace = CreateOwnedScene("Workspace");
            SceneManager.SetActiveScene(workspace);
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = originalBuildScenes;
            if (!ownsOutput) return;
            // Keep every preexisting scene instance and its unsaved changes in memory. Only
            // fixture-owned scenes (including intentionally dirty/unnamed probes) can be closed.
            if (originalActive.IsValid() && originalActive.isLoaded) SceneManager.SetActiveScene(originalActive);
            try
            {
                for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (originalScenes.Any(original => original.handle == scene.handle)) continue;
                    if (scene.path.StartsWith(TestRoot + "/", System.StringComparison.Ordinal) ||
                        ownedScenes.Any(owned => owned.handle == scene.handle))
                        EditorSceneManager.CloseScene(scene, true);
                }
            }
            finally { AssetDatabase.DeleteAsset(TestRoot); }
            Assert.That(SceneManager.sceneCount, Is.EqualTo(originalScenes.Length));
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(originalActive.handle));
            for (int i = 0; i < originalScenes.Length; i++)
            {
                Assert.That(SceneManager.GetSceneAt(i).handle, Is.EqualTo(originalScenes[i].handle));
                Assert.That(originalScenes[i].isDirty, Is.EqualTo(originalDirty[i]), "User scene dirty state changed.");
            }
        }

        private Scene CreateOwnedScene(string savedName = null)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            ownedScenes.Add(scene);
            if (savedName != null) Assert.That(EditorSceneManager.SaveScene(scene, TestRoot + "/" + savedName + ".unity"), Is.True);
            return scene;
        }

        [Test]
        public void RegenerationPreservesSerializedReferencesSettingsGuidsAndUnrelatedRoots()
        {
            string scenePath = PrototypeSceneBuilder.Generate(TestRoot, false);
            string prefabGuid = AssetDatabase.AssetPathToGUID(TestRoot + "/Prefabs/Basic.prefab");
            string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            var settings = AssetDatabase.LoadAssetAtPath<PrototypeSettings>(TestRoot + "/Configuration/PrototypeSettings.asset");
            settings.ammunitionCost = 31;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssetIfDirty(settings);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            var unrelated = new GameObject("Preserve this unrelated root");
            SceneManager.MoveGameObjectToScene(unrelated, scene);
            EditorSceneManager.SaveScene(scene);
            PrototypeSceneBuilder.Generate(TestRoot, false);
            Assert.That(scene.GetRootGameObjects().Count(g => g.GetComponent<GeneratedLevelRoot>() != null), Is.EqualTo(1));
            Assert.That(scene.GetRootGameObjects().Contains(unrelated), Is.True);
            Assert.That(scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<OutpostSession>(true)).Count(), Is.EqualTo(1));
            Assert.That(AssetDatabase.FindAssets("t:Prefab", new[] { TestRoot }).Length, Is.EqualTo(4));
            Assert.That(AssetDatabase.AssetPathToGUID(TestRoot + "/Prefabs/Basic.prefab"), Is.EqualTo(prefabGuid));
            Assert.That(AssetDatabase.AssetPathToGUID(scenePath), Is.EqualTo(sceneGuid));
            Assert.That(settings.ammunitionCost, Is.EqualTo(31));
            // A live hierarchy alone cannot prove scene references were serialized correctly.
            EditorSceneManager.CloseScene(scene, true);
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            GameObject loadedRoot = scene.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            Assert.That(scene.GetRootGameObjects().Any(g => g.name == "Preserve this unrelated root"), Is.True);
            AssertReferences(loadedRoot.GetComponent<OutpostSession>(), "settings", "progressionHost");
            Assert.That(loadedRoot.GetComponentsInChildren<ProgressionHost>(true).Length, Is.EqualTo(1));
            AssertReferences(loadedRoot.GetComponent<PlatformMotor>(), "session", "vehicle");
            AssertReferences(loadedRoot.GetComponent<EnemySpawner>(), "session", "basicPrefab", "fastPrefab", "bossPrefab", "actorRoot");
            AssertReferences(loadedRoot.GetComponent<CombatFeedback>(), "session", "tracerMaterial", "effectRoot");
            AssertReferences(loadedRoot.GetComponent<PrototypeHud>(), "session");
            AssertReferences(loadedRoot.GetComponentInChildren<CameraFollow>(), "platform");
            AssertReferences(loadedRoot.GetComponentInChildren<HealthBarView>(), "session", "fill");
            Assert.That(new SerializedObject(loadedRoot.GetComponent<OutpostSession>()).FindProperty("settings").objectReferenceValue,
                Is.SameAs(settings));
            foreach (SpriteRenderer renderer in loadedRoot.GetComponentsInChildren<SpriteRenderer>())
            {
                Assert.That(renderer.sprite, Is.Not.Null);
                Assert.That(renderer.sharedMaterial, Is.Not.Null);
            }
        }

        [Test]
        public void BuildSceneRegistrationIsIdempotentAndPreservesExistingEntries()
        {
            string scenePath = PrototypeSceneBuilder.Generate(TestRoot, true);
            PrototypeSceneBuilder.Generate(TestRoot, true);
            Assert.That(EditorBuildSettings.scenes.Count(s => s.path == scenePath && s.enabled), Is.EqualTo(1));
            foreach (EditorBuildSettingsScene existing in originalBuildScenes)
                Assert.That(EditorBuildSettings.scenes.Any(s => s.path == existing.path && s.enabled == existing.enabled), Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FailedSaveRestoresPreviousLoadedHierarchyAndLeavesDiskUnchanged(bool throws)
        {
            string scenePath = PrototypeSceneBuilder.Generate(TestRoot, false);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            GameObject previous = scene.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            previous.name = "Previously generated root";
            var unrelated = new GameObject("Unrelated unsaved edit");
            SceneManager.MoveGameObjectToScene(unrelated, scene);
            EditorSceneManager.SaveScene(scene);
            // Verify an unsaved edit survives too; the replacement transaction must not revert it.
            previous.transform.position = new Vector3(3, 2, 0);
            EditorSceneManager.MarkSceneDirty(scene);
            byte[] before = File.ReadAllBytes(scenePath);
            var buildScenesBefore = EditorBuildSettings.scenes.Select(s => (s.path, s.enabled)).ToArray();

            Assert.Throws<System.InvalidOperationException>(() => PrototypeSceneBuilder.Generate(TestRoot, true,
                (candidate, path) => throws ? throw new System.InvalidOperationException("Injected save failure") : false,
                candidate => true)); // Explicit replacement consent for this fixture-owned dirty scene.

            GameObject restored = scene.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            Assert.That(restored.name, Is.EqualTo("Previously generated root"));
            Assert.That(restored.transform.position, Is.EqualTo(new Vector3(3, 2, 0)));
            Assert.That(scene.GetRootGameObjects().Contains(unrelated), Is.True);
            AssertReferences(restored.GetComponent<OutpostSession>(), "settings");
            Assert.That(File.ReadAllBytes(scenePath), Is.EqualTo(before));
            Assert.That(EditorBuildSettings.scenes.Select(s => (s.path, s.enabled)).ToArray(), Is.EqualTo(buildScenesBefore));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UntitledSceneIsRejectedBeforeAnyWritesAndRemainsOpen(bool modified)
        {
            Scene untitled = CreateOwnedScene();
            SceneManager.SetActiveScene(untitled);
            GameObject marker = null;
            if (modified)
            {
                marker = new GameObject("Unsaved fixture work");
                marker.transform.position = new Vector3(7, 3, 1);
                EditorSceneManager.MarkSceneDirty(untitled);
            }
            bool dirtyBefore = untitled.isDirty;
            string[] assetsBefore = SnapshotAssets();
            int sceneCount = SceneManager.sceneCount;
            var error = Assert.Throws<System.InvalidOperationException>(() => PrototypeSceneBuilder.Generate(TestRoot + "/Rejected"));
            StringAssert.Contains("Save each Untitled scene manually", error.Message);
            bool prompted = false;
            Assert.Throws<System.InvalidOperationException>(() => PrototypeSceneBuilder.TryGenerateAndOpen(TestRoot + "/Rejected",
                candidate => { prompted = true; return true; }));
            Assert.That(prompted, Is.False, "An unrelated scene must not receive a save/replacement prompt.");
            Assert.That(untitled.isLoaded, Is.True);
            Assert.That(untitled.path, Is.Empty);
            Assert.That(untitled.isDirty, Is.EqualTo(dirtyBefore));
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(untitled.handle));
            Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCount));
            if (modified) Assert.That(marker.transform.position, Is.EqualTo(new Vector3(7, 3, 1)));
            Assert.That(SnapshotAssets(), Is.EqualTo(assetsBefore));
            Assert.That(Directory.Exists(TestRoot + "/Rejected"), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SavedMultipleScenesKeepTheirEditsPathsAndActiveState(bool modified)
        {
            SceneManager.SetActiveScene(workspace);
            var marker = new GameObject("Keep this workspace root");
            Assert.That(EditorSceneManager.SaveScene(workspace), Is.True);
            Scene other = CreateOwnedScene("Other");
            SceneManager.SetActiveScene(workspace);
            if (modified)
            {
                marker.transform.position = new Vector3(9, 4, 2);
                EditorSceneManager.MarkSceneDirty(workspace);
            }
            byte[] workspaceBytes = File.ReadAllBytes(workspace.path);
            byte[] otherBytes = File.ReadAllBytes(other.path);
            int sceneCount = SceneManager.sceneCount;
            string scenePath = PrototypeSceneBuilder.Generate(TestRoot, false);
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(workspace.handle));
            Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCount), "Temporary generation scene leaked.");
            Assert.That(workspace.isLoaded && other.isLoaded, Is.True);
            Assert.That(workspace.isDirty, Is.EqualTo(modified));
            Assert.That(other.isDirty, Is.False);
            Assert.That(marker.transform.position, Is.EqualTo(modified ? new Vector3(9, 4, 2) : Vector3.zero));
            Assert.That(File.ReadAllBytes(workspace.path), Is.EqualTo(workspaceBytes));
            Assert.That(File.ReadAllBytes(other.path), Is.EqualTo(otherBytes));
            // The menu workflow opens additively; it must not close or save these scenes either.
            Assert.That(PrototypeSceneBuilder.TryGenerateAndOpen(TestRoot), Is.True);
            Assert.That(SceneManager.GetActiveScene().path, Is.EqualTo(scenePath));
            Assert.That(workspace.isLoaded && other.isLoaded, Is.True);
            Assert.That(workspace.isDirty, Is.EqualTo(modified));
            Assert.That(File.ReadAllBytes(workspace.path), Is.EqualTo(workspaceBytes));
            Assert.That(File.ReadAllBytes(other.path), Is.EqualTo(otherBytes));
        }

        [Test]
        public void CancellingDirtyGeneratedSceneReplacementLeavesAllAssetsAndEditsUnchanged()
        {
            string scenePath = PrototypeSceneBuilder.Generate(TestRoot, false);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            GameObject root = scene.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            root.transform.position = new Vector3(3, 5, 1);
            EditorSceneManager.MarkSceneDirty(scene);
            Scene active = SceneManager.GetActiveScene();
            string[] before = SnapshotAssets();
            int prompts = 0;
            Assert.That(PrototypeSceneBuilder.TryGenerateAndOpen(TestRoot, candidate =>
            {
                Assert.That(candidate.handle, Is.EqualTo(scene.handle));
                prompts++;
                return false;
            }), Is.False);
            Assert.That(prompts, Is.EqualTo(1));
            Assert.That(root.transform.position, Is.EqualTo(new Vector3(3, 5, 1)));
            Assert.That(scene.isDirty, Is.True);
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(active.handle));
            Assert.That(SnapshotAssets(), Is.EqualTo(before));
            Assert.That(EditorBuildSettings.scenes.Select(s => (s.path, s.enabled)),
                Is.EqualTo(originalBuildScenes.Select(s => (s.path, s.enabled))));
            Assert.Throws<System.OperationCanceledException>(() => PrototypeSceneBuilder.Generate(TestRoot));
            Assert.That(SnapshotAssets(), Is.EqualTo(before), "Noninteractive entry points require explicit consent too.");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FirstGenerationSaveFailureCleansOnlyNewOutputAndRestoresSceneState(bool throws)
        {
            string output = TestRoot + "/FailedFirstGeneration";
            int sceneCount = SceneManager.sceneCount;
            string[] before = SnapshotAssets();
            Assert.Throws<System.InvalidOperationException>(() => PrototypeSceneBuilder.Generate(output, true,
                (candidate, path) => throws ? throw new System.InvalidOperationException("Injected first-save failure") : false));
            Assert.That(Directory.Exists(output), Is.False);
            Assert.That(File.Exists(output + ".meta"), Is.False);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(sceneCount));
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(workspace.handle));
            Assert.That(SnapshotAssets(), Is.EqualTo(before));
        }

        private static string[] SnapshotAssets() => Directory.GetFiles(TestRoot, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, System.StringComparer.Ordinal)
            .Select(path => path + ":" + System.Convert.ToBase64String(File.ReadAllBytes(path))).ToArray();

        private static void AssertReferences(UnityEngine.Object component, params string[] fields)
        {
            Assert.That(component, Is.Not.Null);
            var serialized = new SerializedObject(component);
            foreach (string field in fields)
            {
                SerializedProperty property = serialized.FindProperty(field);
                Assert.That(property, Is.Not.Null, component.name + "." + field + " is not serialized");
                Assert.That(property.objectReferenceValue, Is.Not.Null, component.name + "." + field + " lost its scene/asset reference");
            }
        }
    }
}
