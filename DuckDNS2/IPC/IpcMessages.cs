using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DuckDNS2.Ipc
{
    /// <summary>Commands understood by the service over the named pipe.</summary>
    static class IpcCommands
    {
        /// <summary>Returns the current <see cref="UpdateStatus"/>.</summary>
        public const string Status = "STATUS";
        /// <summary>Triggers an immediate DNS update (asynchronous; poll STATUS for the result).</summary>
        public const string Update = "UPDATE";
        /// <summary>Re-reads the configuration file and triggers an update.</summary>
        public const string Reload = "RELOAD";
    }

    enum UpdateResult
    {
        None = 0,
        Success = 1,
        Failed = 2,
    }

    [DataContract]
    class IpcRequest
    {
        [DataMember] public string Command { get; set; }
    }

    [DataContract]
    class IpcResponse
    {
        [DataMember] public bool Success { get; set; }
        [DataMember] public string Message { get; set; }
        [DataMember] public UpdateStatus Status { get; set; }
    }

    [DataContract]
    class UpdateStatus
    {
        [DataMember] public string ServiceVersion { get; set; }
        [DataMember] public DateTime ServiceStartedUtc { get; set; }
        [DataMember] public bool Configured { get; set; }
        [DataMember] public string Domains { get; set; }
        [DataMember] public string Interval { get; set; }

        [DataMember] public bool InProgress { get; set; }
        [DataMember] public UpdateResult LastResult { get; set; }
        [DataMember] public DateTime? LastAttemptUtc { get; set; }
        [DataMember] public DateTime? LastSuccessUtc { get; set; }
        [DataMember] public DateTime? NextUpdateUtc { get; set; }
        [DataMember] public string[] Messages { get; set; }
        [DataMember] public int UpdateCount { get; set; }
        [DataMember] public int FailureCount { get; set; }

        public UpdateStatus Clone()
        {
            UpdateStatus s = (UpdateStatus)MemberwiseClone();
            s.Messages = Messages == null ? new string[0] : (string[])Messages.Clone();
            return s;
        }

        /// <summary>One-line summary, e.g. "OK at 2026-11-17 10:00:00".</summary>
        public string Summary
        {
            get
            {
                if (!Configured)
                    return "Not configured";
                if (InProgress)
                    return "Updating...";
                string when = LastAttemptUtc.HasValue ? LastAttemptUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "";
                switch (LastResult)
                {
                    case UpdateResult.Success: return "OK at " + when;
                    case UpdateResult.Failed: return "FAILED at " + when;
                    default: return "No update yet";
                }
            }
        }
    }

    /// <summary>Length-prefixed (Int32 LE) UTF-8 JSON framing used on the pipe.</summary>
    static class IpcFraming
    {
        private const int MaxMessage = 1024 * 1024;

        public static void Write<T>(Stream s, T msg)
        {
            byte[] body;
            using (MemoryStream ms = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(ms, msg);
                body = ms.ToArray();
            }
            byte[] len = BitConverter.GetBytes(body.Length);
            s.Write(len, 0, 4);
            s.Write(body, 0, body.Length);
            s.Flush();
        }

        public static T Read<T>(Stream s)
        {
            byte[] len = ReadExact(s, 4);
            int n = BitConverter.ToInt32(len, 0);
            if (n < 0 || n > MaxMessage)
                throw new InvalidDataException("Invalid IPC message length " + n);
            byte[] body = ReadExact(s, n);
            using (MemoryStream ms = new MemoryStream(body))
                return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(ms);
        }

        private static byte[] ReadExact(Stream s, int count)
        {
            byte[] buf = new byte[count];
            int off = 0;
            while (off < count)
            {
                int r = s.Read(buf, off, count - off);
                if (r <= 0)
                    throw new EndOfStreamException("Pipe closed");
                off += r;
            }
            return buf;
        }
    }
}
