using System;
using System.Drawing;
using System.Windows.Forms;
using DuckDNS2.Ipc;
using DuckDNS2.Properties;

namespace DuckDNS2.Tray
{
    /// <summary>
    /// Tray icon runner ("DuckDNS2.exe -start"). Runs non-elevated in the user session and talks to the
    /// service over the named pipe: polls the status (icon colour) and can push an update.
    ///   yellow = last update OK,  blue = update in progress / waiting,  red = failed / service unreachable / not configured
    /// </summary>
    class TrayContext : ApplicationContext
    {
        private const int NormalPollMs = 5000;
        private const int FastPollMs = 1000;

        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip menu;
        private readonly ToolStripMenuItem miStartWithWindows;
        private readonly Timer timer;
        private readonly Icon icoOk = Resources.tray;
        private readonly Icon icoBusy = Resources.tray_checking;
        private readonly Icon icoError = Resources.tray_error;

        private FStatus statusForm;
        private UpdateStatus lastStatus;
        private string lastError;
        private bool firstPoll = true;
        private bool polling;
        private int fastPolls;

        public TrayContext()
        {
            menu = new ContextMenuStrip();
            ToolStripMenuItem miUpdate = new ToolStripMenuItem("Update Now!", null, (s, e) => PushUpdate());
            miUpdate.Font = new Font(miUpdate.Font, FontStyle.Bold);
            menu.Items.Add(miUpdate);
            menu.Items.Add(new ToolStripMenuItem("Status...", null, (s, e) => ShowStatusForm()));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Open DuckDNS2 Manager...", null, (s, e) => Program.LaunchManager()));
            miStartWithWindows = new ToolStripMenuItem("Start tray icon with Windows", null, (s, e) => ToggleStartWithWindows());
            menu.Items.Add(miStartWithWindows);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Exit", null, (s, e) => ExitThread()));
            menu.Opening += (s, e) => miStartWithWindows.Checked = StartupRegistration.IsRegistered(false);

            notifyIcon = new NotifyIcon
            {
                Icon = icoBusy,
                Text = "DuckDNS2: connecting...",
                ContextMenuStrip = menu,
                Visible = true,
            };
            notifyIcon.DoubleClick += (s, e) => ShowStatusForm();

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
                Apply(r.Item1, r.Item2);
            }
            finally
            {
                polling = false;
                if (fastPolls > 0)
                    fastPolls--;
                timer.Interval = fastPolls > 0 ? FastPollMs : NormalPollMs;
            }
        }

        private void Apply(UpdateStatus s, string error)
        {
            UpdateStatus prev = lastStatus;
            string prevError = lastError;
            lastStatus = s;
            lastError = error;

            // --- icon + tooltip
            Icon icon;
            string tip;
            if (s == null)
            {
                icon = icoError;
                tip = "DuckDNS2: service not running";
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
                if (firstPoll || prevError == null)
                    notifyIcon.ShowBalloonTip(5000, "DuckDNS2", "Cannot reach the DuckDNS2 service.\r\n" + error, ToolTipIcon.Warning);
            }
            else if (!s.InProgress && s.LastAttemptUtc.HasValue &&
                     (prev == null || prev.LastAttemptUtc != s.LastAttemptUtc))
            {
                bool wasFailing = prev != null && prev.LastResult == UpdateResult.Failed;
                if (s.LastResult == UpdateResult.Failed)
                {
                    string msg = "Update of DuckDNS failed, check token and domain:\r\n" + StatusText.Messages(s);
                    notifyIcon.ShowBalloonTip(5000, "Error", msg.Length > 250 ? msg.Substring(0, 250) + "..." : msg, ToolTipIcon.Error);
                }
                else if (s.LastResult == UpdateResult.Success && wasFailing)
                {
                    notifyIcon.ShowBalloonTip(3000, "DuckDNS2", "DNS update OK again.", ToolTipIcon.Info);
                }
            }
            firstPoll = false;

            if (statusForm != null && !statusForm.IsDisposed)
                statusForm.ShowStatus(s, error);
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
                notifyIcon.ShowBalloonTip(5000, "DuckDNS2", "Update request failed:\r\n" + r.Item2, ToolTipIcon.Error);
                Apply(null, r.Item2);
                return;
            }
            fastPolls = 30; // poll every second for a while to catch the result
            timer.Interval = FastPollMs;
        }

        private void ShowStatusForm()
        {
            if (statusForm == null || statusForm.IsDisposed)
            {
                statusForm = new FStatus();
                statusForm.UpdateNowRequested += (s, e) => PushUpdate();
                statusForm.ShowStatus(lastStatus, lastError);
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
