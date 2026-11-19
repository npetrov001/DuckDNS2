using System;
using System.Drawing;
using System.Text;
using DuckDNS2.Ipc;

namespace DuckDNS2
{
    /// <summary>Formatting of <see cref="UpdateStatus"/> shared by the manager and the tray.</summary>
    static class StatusText
    {
        public static readonly Color Good = Color.FromArgb(0, 150, 0);
        public static readonly Color Bad = Color.FromArgb(220, 0, 0);
        public static readonly Color Busy = Color.FromArgb(0, 90, 200);
        public static readonly Color Neutral = Color.DimGray;

        public static string Headline(UpdateStatus s)
        {
            if (s == null) return "Service not reachable";
            if (!s.Configured) return "Not configured";
            if (s.InProgress) return "Updating...";
            switch (s.LastResult)
            {
                case UpdateResult.Success: return "Last update succeeded";
                case UpdateResult.Failed: return "Last update FAILED";
                default: return "Waiting for first update";
            }
        }

        public static Color HeadlineColor(UpdateStatus s)
        {
            if (s == null || !s.Configured) return Bad;
            if (s.InProgress) return Busy;
            switch (s.LastResult)
            {
                case UpdateResult.Success: return Good;
                case UpdateResult.Failed: return Bad;
                default: return Neutral;
            }
        }

        public static string Details(UpdateStatus s)
        {
            if (s == null)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Domains: " + (string.IsNullOrEmpty(s.Domains) ? "-" : s.Domains) + "    Interval: " + s.Interval);
            sb.AppendLine("Last attempt: " + Fmt(s.LastAttemptUtc) + "    Last success: " + Fmt(s.LastSuccessUtc));
            sb.Append("Next update: " + Fmt(s.NextUpdateUtc) + "    Updates: " + s.UpdateCount + " (" + s.FailureCount + " failed)");
            return sb.ToString();
        }

        public static string Messages(UpdateStatus s)
        {
            if (s == null || s.Messages == null || s.Messages.Length == 0)
                return "";
            return string.Join(Environment.NewLine, s.Messages);
        }

        public static string Fmt(DateTime? utc)
        {
            return utc.HasValue ? utc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") : "-";
        }
    }
}
