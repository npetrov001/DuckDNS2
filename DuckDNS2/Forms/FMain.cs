using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DuckDNS2.Ipc;
using DuckDNS2.Service;

namespace DuckDNS2
{
    /// <summary>
    /// Manager UI (runs elevated): service install/uninstall/start/stop, configuration and live status.
    /// </summary>
    public partial class FMain : Form
    {
        private const string MultiText = "<multiple configuration>";

        private readonly DDns ddns = new DDns();
        private bool loading;
        private bool polling;

        public FMain()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(AppPaths.ExePath);
        }

        #region Load

        private void FMain_Load(object sender, EventArgs e)
        {
            Text = "DuckDNS2 Manager - v" + AppPaths.Version;
            MinimumSize = Size; // designed (DPI-scaled) size is the smallest sensible layout
            loading = true;
            try
            {
                bool found = ddns.Load();
                if (!found)
                {
                    // first run: pick up a DuckDNS.cfg of the original client sitting next to the EXE
                    string old = Path.Combine(AppPaths.ExeDir, AppPaths.ConfigFileName);
                    if (File.Exists(old) && ddns.LoadFrom(old))
                        MessageBox.Show(this, "Configuration imported from\r\n" + old + "\r\n\r\nClick 'Save & Apply' to store it for the service.",
                            "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ConfigToUI();
                chkTrayAllUsers.Checked = StartupRegistration.IsRegistered(true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                loading = false;
            }
            RefreshServiceState();
            timerRefresh.Start();
        }

        #endregion

        #region Configuration

        private void ConfigToUI()
        {
            DomainToUI();
            txtToken.Text = ddns.Token;
            cbInterval.Text = string.IsNullOrEmpty(ddns.Interval) ? "10m" : ddns.Interval;
            ValidateInterval();
        }

        private void DomainToUI()
        {
            if (ddns.Domain == "*")
            {
                txtDomain.Text = MultiText;
                txtDomain.Enabled = false;
            }
            else
            {
                txtDomain.Text = ddns.Domain;
                txtDomain.Enabled = true;
            }
        }

        private void DomainFromUI()
        {
            ddns.Domain = txtDomain.Enabled ? txtDomain.Text.Trim() : "*";
        }

        private void UIToConfig()
        {
            DomainFromUI();
            ddns.Token = txtToken.Text.Trim();
            ddns.Interval = cbInterval.Text.Trim();
        }

        private bool ValidateInterval()
        {
            bool ok = DDns.ParseInterval(cbInterval.Text).HasValue;
            cbInterval.BackColor = ok ? SystemColors.Window : Color.LightPink;
            return ok;
        }

        private void cbInterval_TextChanged(object sender, EventArgs e)
        {
            ValidateInterval();
        }

        private void txtToken_Enter(object sender, EventArgs e)
        {
            txtToken.UseSystemPasswordChar = false;
        }

        private void txtToken_Leave(object sender, EventArgs e)
        {
            txtToken.UseSystemPasswordChar = true;
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            DomainFromUI();
            if (FSettings.Execute(this, ddns))
                DomainToUI();
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Import configuration of the original DuckDNS client";
                ofd.Filter = "DuckDNS config (DuckDNS.cfg)|DuckDNS.cfg|All files (*.*)|*.*";
                ofd.FileName = AppPaths.ConfigFileName;
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;
                try
                {
                    if (!ddns.LoadFrom(ofd.FileName))
                        throw new Exception("The file could not be read.");
                    ConfigToUI();
                    MessageBox.Show(this, "Imported. Click 'Save & Apply' to store it for the service.", "DuckDNS2",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInterval())
            {
                MessageBox.Show(this, "Invalid interval. Use a number followed by s, m, h or d (e.g. 30m, 1h).", "DuckDNS2",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbInterval.Focus();
                return;
            }
            UIToConfig();
            if (!ddns.IsConfigured)
            {
                if (MessageBox.Show(this, "Domain and/or token are empty - the service will not be able to update.\r\nSave anyway?",
                    "DuckDNS2", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }
            try
            {
                using (new WaitCursor(this))
                    ddns.Save();
            }
            catch (Exception ex)
            {
                ShowError(new Exception("Cannot save " + AppPaths.ConfigFile + ": " + ex.Message, ex));
                return;
            }

            ServiceState st = ServiceControl.GetState();
            if (st == ServiceState.Running)
            {
                RunIpc(IpcClient.Reload);
            }
            else if (st == ServiceState.NotInstalled)
            {
                if (MessageBox.Show(this, "Configuration saved.\r\n\r\nThe DuckDNS2 service is not installed. Install and start it now?",
                    "DuckDNS2", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    DoServiceAction(() => { ServiceControl.Install(); ServiceControl.Start(); });
            }
            else
            {
                if (MessageBox.Show(this, "Configuration saved.\r\n\r\nThe DuckDNS2 service is not running. Start it now?",
                    "DuckDNS2", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    DoServiceAction(ServiceControl.Start);
            }
        }

        #endregion

        #region Service control

        private void RefreshServiceState()
        {
            ServiceState st = ServiceControl.GetState();
            switch (st)
            {
                case ServiceState.Running:
                    SetSvcLabel("RUNNING", StatusText.Good);
                    break;
                case ServiceState.Stopped:
                    SetSvcLabel("STOPPED", StatusText.Bad);
                    break;
                case ServiceState.NotInstalled:
                    SetSvcLabel("NOT INSTALLED", StatusText.Bad);
                    break;
                case ServiceState.StartPending:
                    SetSvcLabel("STARTING...", StatusText.Busy);
                    break;
                case ServiceState.StopPending:
                    SetSvcLabel("STOPPING...", StatusText.Busy);
                    break;
                default:
                    SetSvcLabel(st.ToString().ToUpperInvariant(), StatusText.Neutral);
                    break;
            }

            bool installed = st != ServiceState.NotInstalled;
            btnInstall.Enabled = !installed;
            btnUninstall.Enabled = installed;
            btnStart.Enabled = st == ServiceState.Stopped;
            btnStop.Enabled = st == ServiceState.Running || st == ServiceState.StartPending;
            btnUpdateNow.Enabled = st == ServiceState.Running;

            string path = installed ? ServiceControl.GetInstalledPath() : null;
            if (path == null)
            {
                lblSvcPath.Text = "Service will run: " + AppPaths.ExePath;
                lblSvcPath.ForeColor = Color.DimGray;
            }
            else if (string.Equals(path.Trim('"', ' '), AppPaths.ExePath, StringComparison.OrdinalIgnoreCase))
            {
                lblSvcPath.Text = "Service runs: " + path;
                lblSvcPath.ForeColor = Color.DimGray;
            }
            else
            {
                lblSvcPath.Text = "Installed from a different location: " + path;
                lblSvcPath.ForeColor = StatusText.Bad;
            }

            if (st == ServiceState.Running)
                PollStatus();
            else
                ShowStatus(null, st == ServiceState.NotInstalled ? "Service is not installed" : "Service is not running");
        }

        private void SetSvcLabel(string text, Color color)
        {
            lblSvcStatus.Text = text;
            lblSvcStatus.ForeColor = color;
        }

        private void DoServiceAction(Action action)
        {
            try
            {
                using (new WaitCursor(this))
                    action();
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            RefreshServiceState();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            DoServiceAction(ServiceControl.Install);
            if (ServiceControl.GetState() == ServiceState.Stopped &&
                MessageBox.Show(this, "Service installed. Start it now?", "DuckDNS2", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                DoServiceAction(ServiceControl.Start);
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Stop and uninstall the DuckDNS2 service?\r\n(The configuration is kept.)", "DuckDNS2",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            DoServiceAction(ServiceControl.Uninstall);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            DoServiceAction(ServiceControl.Start);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            DoServiceAction(ServiceControl.Stop);
        }

        #endregion

        #region Status (IPC)

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            RefreshServiceState();
        }

        private void PollStatus()
        {
            if (!polling)
                RunIpc(IpcClient.GetStatus);
        }

        private async void RunIpc(Func<UpdateStatus> call)
        {
            polling = true;
            try
            {
                Tuple<UpdateStatus, string> r = await IpcClient.TryAsync(call);
                if (IsDisposed)
                    return;
                ShowStatus(r.Item1, r.Item2);
            }
            finally
            {
                polling = false;
            }
        }

        private void ShowStatus(UpdateStatus s, string error)
        {
            if (s == null)
            {
                lblResult.Text = string.IsNullOrEmpty(error) ? "No status" : error;
                lblResult.ForeColor = StatusText.Neutral;
                lblDetails.Text = "";
                txtMessages.Text = "";
                return;
            }
            lblResult.Text = StatusText.Headline(s);
            lblResult.ForeColor = StatusText.HeadlineColor(s);
            lblDetails.Text = StatusText.Details(s);
            string msg = StatusText.Messages(s);
            if (txtMessages.Text != msg)
                txtMessages.Text = msg;
        }

        private void btnUpdateNow_Click(object sender, EventArgs e)
        {
            RunIpc(IpcClient.PushUpdate);
        }

        #endregion

        #region Misc

        private void chkTrayAllUsers_CheckedChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            try
            {
                StartupRegistration.Set(true, chkTrayAllUsers.Checked);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnOpenLog_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(AppPaths.LogFile))
                {
                    MessageBox.Show(this, "No log yet:\r\n" + AppPaths.LogFile, "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Process.Start("notepad.exe", "\"" + AppPaths.LogFile + "\"");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnStartTray_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(AppPaths.ExePath, "-start");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            FAbout.Execute();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerRefresh.Stop();
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show(this, ex.Message, "DuckDNS2", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion
    }

    /// <summary>Hourglass cursor for the duration of a using block.</summary>
    sealed class WaitCursor : IDisposable
    {
        private readonly Control ctl;
        private readonly Cursor old;

        public WaitCursor(Control ctl)
        {
            this.ctl = ctl;
            old = ctl.Cursor;
            ctl.Cursor = Cursors.WaitCursor;
            ctl.Update();
        }

        public void Dispose()
        {
            ctl.Cursor = old;
        }
    }
}
