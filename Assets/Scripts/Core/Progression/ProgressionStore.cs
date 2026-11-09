using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

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

    /// <summary>Optional recovery capability. A token binds confirmation to unchanged raw data.</summary>
    public interface IProgressionRecoveryStore : IProgressionStore
    {
        string CaptureRecoveryToken();
        StoreCommitResult ResetWithBackup(string token, string replacement, out string backupId);
    }

    public sealed class MemoryProgressionStore : IProgressionRecoveryStore
    {
        private readonly object gate = new object();
        private string payload;
        private long generation;
        private readonly List<string> backups = new List<string>();
        public IReadOnlyList<string> Backups { get { lock (gate) return backups.AsReadOnly(); } }
        public MemoryProgressionStore(string initialPayload = null) => payload = initialPayload;
        public string Read() { lock (gate) return payload; }
        public StoreCommitResult Commit(string expected, string replacement)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            lock (gate)
            {
                if (!string.Equals(payload, expected, StringComparison.Ordinal)) return StoreCommitResult.Conflict;
                generation = checked(generation + 1);
                payload = replacement;
                return StoreCommitResult.Committed;
            }
        }

        public string CaptureRecoveryToken() { lock (gate) return generation.ToString(System.Globalization.CultureInfo.InvariantCulture); }
        public StoreCommitResult ResetWithBackup(string token, string replacement, out string backupId)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            lock (gate)
            {
                backupId = null;
                if (token != CaptureRecoveryToken()) return StoreCommitResult.Conflict;
                long next = checked(generation + 1);
                backups.Add(payload);
                backupId = "memory-backup-" + backups.Count;
                payload = replacement;
                generation = next;
                return StoreCommitResult.Committed;
            }
        }
    }

    /// <summary>
    /// Local, single-document storage. Writes a sibling .pending, flushes its contents to disk,
    /// then atomically replaces the primary (or renames on first save). No delete-and-copy fallback.
    /// Unity supplies a path below Application.persistentDataPath; this class never calls Unity APIs.
    /// </summary>
    public class FileProgressionStore : IProgressionRecoveryStore
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
                WriteAndPromote(replacement);
                return StoreCommitResult.Committed;
            }
        }

        private void WriteAndPromote(string replacement)
        {
            // Shared by normal commits and confirmed recovery; never a delete/copy fallback.
            using (var stream = new FileStream(PendingPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(replacement);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            PromotePending();
        }

        public string CaptureRecoveryToken() { using (Acquire()) return RawToken(); }

        private string RawToken()
        {
            using (var hash = SHA256.Create())
            {
                foreach (string path in new[] { SavePath, PendingPath })
                {
                    byte[] marker = { File.Exists(path) ? (byte)1 : (byte)0 };
                    hash.TransformBlock(marker, 0, marker.Length, marker, 0);
                    if (marker[0] == 0) continue;
                    using (var input = File.OpenRead(path))
                    {
                        byte[] length = BitConverter.GetBytes(input.Length);
                        hash.TransformBlock(length, 0, length.Length, length, 0);
                        var buffer = new byte[8192];
                        int count;
                        while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                            hash.TransformBlock(buffer, 0, count, buffer, 0);
                    }
                }
                hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return BitConverter.ToString(hash.Hash).Replace("-", "");
            }
        }

        public StoreCommitResult ResetWithBackup(string token, string replacement, out string backupId)
        {
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            backupId = null;
            using (Acquire())
            {
                if (!string.Equals(token, RawToken(), StringComparison.Ordinal)) return StoreCommitResult.Conflict;
                string backup = SavePath + ".recovery-" + Guid.NewGuid().ToString("N");
                Directory.CreateDirectory(backup);
                foreach (string path in new[] { SavePath, PendingPath })
                    if (File.Exists(path))
                        using (var input = File.OpenRead(path))
                        using (var output = new FileStream(Path.Combine(backup, Path.GetFileName(path)), FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        { input.CopyTo(output); output.Flush(true); }
                backupId = backup;
                WriteAndPromote(replacement); // Original bytes are backed up before replacement.
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
