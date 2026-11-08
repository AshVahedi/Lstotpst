using System;
using System.IO;
using System.Text;

namespace LastOutpost.Core.Progression
{
    public enum StoreCommitResult { Committed, Conflict }

    /// <summary>
    /// Compare-and-swap an entire document. Null means no committed save.
    /// Implementations must never partially write a committed document. After any I/O error,
    /// the service blocks writes until reload resolves the authoritative on-disk state.
    /// </summary>
    public interface IProgressionStore
    {
        string Read();
        StoreCommitResult Commit(string expected, string replacement);
    }

    public sealed class MemoryProgressionStore : IProgressionStore
    {
        private readonly object gate = new object();
        private string payload;
        public MemoryProgressionStore(string initialPayload = null) => payload = initialPayload;
        public string Read() { lock (gate) return payload; }
        public StoreCommitResult Commit(string expected, string replacement)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            lock (gate)
            {
                if (!string.Equals(payload, expected, StringComparison.Ordinal)) return StoreCommitResult.Conflict;
                payload = replacement;
                return StoreCommitResult.Committed;
            }
        }
    }

    /// <summary>
    /// Local, single-document storage. Writes a sibling .pending, flushes its contents to disk,
    /// then atomically replaces the primary (or renames on first save). No delete-and-copy fallback.
    /// Unity supplies a path below Application.persistentDataPath; this class never calls Unity APIs.
    /// </summary>
    public class FileProgressionStore : IProgressionStore
    {
        public string SavePath { get; }
        public string PendingPath => SavePath + ".pending";
        public bool HasPendingWrite => File.Exists(PendingPath);

        public FileProgressionStore(string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath)) throw new ArgumentException("A save file path is required.", nameof(savePath));
            SavePath = Path.GetFullPath(savePath);
        }

        private FileStream Acquire()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            // Keep the lock file: deleting it can let other processes lock a different inode.
            return new FileStream(SavePath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        private string ReadPrimary()
        {
            if (!File.Exists(SavePath))
            {
                if (HasPendingWrite) throw new ProgressionLoadException(ProgressionLoadError.InterruptedWrite,
                    "Only an uncommitted pending file exists. Preserve it and explicitly recover; do not initialize over it.");
                return null;
            }
            var info = new FileInfo(SavePath);
            if (info.Length > JsonProgressionCodec.MaximumCharacters * 4L)
                throw new ProgressionLoadException(ProgressionLoadError.Corrupt, "Save file is oversized; original preserved.");
            try { return File.ReadAllText(SavePath, new UTF8Encoding(false, true)); }
            catch (DecoderFallbackException error)
            {
                throw new ProgressionLoadException(ProgressionLoadError.Corrupt, "Save encoding is invalid; original bytes preserved.", error);
            }
        }

        public string Read()
        {
            using (Acquire()) return ReadPrimary();
        }

        public StoreCommitResult Commit(string expected, string replacement)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            using (Acquire())
            {
                if (!string.Equals(ReadPrimary(), expected, StringComparison.Ordinal)) return StoreCommitResult.Conflict;
                // A valid primary remains authoritative after interruption. Reuse only its stale
                // pending file on an explicit new transaction, never promote it during load.
                using (var stream = new FileStream(PendingPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(replacement);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                PromotePending(); // Linearization point. Nothing fallible follows except lock disposal.
                return StoreCommitResult.Committed;
            }
        }

        /// <summary>Must atomically publish or throw. Overridable for filesystem adapters/fault-injection tests.</summary>
        protected virtual void PromotePending()
        {
            if (File.Exists(SavePath)) File.Replace(PendingPath, SavePath, null);
            else File.Move(PendingPath, SavePath);
        }
    }
}
