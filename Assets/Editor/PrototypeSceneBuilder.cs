using System;
using System.Collections.Generic;
using System.IO;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using LastOutpost.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastOutpost.Editor
{
    /// <summary>
    /// Owns assets below its output root and objects marked GeneratedLevelRoot. Reuses asset paths
    /// and GUIDs, preserves existing settings tuning and unrelated scene roots. Requires a real Editor.
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        public const string OutputRoot = "Assets/LastOutpostGenerated";
        public const string SceneRelativePath = "/Scenes/IndustrialWasteland.unity";

        [MenuItem("Last Outpost/Generate Level 1 Prototype")]
        public static void GenerateFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string scenePath = Generate();
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        /// <summary>Batch entry point: -executeMethod LastOutpost.Editor.PrototypeSceneBuilder.GenerateBatch.</summary>
        public static void GenerateBatch() => Generate();

        public static string Generate(string outputRoot = OutputRoot, bool registerBuildScene = true)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Generate the prototype outside Play Mode.");
            if (string.IsNullOrWhiteSpace(outputRoot) || !outputRoot.StartsWith("Assets/", StringComparison.Ordinal) ||
                outputRoot.Contains("..") || outputRoot.Contains("\\") || outputRoot.EndsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("Use a dedicated normalized folder below Assets.", nameof(outputRoot));
            EnsureFolders(outputRoot);
            PrototypeSettings settings = EnsureSettings(outputRoot + "/Configuration/PrototypeSettings.asset");
            settings.Build(); // Validate inspector values before replacing any generated scene content.
            Sprite sprite = EnsureSprite(outputRoot + "/Art/Placeholder.png");
            Material material = EnsureMaterial(outputRoot + "/Art/Placeholder.mat");
            GameObject platformPrefab = SavePlatform(outputRoot + "/Prefabs/Platform.prefab", sprite, material);
            EnemyActor basic = SaveEnemy(outputRoot + "/Prefabs/Basic.prefab", "Basic", sprite, material,
                new Color(0.9f, 0.3f, 0.2f), new Vector2(0.7f, 0.7f));
            EnemyActor fast = SaveEnemy(outputRoot + "/Prefabs/Fast.prefab", "Fast", sprite, material,
                new Color(1, 0.75f, 0.15f), new Vector2(0.55f, 0.55f));
            EnemyActor boss = SaveEnemy(outputRoot + "/Prefabs/Boss.prefab", "Boss", sprite, material,
                new Color(0.65f, 0.25f, 0.8f), new Vector2(1.8f, 1.2f));
            string scenePath = outputRoot + SceneRelativePath;
            BuildScene(scenePath, settings, sprite, material, platformPrefab, basic, fast, boss);
            if (registerBuildScene) RegisterBuildScene(scenePath);
            AssetDatabase.SaveAssets();
            return scenePath;
        }

        private static void EnsureFolders(string root)
        {
            foreach (string folder in new[] { "Configuration", "Art", "Prefabs", "Scenes" })
                Directory.CreateDirectory(root + "/" + folder);
            AssetDatabase.Refresh();
        }

        private static PrototypeSettings EnsureSettings(string path)
        {
            PrototypeSettings settings = AssetDatabase.LoadAssetAtPath<PrototypeSettings>(path);
            if (settings != null) return settings;
            if (File.Exists(path)) throw new InvalidOperationException("Generated settings path contains a different asset: " + path);
            settings = ScriptableObject.CreateInstance<PrototypeSettings>();
            AssetDatabase.CreateAsset(settings, path);
            return settings;
        }

        private static Sprite EnsureSprite(string path)
        {
            if (!File.Exists(path))
            {
                var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                try
                {
                    var pixels = new Color[64];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                    texture.SetPixels(pixels);
                    texture.Apply();
                    File.WriteAllBytes(path, texture.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Could not import generated placeholder texture.");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 8;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            return sprite != null ? sprite : throw new InvalidOperationException("Generated sprite import failed.");
        }

        private static Material EnsureMaterial(string path)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) throw new InvalidOperationException("Built-in Sprites/Default shader is required.");
            var material = new Material(shader) { name = "Outpost Placeholder" };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject SavePlatform(string path, Sprite sprite, Material material)
        {
            var root = new GameObject("Platform");
            try
            {
                Block("Armored body", root.transform, sprite, material, new Color(0.2f, 0.65f, 0.7f),
                    Vector3.zero, new Vector2(1.8f, 0.75f));
                Block("Tracks", root.transform, sprite, material, new Color(0.12f, 0.15f, 0.18f),
                    new Vector3(0, -0.4f, 0), new Vector2(1.95f, 0.25f));
                Block("Mounted Basic Gun", root.transform, sprite, material, Color.cyan,
                    new Vector3(0.55f, 0.55f, 0), new Vector2(0.8f, 0.16f));
                Block("Mounted Missile Launcher", root.transform, sprite, material, new Color(0.8f, 0.8f, 0.35f),
                    new Vector3(-0.5f, 0.55f, 0), new Vector2(0.6f, 0.35f));
                return SavePrefab(root, path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static EnemyActor SaveEnemy(string path, string name, Sprite sprite, Material material, Color color, Vector2 size)
        {
            var root = new GameObject(name);
            try
            {
                EnemyActor actor = root.AddComponent<EnemyActor>();
                GameObject body = Block("Body", root.transform, sprite, material, color, Vector3.zero, size);
                if (name == "Fast") body.transform.localRotation = Quaternion.Euler(0, 0, 45);
                if (name == "Boss") Block("Siege cannon", root.transform, sprite, material, Color.red,
                    new Vector3(-0.95f, 0.2f, 0), new Vector2(0.6f, 0.22f));
                AddHealthBar(root, null, actor, sprite, material, size.x, size.y / 2 + 0.3f);
                return SavePrefab(root, path).GetComponent<EnemyActor>();
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path, out bool success);
            return success && saved != null ? saved : throw new InvalidOperationException("Failed to save prefab: " + path);
        }

        private static void BuildScene(string path, PrototypeSettings settings, Sprite sprite, Material material,
            GameObject platformPrefab, EnemyActor basic, EnemyActor fast, EnemyActor boss)
        {
            Scene previousActive = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(path);
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (!alreadyLoaded) scene = File.Exists(path) ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) :
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject generated = null;
            try
            {
                SceneManager.SetActiveScene(scene);
                var oldRoots = new List<GameObject>();
                foreach (GameObject candidate in scene.GetRootGameObjects())
                    if (candidate.GetComponent<GeneratedLevelRoot>() != null) oldRoots.Add(candidate);
                generated = new GameObject("Last Outpost Prototype");
                SceneManager.MoveGameObjectToScene(generated, scene);
                generated.AddComponent<GeneratedLevelRoot>();
                OutpostSession session = generated.AddComponent<OutpostSession>();
                session.Configure(settings);
                GameObject platform = (GameObject)PrefabUtility.InstantiatePrefab(platformPrefab, scene);
                platform.transform.SetParent(generated.transform, false);
                generated.AddComponent<PlatformMotor>().Configure(session, platform.transform, Vector3.zero);
                AddHealthBar(platform, session, null, sprite, material, 1.8f, 1.05f);
                Transform enemies = Child("Enemies", generated.transform);
                generated.AddComponent<EnemySpawner>().Configure(session, basic, fast, boss, enemies, Vector3.zero);
                Transform effects = Child("Shot feedback", generated.transform);
                generated.AddComponent<CombatFeedback>().Configure(session, material, effects, Vector3.zero);
                generated.AddComponent<PrototypeHud>().Configure(session);
                CreateCamera(generated.transform, platform.transform);
                CreateScenery(generated.transform, sprite, material);
                // Replace owned content only after a complete new root has been assembled.
                foreach (GameObject oldRoot in oldRoots) UnityEngine.Object.DestroyImmediate(oldRoot);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException("Could not save generated scene: " + path);
            }
            catch
            {
                if (generated != null) UnityEngine.Object.DestroyImmediate(generated);
                throw;
            }
            finally
            {
                if (previousActive.IsValid() && previousActive.isLoaded) SceneManager.SetActiveScene(previousActive);
                if (!alreadyLoaded) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void RegisterBuildScene(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            for (int i = scenes.Count - 1; i >= 0; i--)
                if (scenes[i].path == path)
                {
                    if (found) scenes.RemoveAt(i);
                    else { scenes[i] = new EditorBuildSettingsScene(path, true); found = true; }
                }
            if (!found) scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static Transform Child(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        private static GameObject Block(string name, Transform parent, Sprite sprite, Material material, Color color,
            Vector3 position, Vector2 size, int sortingOrder = 0)
        {
            Transform child = Child(name, parent);
            child.localPosition = position;
            child.localScale = new Vector3(size.x, size.y, 1);
            var renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return child.gameObject;
        }

        private static void AddHealthBar(GameObject root, OutpostSession session, EnemyActor enemy, Sprite sprite,
            Material material, float width, float height)
        {
            Transform bar = Child("Health bar", root.transform);
            bar.localPosition = new Vector3(0, height, 0);
            Block("Background", bar, sprite, material, Color.black, Vector3.zero, new Vector2(width, 0.12f), 10);
            Transform fill = Block("Fill", bar, sprite, material, Color.green, Vector3.zero, new Vector2(width, 0.09f), 11).transform;
            root.AddComponent<HealthBarView>().Configure(session, enemy, fill, width);
        }

        private static void CreateCamera(Transform root, Transform platform)
        {
            Transform cameraTransform = Child("Main Camera", root);
            cameraTransform.tag = "MainCamera";
            cameraTransform.position = new Vector3(2.1f, 2, -10);
            var camera = cameraTransform.gameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.08f, 0.12f);
            cameraTransform.gameObject.AddComponent<CameraFollow>().Configure(platform);
        }

        private static void CreateScenery(Transform root, Sprite sprite, Material material)
        {
            Transform scenery = Child("Industrial Wasteland", root);
            Block("Route", scenery, sprite, material, new Color(0.24f, 0.26f, 0.28f),
                new Vector3(60, -0.62f, 0), new Vector2(140, 0.22f), -10);
            for (int i = 0; i < 14; i++)
                Block("Industrial silhouette " + i, scenery, sprite, material, new Color(0.1f, 0.15f, 0.19f),
                    new Vector3(i * 6, 0.2f, 0), new Vector2(1.3f, 2.5f + i % 3), -20);
        }
    }
}
