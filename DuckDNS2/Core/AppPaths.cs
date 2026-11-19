using System;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DuckDNS2
{
    /// <summary>
    /// Shared locations. The service runs as LocalSystem, so the configuration lives in
    /// %ProgramData%\DuckDNS2 which is restricted to SYSTEM + Administrators (it holds the DuckDNS token).
    /// </summary>
    static class AppPaths
    {
        public const string ServiceName = "DuckDNS2";
        public const string ServiceDisplayName = "DuckDNS2 Updater";
        public const string ServiceDescription = "Keeps DuckDNS (duckdns.org) domains pointing at this computer's IP address.";
        public const string PipeName = "DuckDNS2.Service";
        public const string ConfigFileName = "DuckDNS.cfg";

        public static string DataDir
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "DuckDNS2"); }
        }

        public static string ConfigFile { get { return Path.Combine(DataDir, ConfigFileName); } }

        public static string LogFile { get { return Path.Combine(DataDir, "DuckDNS2.log"); } }

        public static string ExePath { get { return Assembly.GetExecutingAssembly().Location; } }

        public static string ExeDir { get { return Path.GetDirectoryName(ExePath); } }

        /// <summary>
        /// Creates the data folder. On creation the folder is locked down to SYSTEM and Administrators
        /// (inheritance from ProgramData would give every user read access to the token).
        /// </summary>
        public static void EnsureDataDir()
        {
            if (Directory.Exists(DataDir))
                return;
            Directory.CreateDirectory(DataDir);
            try
            {
                DirectorySecurity ds = new DirectorySecurity();
                ds.SetAccessRuleProtection(true, false);
                InheritanceFlags inh = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                ds.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                    FileSystemRights.FullControl, inh, PropagationFlags.None, AccessControlType.Allow));
                ds.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                    FileSystemRights.FullControl, inh, PropagationFlags.None, AccessControlType.Allow));
                Directory.SetAccessControl(DataDir, ds);
            }
            catch (Exception ex)
            {
                Log.Error("Cannot secure data folder: " + ex.Message);
            }
        }

        public static bool IsAdministrator()
        {
            using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(3); }
        }
    }
}
