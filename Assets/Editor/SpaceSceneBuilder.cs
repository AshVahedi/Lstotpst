using System;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using UnityEditor;
using UnityEngine;
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

        // Editor-only verification aid. Mutates transient emitters, never settings, inventory or
        // health/reward state. The ordinary combat loop must produce and persist the defeat.
        [MenuItem("Last Outpost/Verification/Disarm Selected Space Run")]
        public static void DisarmSelectedSpaceRun()
        {
            if (!CanDisarmSelectedSpaceRun()) throw new InvalidOperationException("Select a playing space session in the Hierarchy first.");
            OutpostSession session = Selection.activeGameObject.GetComponentInParent<OutpostSession>();
            foreach (var weapon in session.Run.Space.Weapons) weapon.Enabled = false;
            Debug.Log("Space verification: selected run disarmed. Ordinary enemy combat will determine the outcome; assets and ownership are unchanged.");
        }
        [MenuItem("Last Outpost/Verification/Disarm Selected Space Run", true)]
        public static bool CanDisarmSelectedSpaceRun() => Application.isPlaying && Selection.activeGameObject != null &&
            Selection.activeGameObject.GetComponentInParent<OutpostSession>()?.Run is GameRun run &&
            run.Space != null && run.Status == RunStatus.Playing;
    }
}
