using System;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace LastOutpost.Editor
{
    /// <summary>Separate output, shared safe generator transaction; never edits legacy assets.</summary>
    public static class SpaceSceneBuilder
    {
        public const string OutputRoot = "Assets/SpacePrototypeGenerated";
        public const string SceneRelativePath = "/Scenes/SpaceFoundation.unity";
        [MenuItem("Last Outpost/Generate Space Foundation")]
        public static void GenerateFromMenu()
        {
            try
            {
                PrototypeSceneBuilder.TryGenerateSpaceAndOpen(OutputRoot, scene => EditorUtility.DisplayDialog(
                    "Replace generated space scene?", "Replace generated content and save this modified space scene? Other scenes remain open and unsaved.",
                    "Replace and save", "Cancel"));
            }
            catch (InvalidOperationException error) { EditorUtility.DisplayDialog("Space generation blocked", error.Message, "OK"); }
        }
        public static string Generate(string outputRoot = OutputRoot, bool registerBuildScene = true,
            Func<Scene, string, bool> saveScene = null, Func<Scene, bool> confirmReplacement = null) =>
            PrototypeSceneBuilder.GenerateSpace(outputRoot, registerBuildScene, saveScene, confirmReplacement);
        public static void GenerateBatch() => Generate();
    }
}
