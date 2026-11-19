using System;
using System.IO;
using System.Text;

namespace DuckDNS2
{
    /// <summary>
    /// Tiny file logger. Writes to %ProgramData%\DuckDNS2\DuckDNS2.log (only writable by SYSTEM/Admins,
    /// so log calls from the non-elevated tray are silently ignored). Rotates at 1 MB.
    /// </summary>
    static class Log
    {
        private const long MaxSize = 1024 * 1024;
        private static readonly object sync = new object();

        public static void Info(string msg) { Write("INFO ", msg); }
        public static void Error(string msg) { Write("ERROR", msg); }

        private static void Write(string level, string msg)
        {
            lock (sync)
            {
                try
                {
                    if (!Directory.Exists(AppPaths.DataDir))
                        return;
                    string file = AppPaths.LogFile;
                    FileInfo fi = new FileInfo(file);
                    if (fi.Exists && fi.Length > MaxSize)
                    {
                        string old = file + ".old";
                        if (File.Exists(old))
                            File.Delete(old);
                        File.Move(file, old);
                    }
                    string who = Environment.UserInteractive ? "ui " : "svc";
                    File.AppendAllText(file,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + who + "] " + level + " " + msg + Environment.NewLine,
                        Encoding.UTF8);
                }
                catch
                {
                    // logging must never break the app
                }
            }
        }
    }
}
