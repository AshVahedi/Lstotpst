using System;
using System.IO;
using LastOutpost.Core.Progression;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    /// <summary>Only Unity lifetime/path ownership lives here. All transactions use the Core service.</summary>
    [DefaultExecutionOrder(-1000), DisallowMultipleComponent]
    public sealed class ProgressionHost : MonoBehaviour
    {
        public const string SaveFileName = "progression-v1.json";
        private static ProgressionHost instance;
        private ProgressionController controller;
        private IProgressionStore configuredStore;
        private OutpostSession driver;
        public ProgressionController Controller => controller;
        public string BootError { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry() => instance = null;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            transform.SetParent(null, false);
            DontDestroyOnLoad(gameObject);
            EnsureBoot();
        }

        public static ProgressionHost Resolve(ProgressionHost configured)
        {
            ProgressionHost owner = instance != null ? instance : configured;
            if (owner != null) owner.EnsureBoot();
            return owner;
        }

        public static FileProgressionStore CreateFileStore(string persistentDirectory)
        {
            if (string.IsNullOrWhiteSpace(persistentDirectory)) throw new ArgumentException("A persistent storage directory is required.");
            return new FileProgressionStore(Path.Combine(Path.GetFullPath(persistentDirectory), SaveFileName));
        }

        /// <summary>Inject a store on an inactive object before Awake; production uses persistentDataPath.</summary>
        public void ConfigureStore(IProgressionStore store)
        {
            if (controller != null) throw new InvalidOperationException("Cannot replace an initialized profile owner.");
            configuredStore = store ?? throw new ArgumentNullException(nameof(store));
        }

        public void EnsureBoot()
        {
            if (!Application.isPlaying || controller != null) return;
            try
            {
                controller = new ProgressionController(ProgressionConfiguration.Prototype(),
                    configuredStore ?? CreateFileStore(Application.persistentDataPath));
                controller.Initialize();
                BootError = null;
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException ||
                error is NotSupportedException || error is ArgumentException)
            { BootError = "Permanent storage could not initialize. Retry startup; original data was not reset."; }
        }

        public void ClaimDriver(OutpostSession client)
        {
            if (driver != null && driver != client) driver.ReleaseProgressionBinding();
            driver = client;
        }
        public void ReleaseDriver(OutpostSession client) { if (driver == client) driver = null; }
        public bool CanDrive(OutpostSession client) => isActiveAndEnabled && driver == client;

        private void OnDestroy()
        {
            if (instance != this) return;
            instance = null;
            controller?.Dispose();
            driver = null;
        }
    }
}
