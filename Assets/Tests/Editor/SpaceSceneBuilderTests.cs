using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
    [PrebuildSetup(typeof(GeneratorTestWorkspace)), PostBuildCleanup(typeof(GeneratorTestWorkspace))]
    public sealed class SpaceSceneBuilderTests
    {
        private const string Root = "Assets/LastOutpostSpaceGeneratorTestOutput";
        private const string Output = Root + "/Generated";
        private Scene original, workspace, unnamed;
        private Scene[] originals;
        private bool[] dirty;
        private EditorBuildSettingsScene[] build;
        private bool owns;
        private static string[] Snapshot(string path) => Directory.Exists(path) ? Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.Ordinal).Select(p => p + ":" + Convert.ToBase64String(SHA256.Create().ComputeHash(File.ReadAllBytes(p)))).ToArray() : Array.Empty<string>();

        [SetUp]
        public void Setup()
        {
            owns = false; unnamed = default; original = SceneManager.GetActiveScene(); build = EditorBuildSettings.scenes;
            originals = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
            dirty = originals.Select(s => s.isDirty).ToArray();
            Assert.That(Directory.Exists(Root), Is.False, "Never overwrite preexisting test output.");
            Assert.That(originals.Any(s => string.IsNullOrEmpty(s.path)), Is.False);
            AssetDatabase.CreateFolder("Assets", "LastOutpostSpaceGeneratorTestOutput"); owns = true;
            workspace = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            Assert.That(EditorSceneManager.SaveScene(workspace, Root + "/Workspace.unity"), Is.True);
            SceneManager.SetActiveScene(workspace);
        }

        [TearDown]
        public void Cleanup()
        {
            EditorBuildSettings.scenes = build;
            if (!owns) return;
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.path.StartsWith(Root + "/", StringComparison.Ordinal) || (unnamed.IsValid() && s.handle == unnamed.handle))
                    EditorSceneManager.CloseScene(s, true); // Only this fixture's owned scenes.
            }
            AssetDatabase.DeleteAsset(Root);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(originals.Length));
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(original.handle));
            for (int i = 0; i < originals.Length; i++)
            { Assert.That(SceneManager.GetSceneAt(i).handle, Is.EqualTo(originals[i].handle)); Assert.That(originals[i].isDirty, Is.EqualTo(dirty[i])); }
        }

        [Test]
        public void GeneratedSpaceSceneBindsAllAdaptersAndDoesNotChangeLegacyAssets()
        {
            string[] legacy = Snapshot(PrototypeSceneBuilder.OutputRoot);
            string path = SpaceSceneBuilder.Generate(Output);
            Assert.That(SceneManager.GetActiveScene().handle, Is.EqualTo(workspace.handle));
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            GameObject root = scene.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            foreach (Component c in new Component[] { root.GetComponent<PlatformMotor>(), root.GetComponent<EnemySpawner>(),
                root.GetComponent<CombatFeedback>(), root.GetComponentInChildren<CameraFollow>() })
                Assert.That(new SerializedObject(c).FindProperty("orientation").intValue, Is.EqualTo((int)RouteOrientation.SpaceForwardY));
            ShipAssemblyView ship = root.GetComponentInChildren<ShipAssemblyView>();
            foreach (string field in new[] { "session", "settings", "sprite", "material", "ownedRoot" })
                Assert.That(new SerializedObject(ship).FindProperty(field).objectReferenceValue, Is.Not.Null, field);
            Assert.That(ship.transform.Find("Horizontal attachment mounts").childCount, Is.EqualTo(5)); // Rail + four stable mounts.
            Assert.That(root.GetComponentInChildren<ProgressionHost>(), Is.Not.Null);
            Assert.That(root.GetComponentInChildren<ShipCameraFraming>(), Is.Not.Null);
            Assert.That(root.GetComponent<PrototypeHud>(), Is.Not.Null);
            Assert.That(new SerializedObject(root.GetComponent<PrototypeHud>()).FindProperty("levelTitle").stringValue, Is.EqualTo("Space Foundation"));
            Camera camera = root.GetComponentInChildren<Camera>();
            Assert.That(camera.orthographic, Is.True); Assert.That(camera.transform.position.x, Is.Zero); Assert.That(camera.transform.position.y, Is.Zero);
            Assert.That(EditorBuildSettings.scenes.Count(s => s.path == path && s.enabled), Is.EqualTo(1));
            Assert.That(Snapshot(PrototypeSceneBuilder.OutputRoot), Is.EqualTo(legacy));
        }

        [Test]
        public void RepeatedGenerationPreservesSettingsGuidsUnrelatedRootsAndBuildEntries()
        {
            string path = SpaceSceneBuilder.Generate(Output);
            string sceneGuid = AssetDatabase.AssetPathToGUID(path);
            string prefabGuid = AssetDatabase.AssetPathToGUID(Output + "/Prefabs/ShipCore.prefab");
            var settings = AssetDatabase.LoadAssetAtPath<SpaceShipSettings>(Output + "/Configuration/SpaceShipSettings.asset");
            settings.gap = 0.2f; EditorUtility.SetDirty(settings); AssetDatabase.SaveAssetIfDirty(settings);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene); new GameObject("Preserved unrelated root"); EditorSceneManager.SaveScene(scene);
            SpaceSceneBuilder.Generate(Output, true, null, s => true);
            Assert.That(scene.GetRootGameObjects().Count(g => g.GetComponent<GeneratedLevelRoot>() != null), Is.EqualTo(1));
            Assert.That(scene.GetRootGameObjects().Any(g => g.name == "Preserved unrelated root"), Is.True);
            Assert.That(settings.gap, Is.EqualTo(0.2f)); Assert.That(AssetDatabase.AssetPathToGUID(path), Is.EqualTo(sceneGuid));
            Assert.That(AssetDatabase.AssetPathToGUID(Output + "/Prefabs/ShipCore.prefab"), Is.EqualTo(prefabGuid));
            Assert.That(EditorBuildSettings.scenes.Count(s => s.path == path), Is.EqualTo(1));
            foreach (var entry in build) Assert.That(EditorBuildSettings.scenes.Any(s => s.path == entry.path && s.enabled == entry.enabled), Is.True);
        }

        [TestCase(false)] [TestCase(true)]
        public void UntitledSceneIsRejectedBeforeAnySpaceWrites(bool modified)
        {
            unnamed = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            if (modified) { SceneManager.SetActiveScene(unnamed); new GameObject("Owned unsaved probe"); EditorSceneManager.MarkSceneDirty(unnamed); }
            string[] before = Snapshot(Root);
            Assert.Throws<InvalidOperationException>(() => SpaceSceneBuilder.Generate(Output));
            Assert.That(Snapshot(Root), Is.EqualTo(before)); Assert.That(unnamed.isLoaded, Is.True);
        }

        [Test]
        public void DirtyReplacementCancellationIsReadOnly()
        {
            string path = SpaceSceneBuilder.Generate(Output);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            GameObject root = scene.GetRootGameObjects().Single(); root.name = "Keep my space edits"; EditorSceneManager.MarkSceneDirty(scene);
            string[] before = Snapshot(Root);
            Assert.Throws<OperationCanceledException>(() => SpaceSceneBuilder.Generate(Output, true, null, s => false));
            Assert.That(root.name, Is.EqualTo("Keep my space edits")); Assert.That(scene.isDirty, Is.True);
            Assert.That(Snapshot(Root), Is.EqualTo(before));
        }

        [TestCase(false)] [TestCase(true)]
        public void ReplacementSaveFailureRollsBackHierarchyAndDisk(bool throws)
        {
            string path = SpaceSceneBuilder.Generate(Output);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            GameObject root = scene.GetRootGameObjects().Single(); root.name = "Previous space root"; EditorSceneManager.SaveScene(scene);
            string[] before = Snapshot(Root);
            Assert.Throws<InvalidOperationException>(() => SpaceSceneBuilder.Generate(Output, true,
                (s, p) => throws ? throw new InvalidOperationException("Injected") : false, s => true));
            Assert.That(scene.GetRootGameObjects().Single().name, Is.EqualTo("Previous space root"));
            Assert.That(Snapshot(Root), Is.EqualTo(before));
        }

        [TestCase(false)] [TestCase(true)]
        public void FirstSaveFailureCleansOwnedSpaceAssetsAndTemporaryScene(bool throws)
        {
            int count = SceneManager.sceneCount;
            Assert.Throws<InvalidOperationException>(() => SpaceSceneBuilder.Generate(Output, true,
                (s, p) => throws ? throw new InvalidOperationException("Injected") : false));
            Assert.That(Directory.Exists(Output), Is.False); Assert.That(File.Exists(Output + ".meta"), Is.False);
            Assert.That(SceneManager.sceneCount, Is.EqualTo(count));
        }

        [TestCase("Assets/LastOutpostGenerated")] [TestCase("Assets/LastOutpostGenerated/NestedSpace")]
        [TestCase("Assets/lastoutpostgenerated")] [TestCase("Assets/./LastOutpostGenerated")]
        [TestCase("Assets//LastOutpostGenerated")] [TestCase("Assets/LastOutpostGenerated.")]
        public void SpaceCannotTargetLegacyOutput(string path)
        { string[] before = Snapshot(PrototypeSceneBuilder.OutputRoot); Assert.Throws<ArgumentException>(() => SpaceSceneBuilder.Generate(path)); Assert.That(Snapshot(PrototypeSceneBuilder.OutputRoot), Is.EqualTo(before)); }

        [Test]
        public void InvalidSavedLayoutDoesNotReplaceTheExistingSceneOrPrefabs()
        {
            SpaceSceneBuilder.Generate(Output);
            var settings = AssetDatabase.LoadAssetAtPath<SpaceShipSettings>(Output + "/Configuration/SpaceShipSettings.asset");
            settings.widthLimit = 1; EditorUtility.SetDirty(settings); AssetDatabase.SaveAssetIfDirty(settings);
            string[] before = Snapshot(Root);
            Assert.Throws<ArgumentException>(() => SpaceSceneBuilder.Generate(Output));
            Assert.That(settings.widthLimit, Is.EqualTo(1)); Assert.That(Snapshot(Root), Is.EqualTo(before));
        }
    }
}
