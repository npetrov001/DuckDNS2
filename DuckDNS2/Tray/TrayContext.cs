using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Forms;
using DuckDNS2.Ipc;
using DuckDNS2.Properties;
using DuckDNS2.Service;

namespace DuckDNS2.Tray
{
    /// <summary>
    /// How the tray presents the service while the pipe does not answer (state queried from the SCM,
    /// which needs no admin rights).
    /// </summary>
    class ServiceView
    {
        /// <summary>true = expected / transient (blue icon, no balloon), false = a real problem (red icon).</summary>
        public bool Busy;
        public string Headline;
        public string Detail;
        /// <summary>Non-null = worth one balloon; it is shown once per distinct key.</summary>
        public string AlertKey;
        public string AlertText;
        /// <summary>Installed, stopped and not disabled: "Start Service..." is offered.</summary>
        public bool CanStart;
    }

    /// <summary>
    /// Tray icon runner ("DuckDNS2.exe -start"). Runs non-elevated in the user session and talks to the
    /// service over the named pipe: polls the status (icon colour) and can push an update.
    ///   yellow = last update OK,  blue = update in progress / waiting / service starting,
    ///   red = failed / service not running / not configured
    /// When the pipe does not answer the service state is asked from Windows so that the normal
    /// "logged on before the delayed-start service is up" case shows no error.
    /// </summary>
    class TrayContext : ApplicationContext
    {
        private const int NormalPollMs = 5000;
        private const int FastPollMs = 1000;
        /// <summary>Delayed-start services start ~2 minutes after the automatic ones; allow for a slow boot.</summary>
        private static readonly TimeSpan BootGrace = TimeSpan.FromMinutes(6);
        /// <summary>A running service gets this long to open its pipe before the tray complains.</summary>
        private static readonly TimeSpan PipeGrace = TimeSpan.FromSeconds(30);

        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip menu;
        private readonly ToolStripMenuItem miStartService;
        private readonly ToolStripMenuItem miStartWithWindows;
        private readonly Timer timer;
        private readonly Icon icoOk = Resources.tray;
        private readonly Icon icoBusy = Resources.tray_checking;
        private readonly Icon icoError = Resources.tray_error;

        private FStatus statusForm;
        private UpdateStatus lastStatus;
        private string lastError;
        private ServiceView lastView;
        private string lastAlertKey;
        private DateTime? runningWithoutPipeSince;
        private bool balloonStartsService;
        private bool startingService;
        private bool polling;
        private int fastPolls;

        public TrayContext()
        {
            menu = new ContextMenuStrip();
            ToolStripMenuItem miUpdate = new ToolStripMenuItem("Update Now!", null, (s, e) => PushUpdate());
            miUpdate.Font = new Font(miUpdate.Font, FontStyle.Bold);
            menu.Items.Add(miUpdate);
            menu.Items.Add(new ToolStripMenuItem("Status...", null, (s, e) => ShowStatusForm()));
            miStartService = new ToolStripMenuItem("Start Service...", ShieldImage(), (s, e) => StartService());
            menu.Items.Add(miStartService);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Open DuckDNS2 Manager...", null, (s, e) => Program.LaunchManager()));
            miStartWithWindows = new ToolStripMenuItem("Start tray icon with Windows", null, (s, e) => ToggleStartWithWindows());
            menu.Items.Add(miStartWithWindows);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => ExitThread()));
            menu.Opening += (s, e) =>
            {
                miStartWithWindows.Checked = StartupRegistration.IsRegistered(false);
                miStartService.Visible = lastStatus == null && lastView != null && lastView.CanStart && !startingService;
            };

            notifyIcon = new NotifyIcon
            {
                Icon = icoBusy,
                Text = "DuckDNS2: connecting...",
                ContextMenuStrip = menu,
                Visible = true,
            };
            notifyIcon.DoubleClick += (s, e) => ShowStatusForm();
            notifyIcon.BalloonTipClicked += (s, e) =>
            {
                if (balloonStartsService)
                    StartService();
                else
                    ShowStatusForm();
            };

            timer = new Timer { Interval = 300 };
            timer.Tick += (s, e) => Poll();
            timer.Start();
        }

        #region Polling

        private async void Poll()
        {
            if (polling)
                return;
            polling = true;
            try
            {
                Tuple<UpdateStatus, string> r = await IpcClient.TryAsync(IpcClient.GetStatus);
                ServiceView view = null;
                if (r.Item1 == null)
                {
                    // ask Windows why (query rights only, no admin needed); off the UI thread
                    Tuple<ServiceState, ServiceStartMode?> svc = await Task.Run(() =>
                        Tuple.Create(ServiceControl.GetState(), ServiceControl.GetStartMode()));
                    view = Diagnose(svc.Item1, svc.Item2, r.Item2);
                }
                else
                    runningWithoutPipeSince = null;
                Apply(r.Item1, r.Item2, view);
            }
            catch (Exception ex)
            {
                Log.Error("Tray poll: " + ex.Message);
            }
            finally
            {
                polling = false;
                if (fastPolls > 0)
                    fastPolls--;
                timer.Interval = fastPolls > 0 ? FastPollMs : NormalPollMs;
            }
        }

        private ServiceView Diagnose(ServiceState state, ServiceStartMode? mode, string pipeError)
        {
            if (state != ServiceState.Running)
                runningWithoutPipeSince = null;

            switch (state)
            {
                case ServiceState.NotInstalled:
                    return Problem("Service not installed", "Install the service from the DuckDNS2 Manager.");

                case ServiceState.StartPending:
                    return Waiting("Service starting...", "The DuckDNS2 service is starting.");

                case ServiceState.StopPending:
                    return Problem("Service stopping", "The DuckDNS2 service is stopping.");

                case ServiceState.Running:
                    DateTime now = DateTime.UtcNow;
                    if (!runningWithoutPipeSince.HasValue)
                        runningWithoutPipeSince = now;
                    if (now - runningWithoutPipeSince.Value < PipeGrace)
                        return Waiting("Service starting...", "The DuckDNS2 service is starting.");
                    ServiceView nr = Problem("Service not responding",
                        "The service is running but does not answer:\r\n" + pipeError + "\r\nRestart it from the DuckDNS2 Manager.");
                    nr.AlertKey = "noresponse";
                    nr.AlertText = "The DuckDNS2 service is running but does not respond.";
                    return nr;

                case ServiceState.Stopped:
                    if (mode == ServiceStartMode.Disabled)
                        return Problem("Service disabled", "The DuckDNS2 service is disabled (services.msc).");
                    if (mode == ServiceStartMode.Automatic && SystemUptime() < BootGrace)
                    {
                        ServiceView w = Waiting("Waiting for the service to start",
                            "Windows starts the DuckDNS2 service a few minutes after boot (delayed start).");
                        w.CanStart = true;
                        return w;
                    }
                    ServiceView st = Problem("Service not running",
                        "The DuckDNS2 service is stopped.\r\nUse \"Start Service...\" in the tray menu or the DuckDNS2 Manager.");
                    st.CanStart = true;
                    if (mode == ServiceStartMode.Automatic)
                    {
                        // should be running: it never started or it crashed (Windows' own restarts gave up)
                        st.AlertKey = "stopped";
                        st.AlertText = "The DuckDNS2 service is not running.\r\nClick here to start it.";
                    }
                    // Manual: stopped on purpose by an admin -> red icon + tooltip only, no balloon
                    return st;

                default:
                    return Problem("Service " + state.ToString().ToLowerInvariant(), "The DuckDNS2 service is not available.");
            }
        }

        private static ServiceView Waiting(string headline, string detail)
        {
            return new ServiceView { Busy = true, Headline = headline, Detail = detail };
        }

        private static ServiceView Problem(string headline, string detail)
        {
            return new ServiceView { Busy = false, Headline = headline, Detail = detail };
        }

        private void Apply(UpdateStatus s, string error, ServiceView view)
        {
            UpdateStatus prev = lastStatus;
            lastStatus = s;
            lastError = error;
            lastView = view;

            // --- icon + tooltip
            Icon icon;
            string tip;
            if (s == null)
            {
                icon = startingService || view.Busy ? icoBusy : icoError;
                tip = "DuckDNS2: " + (startingService ? "starting service..." : view.Headline.ToLowerInvariant());
            }
            else if (!s.Configured)
            {
                icon = icoError;
                tip = "DuckDNS2: not configured";
            }
            else if (s.InProgress)
            {
                icon = icoBusy;
                tip = "DuckDNS2: updating...";
                fastPolls = Math.Max(fastPolls, 3);
            }
            else if (s.LastResult == UpdateResult.Failed)
            {
                icon = icoError;
                tip = "DuckDNS2: " + s.Summary;
            }
            else if (s.LastResult == UpdateResult.Success)
            {
                icon = icoOk;
                tip = "DuckDNS2: " + s.Summary;
            }
            else
            {
                icon = icoBusy;
                tip = "DuckDNS2: waiting for first update";
            }
            if (notifyIcon.Icon != icon)
                notifyIcon.Icon = icon;
            notifyIcon.Text = tip.Length > 63 ? tip.Substring(0, 63) : tip; // NotifyIcon limit

            // --- balloons (only on transitions)
            if (s == null)
            {
                string key = startingService ? null : view.AlertKey;
                if (key != null && key != lastAlertKey)
                    ShowBalloon(10000, "DuckDNS2", view.AlertText, ToolTipIcon.Warning, view.CanStart);
                lastAlertKey = key;
            }
            else
            {
                lastAlertKey = null;
                if (!s.InProgress && s.LastAttemptUtc.HasValue &&
                    (prev == null || prev.LastAttemptUtc != s.LastAttemptUtc))
                {
                    bool wasFailing = prev != null && prev.LastResult == UpdateResult.Failed;
                    if (s.LastResult == UpdateResult.Failed)
                    {
                        string msg = "Update of DuckDNS failed, check token and domain:\r\n" + StatusText.Messages(s);
                        ShowBalloon(5000, "Error", msg.Length > 250 ? msg.Substring(0, 250) + "..." : msg, ToolTipIcon.Error);
                    }
                    else if (s.LastResult == UpdateResult.Success && wasFailing)
                    {
                        ShowBalloon(3000, "DuckDNS2", "DNS update OK again.", ToolTipIcon.Info);
                    }
                }
            }

            if (statusForm != null && !statusForm.IsDisposed)
                statusForm.ShowStatus(s, error, view);
        }

        private void ShowBalloon(int timeoutMs, string title, string text, ToolTipIcon icon, bool startsService = false)
        {
            balloonStartsService = startsService;
            notifyIcon.ShowBalloonTip(timeoutMs, title, text, icon);
        }

        #endregion

        #region Actions

        private async void PushUpdate()
        {
            notifyIcon.Icon = icoBusy;
            notifyIcon.Text = "DuckDNS2: updating...";
            Tuple<UpdateStatus, string> r = await IpcClient.TryAsync(IpcClient.PushUpdate);
            if (r.Item1 == null)
            {
                ShowBalloon(5000, "DuckDNS2", "Update request failed, the service is not reachable.", ToolTipIcon.Error);
                fastPolls = 1;
                timer.Interval = FastPollMs; // re-poll right away to show the service state
                return;
            }
            fastPolls = 30; // poll every second for a while to catch the result
            timer.Interval = FastPollMs;
        }

        /// <summary>Starts the service through an elevated "DuckDNS2.exe -startservice" (one UAC prompt).</summary>
        private async void StartService()
        {
            if (startingService)
                return;
            Process p;
            try
            {
                p = Process.Start(new ProcessStartInfo
                {
                    FileName = AppPaths.ExePath,
                    Arguments = "-startservice",
                    UseShellExecute = true,
                    Verb = "runas",
                });
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                return; // UAC cancelled
            }
            catch (Exception ex)
            {
                ShowBalloon(5000, "DuckDNS2", "Could not start the service:\r\n" + ex.Message, ToolTipIcon.Error);
                return;
            }
            if (p == null)
                return;

            startingService = true;
            notifyIcon.Icon = icoBusy;
            notifyIcon.Text = "DuckDNS2: starting service...";
            int code;
            try
            {
                code = await Task.Run(() =>
                {
                    using (p)
                    {
                        p.WaitForExit();
                        return p.ExitCode;
                    }
                });
            }
            catch
            {
                code = 0; // could not read the exit code; the next poll shows the real state
            }
            startingService = false;
            if (code != 0)
                ShowBalloon(5000, "DuckDNS2", "The service could not be started.\r\nSee the log: " + AppPaths.LogFile, ToolTipIcon.Error);
            fastPolls = 15; // pick up the running service quickly
            timer.Interval = FastPollMs;
        }

        private void ShowStatusForm()
        {
            if (statusForm == null || statusForm.IsDisposed)
            {
                statusForm = new FStatus();
                statusForm.UpdateNowRequested += (s, e) => PushUpdate();
                statusForm.ShowStatus(lastStatus, lastError, lastView);
                statusForm.Show();
            }
            else
            {
                if (statusForm.WindowState == FormWindowState.Minimized)
                    statusForm.WindowState = FormWindowState.Normal;
                statusForm.Activate();
            }
        }

        private void ToggleStartWithWindows()
        {
            try
            {
                StartupRegistration.Set(false, !StartupRegistration.IsRegistered(false));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helpers

        [DllImport("kernel32.dll")]
        private static extern ulong GetTickCount64();

        /// <summary>Time since Windows booted (with Fast Startup the services keep running, so a stopped one is real).</summary>
        private static TimeSpan SystemUptime()
        {
            return TimeSpan.FromMilliseconds(GetTickCount64());
        }

        private static Image ShieldImage()
        {
            try
            {
                using (Icon shield = new Icon(SystemIcons.Shield, SystemInformation.SmallIconSize))
                    return shield.ToBitmap();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        protected override void ExitThreadCore()
        {
            timer.Stop();
            if (statusForm != null && !statusForm.IsDisposed)
                statusForm.Close();
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            menu.Dispose();
            base.ExitThreadCore();
        }
    }
}
