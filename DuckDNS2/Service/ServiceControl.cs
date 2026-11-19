using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;

namespace DuckDNS2.Service
{
    enum ServiceState
    {
        NotInstalled,
        Stopped,
        StartPending,
        Running,
        StopPending,
        Other,
    }

    /// <summary>
    /// Install / uninstall / start / stop of the DuckDNS2 Windows service (requires admin rights).
    /// The service binary is this very same EXE (no arguments = service mode when started by the SCM).
    /// </summary>
    static class ServiceControl
    {
        private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(20);

        public static ServiceState GetState()
        {
            try
            {
                using (ServiceController sc = new ServiceController(AppPaths.ServiceName))
                {
                    switch (sc.Status)
                    {
                        case ServiceControllerStatus.Running: return ServiceState.Running;
                        case ServiceControllerStatus.Stopped: return ServiceState.Stopped;
                        case ServiceControllerStatus.StartPending: return ServiceState.StartPending;
                        case ServiceControllerStatus.StopPending: return ServiceState.StopPending;
                        default: return ServiceState.Other;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return ServiceState.NotInstalled;
            }
        }

        /// <summary>Path of the EXE the installed service points to (null when not installed).</summary>
        public static string GetInstalledPath()
        {
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + AppPaths.ServiceName))
                return k?.GetValue("ImagePath") as string;
        }

        public static void Install()
        {
            if (GetState() != ServiceState.NotInstalled)
                throw new InvalidOperationException("The service is already installed.");

            // binPath must be quoted inside (path may contain spaces): binPath= "\"C:\x y\DuckDNS2.exe\""
            string bin = "\\\"" + AppPaths.ExePath + "\\\"";
            Sc("create " + AppPaths.ServiceName + " binPath= \"" + bin + "\" start= delayed-auto DisplayName= \"" + AppPaths.ServiceDisplayName + "\"");
            Sc("description " + AppPaths.ServiceName + " \"" + AppPaths.ServiceDescription + "\"", false);
            // restart automatically if the process ever crashes: after 1 min, 1 min, 5 min; reset counter after 1 day
            Sc("failure " + AppPaths.ServiceName + " reset= 86400 actions= restart/60000/restart/60000/restart/300000", false);
            Log.Info("Service installed: " + AppPaths.ExePath);
        }

        public static void Uninstall()
        {
            ServiceState st = GetState();
            if (st == ServiceState.NotInstalled)
                return;
            if (st != ServiceState.Stopped)
                Stop();
            Sc("delete " + AppPaths.ServiceName);
            Log.Info("Service uninstalled");
        }

        public static void Start()
        {
            using (ServiceController sc = new ServiceController(AppPaths.ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                    return;
                if (sc.Status != ServiceControllerStatus.StartPending)
                    sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, WaitTimeout);
            }
        }

        public static void Stop()
        {
            using (ServiceController sc = new ServiceController(AppPaths.ServiceName))
            {
                if (sc.Status == ServiceControllerStatus.Stopped)
                    return;
                if (sc.Status != ServiceControllerStatus.StopPending)
                    sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, WaitTimeout);
            }
        }

        /// <summary>Runs sc.exe; throws with sc's output when it fails (and <paramref name="throwOnError"/>).</summary>
        private static void Sc(string args, bool throwOnError = true)
        {
            string output;
            int code = Run("sc.exe", args, out output);
            Log.Info("sc " + args + " => " + code);
            if (code != 0 && throwOnError)
                throw new Exception("sc.exe " + args + "\r\nfailed with exit code " + code + ":\r\n" + output.Trim());
        }

        private static int Run(string exe, string args, out string output)
        {
            ProcessStartInfo psi = new ProcessStartInfo(exe, args)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.Default,
            };
            using (Process p = Process.Start(psi))
            {
                string o = p.StandardOutput.ReadToEnd();
                string e = p.StandardError.ReadToEnd();
                p.WaitForExit();
                output = o + e;
                return p.ExitCode;
            }
        }
    }
}
