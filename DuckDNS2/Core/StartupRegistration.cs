using System;
using Microsoft.Win32;

namespace DuckDNS2
{
    /// <summary>
    /// "Run at logon" registration of the tray icon (DuckDNS2.exe -start).
    /// Per user = HKCU\...\Run (set from the tray menu), all users = HKLM\...\Run (set from the elevated manager).
    /// </summary>
    static class StartupRegistration
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "DuckDNS2 Tray";

        private static string Command
        {
            get { return "\"" + AppPaths.ExePath + "\" -start"; }
        }

        public static bool IsRegistered(bool allUsers)
        {
            try
            {
                RegistryKey root = allUsers ? Registry.LocalMachine : Registry.CurrentUser;
                using (RegistryKey k = root.OpenSubKey(RunKey, false))
                    return k?.GetValue(ValueName) != null;
            }
            catch
            {
                return false;
            }
        }

        public static void Set(bool allUsers, bool enabled)
        {
            RegistryKey root = allUsers ? Registry.LocalMachine : Registry.CurrentUser;
            using (RegistryKey k = root.CreateSubKey(RunKey))
            {
                if (enabled)
                    k.SetValue(ValueName, Command);
                else if (k.GetValue(ValueName) != null)
                    k.DeleteValue(ValueName);
            }
        }
    }
}
