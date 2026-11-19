using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using DuckDNS2.Service;
using DuckDNS2.Tray;

namespace DuckDNS2
{
    /// <summary>
    /// One EXE, several personalities:
    ///   (started by the SCM)   -> Windows service that performs the DuckDNS updates
    ///   DuckDNS2.exe           -> Manager UI (elevates): install/uninstall/start/stop service, configuration, status
    ///   DuckDNS2.exe -start    -> Tray icon (no elevation) talking to the service (status icon, Update Now)
    ///   DuckDNS2.exe -install | -uninstall | -startservice | -stopservice   -> silent, for scripts (elevates)
    ///   DuckDNS2.exe -debug    -> runs the service engine inside this process (for debugging, elevates)
    /// </summary>
    static class Program
    {
        public static string VERSION { get { return AppPaths.Version; } }

        [STAThread]
        static int Main(string[] args)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            if (!Environment.UserInteractive)
            {
                ServiceBase.Run(new DuckService());
                return 0;
            }

            string cmd = args.Length > 0 ? args[0].Trim().TrimStart('-', '/').ToLowerInvariant() : "";

            if (Environment.OSVersion.Version.Major >= 6)
                Windows.SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            switch (cmd)
            {
                case "start":
                case "tray":
                    return RunTray();

                case "install":
                case "uninstall":
                case "startservice":
                case "stopservice":
                    if (!AppPaths.IsAdministrator())
                        return Elevate(args);
                    return RunSilent(cmd);

                case "debug":
                    if (!AppPaths.IsAdministrator())
                        return Elevate(args);
                    return RunDebug();

                case "":
                    if (!AppPaths.IsAdministrator())
                        return Elevate(args);
                    AppPaths.EnsureDataDir();
                    Application.Run(new FMain());
                    return 0;

                default:
                    MessageBox.Show("Unknown argument '" + args[0] + "'.\r\n\r\n" +
                        "Usage:\r\n" +
                        "  DuckDNS2.exe                 Manager (service control + configuration)\r\n" +
                        "  DuckDNS2.exe -start          Tray icon\r\n" +
                        "  DuckDNS2.exe -install        Install the service\r\n" +
                        "  DuckDNS2.exe -uninstall      Uninstall the service\r\n" +
                        "  DuckDNS2.exe -startservice   Start the service\r\n" +
                        "  DuckDNS2.exe -stopservice    Stop the service\r\n" +
                        "  DuckDNS2.exe -debug          Run the service engine in-process",
                        "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 2;
            }
        }

        private static int RunTray()
        {
            bool created;
            using (Mutex m = new Mutex(true, @"Local\DuckDNS2.Tray", out created))
            {
                if (!created)
                    return 0; // already running in this session
                Application.Run(new TrayContext());
                GC.KeepAlive(m);
            }
            return 0;
        }

        private static int RunSilent(string cmd)
        {
            try
            {
                AppPaths.EnsureDataDir();
                switch (cmd)
                {
                    case "install": ServiceControl.Install(); break;
                    case "uninstall": ServiceControl.Uninstall(); break;
                    case "startservice": ServiceControl.Start(); break;
                    case "stopservice": ServiceControl.Stop(); break;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(cmd + " failed: " + ex.Message);
                return 1;
            }
        }

        private static int RunDebug()
        {
            if (ServiceControl.GetState() == ServiceState.Running)
            {
                MessageBox.Show("The DuckDNS2 service is running. Stop it first, the debug engine uses the same pipe name.",
                    "DuckDNS2 debug", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 1;
            }
            AppPaths.EnsureDataDir();
            using (UpdateEngine engine = new UpdateEngine())
            {
                engine.Start();
                MessageBox.Show("DuckDNS2 service engine is running inside this process.\r\n" +
                    "Use the manager or 'DuckDNS2.exe -start' to talk to it.\r\n\r\nPress OK to stop.",
                    "DuckDNS2 debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return 0;
        }

        /// <summary>Re-launches this EXE elevated (UAC prompt) with the same arguments.</summary>
        private static int Elevate(string[] args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = AppPaths.ExePath,
                    Arguments = string.Join(" ", args.Select(QuoteArg)),
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = true,
                    Verb = "runas",
                };
                Process.Start(psi);
                return 0;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return 1; // user cancelled the UAC prompt
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        private static string QuoteArg(string a)
        {
            return a.IndexOfAny(new[] { ' ', '\t', '"' }) >= 0 ? "\"" + a.Replace("\"", "\\\"") + "\"" : a;
        }

        /// <summary>Starts the manager elevated (used by the tray).</summary>
        public static void LaunchManager()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = AppPaths.ExePath,
                    UseShellExecute = true,
                    Verb = "runas",
                });
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // cancelled
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
