using System.Linq;
using System.IO;
using LastOutpost.Editor;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using LastOutpost.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastOutpost.Tests.Editor
{
    public sealed class SceneBuilderTests
    {
        private const string TestRoot = "Assets/LastOutpostGeneratorTestOutput";
        private EditorBuildSettingsScene[] originalBuildScenes;
        private bool ownsOutput;

        [SetUp]
        public void SetUp()
        {
            originalBuildScenes = EditorBuildSettings.scenes;
            ownsOutput = false;
            Assert.That(AssetDatabase.IsValidFolder(TestRoot), Is.False,
                "Test output already exists; inspect it before removing or rerunning this fixture.");
            Assert.That(Directory.Exists(TestRoot), Is.False, "Do not overwrite an unimported preexisting output folder.");
            ownsOutput = true;
        }

        [TearDown]
        public void TearDown()
        {
            EditorBuildSettings.scenes = originalBuildScenes;
            if (!ownsOutput) return;
            Scene scene = SceneManager.GetSceneByPath(TestRoot + PrototypeSceneBuilder.SceneRelativePath);
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.DeleteAsset(TestRoot);
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
            AssetDatabase.SaveAssets();
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
            AssertReferences(loadedRoot.GetComponent<OutpostSession>(), "settings");
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
