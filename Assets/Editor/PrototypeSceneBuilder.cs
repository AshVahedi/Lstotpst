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
            try
            {
                TryGenerateAndOpen(OutputRoot, scene => EditorUtility.DisplayDialog("Replace generated scene content?",
                    "The generated scene has unsaved changes. Replace its generated objects and save this scene? " +
                    "Other open scenes will remain open and will not be saved.", "Replace and save", "Cancel"));
            }
            catch (InvalidOperationException error)
            {
                EditorUtility.DisplayDialog("Last Outpost generation blocked", error.Message, "OK");
            }
        }

        /// <summary>Cancellation is read-only; opening is additive so unrelated scene edits survive.</summary>
        public static bool TryGenerateAndOpen(string outputRoot = OutputRoot, Func<Scene, bool> confirmReplacement = null)
            => TryGenerateAndOpenVariant(false, outputRoot, confirmReplacement);

        internal static bool TryGenerateSpaceAndOpen(string outputRoot, Func<Scene, bool> confirmReplacement)
            => TryGenerateAndOpenVariant(true, outputRoot, confirmReplacement);

        private static bool TryGenerateAndOpenVariant(bool space, string outputRoot, Func<Scene, bool> confirmReplacement)
        {
            string scenePath;
            try { scenePath = GenerateVariant(space, outputRoot, true, null, confirmReplacement); }
            catch (OperationCanceledException) { return false; }
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            if (!scene.IsValid() || !scene.isLoaded) scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            SceneManager.SetActiveScene(scene); // Only the explicit menu command focuses the generated scene.
            return true;
        }

        /// <summary>Batch entry point: -executeMethod LastOutpost.Editor.PrototypeSceneBuilder.GenerateBatch.</summary>
        public static void GenerateBatch() => Generate();

        /// <param name="saveScene">Optional persistence callback for Editor automation; false or an exception rolls back the scene replacement.</param>
        public static string Generate(string outputRoot = OutputRoot, bool registerBuildScene = true,
            Func<Scene, string, bool> saveScene = null, Func<Scene, bool> confirmReplacement = null)
            => GenerateVariant(false, outputRoot, registerBuildScene, saveScene, confirmReplacement);

        internal static string GenerateSpace(string outputRoot, bool registerBuildScene,
            Func<Scene, string, bool> saveScene, Func<Scene, bool> confirmReplacement)
            => GenerateVariant(true, outputRoot, registerBuildScene, saveScene, confirmReplacement);

        private static string GenerateVariant(bool space, string outputRoot, bool registerBuildScene,
            Func<Scene, string, bool> saveScene, Func<Scene, bool> confirmReplacement)
        {
            if (Application.isPlaying) throw new InvalidOperationException("Generate the prototype outside Play Mode.");
            if (string.IsNullOrWhiteSpace(outputRoot) || !outputRoot.StartsWith("Assets/", StringComparison.Ordinal) ||
                outputRoot.Contains("..") || outputRoot.Contains("\\") || outputRoot.EndsWith("/", StringComparison.Ordinal))
                throw new ArgumentException("Use a dedicated normalized folder below Assets.", nameof(outputRoot));
            if (space)
            {
                // Reject filesystem aliases as well as ordinary legacy paths (Windows is case-insensitive).
                foreach (string segment in outputRoot.Split('/'))
                    if (segment.Length == 0 || segment != segment.TrimEnd(' ', '.'))
                        throw new ArgumentException("Use a normalized space output path without alias segments.", nameof(outputRoot));
                if (string.Equals(outputRoot, OutputRoot, StringComparison.OrdinalIgnoreCase) ||
                    outputRoot.StartsWith(OutputRoot + "/", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Space output must not overwrite the verified legacy prototype.", nameof(outputRoot));
            }
            string scenePath = outputRoot + (space ? SpaceSceneBuilder.SceneRelativePath : SceneRelativePath);
            ValidateSceneState(scenePath, confirmReplacement); // No folders, imports or asset writes before consent.
            bool ownsNewOutput = !Directory.Exists(outputRoot) && !File.Exists(outputRoot);
            try
            {
                EnsureFolders(outputRoot);
                PrototypeSettings settings = EnsureSettings(outputRoot + "/Configuration/PrototypeSettings.asset");
                settings.Build(); // Validate inspector values before replacing any generated scene content.
                SpaceShipSettings shipSettings = null;
                if (space)
                {
                    string shipPath = outputRoot + "/Configuration/SpaceShipSettings.asset";
                    shipSettings = AssetDatabase.LoadAssetAtPath<SpaceShipSettings>(shipPath);
                    if (shipSettings == null)
                    {
                        if (File.Exists(shipPath)) throw new InvalidOperationException("Space settings path contains an unrelated asset.");
                        shipSettings = ScriptableObject.CreateInstance<SpaceShipSettings>();
                        AssetDatabase.CreateAsset(shipSettings, shipPath);
                    }
                    shipSettings.Build();
                }
                Sprite sprite = EnsureSprite(outputRoot + "/Art/Placeholder.png");
                Material material = EnsureMaterial(outputRoot + "/Art/Placeholder.mat");
                GameObject platformPrefab = space ? SaveShipCore(outputRoot + "/Prefabs/ShipCore.prefab", sprite, material, shipSettings) :
                    SavePlatform(outputRoot + "/Prefabs/Platform.prefab", sprite, material);
                EnemyActor basic = SaveEnemy(outputRoot + "/Prefabs/Basic.prefab", "Basic", sprite, material,
                    new Color(0.9f, 0.3f, 0.2f), new Vector2(0.7f, 0.7f), space);
                EnemyActor fast = SaveEnemy(outputRoot + "/Prefabs/Fast.prefab", "Fast", sprite, material,
                    new Color(1, 0.75f, 0.15f), new Vector2(0.55f, 0.55f), space);
                EnemyActor boss = SaveEnemy(outputRoot + "/Prefabs/Boss.prefab", "Boss", sprite, material,
                    new Color(0.65f, 0.25f, 0.8f), new Vector2(1.8f, 1.2f), space);
                BuildScene(scenePath, settings, sprite, material, platformPrefab, basic, fast, boss,
                    saveScene ?? ((scene, path) => EditorSceneManager.SaveScene(scene, path)), shipSettings);
                if (registerBuildScene) RegisterBuildScene(scenePath);
                AssetDatabase.SaveAssetIfDirty(settings); // Never save unrelated dirty assets globally.
                if (shipSettings != null) AssetDatabase.SaveAssetIfDirty(shipSettings);
                return scenePath;
            }
            catch
            {
                // Never delete a preexisting output tree. BuildScene closes only scenes it opened;
                // a failed first generation can therefore remove its newly owned assets safely.
                if (ownsNewOutput) AssetDatabase.DeleteAsset(outputRoot);
                throw;
            }
        }

        private static void ValidateSceneState(string scenePath, Func<Scene, bool> confirmReplacement)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && string.IsNullOrEmpty(scene.path))
                    throw new InvalidOperationException("Save each Untitled scene manually with File > Save As before generating. " +
                        "No scenes have been closed or saved, and no prototype assets have been written.");
            }
            Scene target = SceneManager.GetSceneByPath(scenePath);
            if (target.IsValid() && target.isLoaded && target.isDirty &&
                (confirmReplacement == null || !confirmReplacement(target)))
                throw new OperationCanceledException("Generated scene replacement cancelled; unsaved changes are preserved.");
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

        private static GameObject SaveShipCore(string path, Sprite sprite, Material material, SpaceShipSettings settings)
        {
            var root = new GameObject("Ship Core");
            try
            {
                Block("Core hull", root.transform, sprite, material, new Color(0.2f, 0.65f, 0.8f),
                    Vector3.zero, new Vector2(settings.coreWidth, 1.6f), 2);
                Block("Forward bridge", root.transform, sprite, material, Color.cyan,
                    new Vector3(0, 0.65f, 0), new Vector2(settings.coreWidth * 0.5f, 0.5f), 3);
                Block("Engine", root.transform, sprite, material, new Color(1, 0.5f, 0.2f),
                    new Vector3(0, -0.95f, 0), new Vector2(settings.coreWidth * 0.6f, 0.3f), 3);
                return SavePrefab(root, path);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }

        private static EnemyActor SaveEnemy(string path, string name, Sprite sprite, Material material, Color color, Vector2 size, bool space = false)
        {
            var root = new GameObject(name);
            try
            {
                EnemyActor actor = root.AddComponent<EnemyActor>();
                GameObject body = Block("Body", root.transform, sprite, material, color, Vector3.zero, size);
                if (name == "Fast") body.transform.localRotation = Quaternion.Euler(0, 0, 45);
                if (name == "Boss") Block(space ? "Forward cannon" : "Siege cannon", root.transform, sprite, material, Color.red,
                    space ? new Vector3(0, -0.85f, 0) : new Vector3(-0.95f, 0.2f, 0),
                    space ? new Vector2(0.22f, 0.6f) : new Vector2(0.6f, 0.22f));
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
            GameObject platformPrefab, EnemyActor basic, EnemyActor fast, EnemyActor boss,
            Func<Scene, string, bool> saveScene, SpaceShipSettings shipSettings = null)
        {
            Scene previousActive = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(path);
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (!alreadyLoaded) scene = File.Exists(path) ? EditorSceneManager.OpenScene(path, OpenSceneMode.Additive) :
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            GameObject generated = null;
            int replacementUndoGroup = -1;
            try
            {
                SceneManager.SetActiveScene(scene);
                var oldRoots = new List<GameObject>();
                foreach (GameObject candidate in scene.GetRootGameObjects())
                    if (candidate.GetComponent<GeneratedLevelRoot>() != null) oldRoots.Add(candidate);
                bool space = shipSettings != null;
                RouteOrientation orientation = space ? RouteOrientation.SpaceForwardY : RouteOrientation.LegacyHorizontal;
                generated = new GameObject(space ? "Last Outpost Space Foundation" : "Last Outpost Prototype");
                generated.AddComponent<GeneratedLevelRoot>();
                ProgressionHost progression = Child("Permanent progression", generated.transform).gameObject.AddComponent<ProgressionHost>();
                OutpostSession session = generated.AddComponent<OutpostSession>();
                session.Configure(settings, progression);
                GameObject platform = (GameObject)PrefabUtility.InstantiatePrefab(platformPrefab, scene);
                platform.transform.SetParent(generated.transform, false);
                generated.AddComponent<PlatformMotor>().Configure(session, platform.transform, Vector3.zero, orientation);
                if (space) platform.AddComponent<ShipAssemblyView>().Configure(session, shipSettings, sprite, material);
                AddHealthBar(platform, session, null, sprite, material, 1.8f, 1.05f);
                Transform enemies = Child("Enemies", generated.transform);
                generated.AddComponent<EnemySpawner>().Configure(session, basic, fast, boss, enemies, Vector3.zero, orientation);
                Transform effects = Child("Shot feedback", generated.transform);
                generated.AddComponent<CombatFeedback>().Configure(session, material, effects, Vector3.zero, orientation);
                generated.AddComponent<PrototypeHud>().Configure(session, space ? "Space Foundation" : null,
                    space ? "Space foundation. Automatic forward travel and horizontal modules. Combat uses the legacy Level 1 harness." : null);
                CreateCamera(generated.transform, platform.transform, shipSettings);
                if (space) CreateSpaceScenery(generated.transform, sprite, material);
                else CreateScenery(generated.transform, sprite, material);
                // An isolated Undo group restores the previous hierarchy if persistence fails.
                // Do not roll back unrelated edits or earlier asset generation/import operations.
                Undo.IncrementCurrentGroup();
                replacementUndoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Replace Last Outpost prototype");
                Undo.RegisterCreatedObjectUndo(generated, "Create replacement prototype");
                foreach (GameObject oldRoot in oldRoots) Undo.DestroyObjectImmediate(oldRoot);
                EditorSceneManager.MarkSceneDirty(scene);
                if (!saveScene(scene, path)) throw new InvalidOperationException("Could not save generated scene: " + path);
                Undo.CollapseUndoOperations(replacementUndoGroup);
            }
            catch
            {
                if (replacementUndoGroup >= 0) Undo.RevertAllDownToGroup(replacementUndoGroup);
                if (generated != null) UnityEngine.Object.DestroyImmediate(generated);
                throw;
            }
            finally
            {
                if (replacementUndoGroup >= 0) Undo.IncrementCurrentGroup();
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

        private static void CreateCamera(Transform root, Transform platform, SpaceShipSettings space = null)
        {
            Transform cameraTransform = Child("Main Camera", root);
            cameraTransform.tag = "MainCamera";
            cameraTransform.position = space == null ? new Vector3(2.1f, 2, -10) : new Vector3(0, 0, -10);
            var camera = cameraTransform.gameObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.08f, 0.12f);
            cameraTransform.gameObject.AddComponent<CameraFollow>().Configure(platform, space == null ? 2.1f : 0,
                space == null ? RouteOrientation.LegacyHorizontal : RouteOrientation.SpaceForwardY);
            if (space != null) cameraTransform.gameObject.AddComponent<ShipCameraFraming>().Configure(space);
        }

        private static void CreateSpaceScenery(Transform root, Sprite sprite, Material material)
        {
            Transform stars = Child("Placeholder star field", root);
            // Deterministic world-space placeholders; never consumes Unity's global Random state.
            for (int i = 0; i < 90; i++)
                Block("Star " + i, stars, sprite, material, new Color(0.35f, 0.45f, 0.65f),
                    new Vector3((i * 17 % 19 - 9) * 0.7f, i * 1.7f - 12, 0), new Vector2(0.045f, 0.045f), -20);
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
