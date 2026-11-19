using System;
using System.Drawing;
using System.Windows.Forms;
using DuckDNS2.Ipc;

namespace DuckDNS2.Tray
{
    /// <summary>Small non-elevated status window shown from the tray icon.</summary>
    class FStatus : Form
    {
        private readonly Label lblResult;
        private readonly Label lblDetails;
        private readonly TextBox txtMessages;

        public event EventHandler UpdateNowRequested;

        public FStatus()
        {
            Text = "DuckDNS2 Status";
            Font = new Font("Segoe UI", 9F);
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(500, 236);
            try { Icon = Icon.ExtractAssociatedIcon(AppPaths.ExePath); } catch { }

            lblResult = new Label
            {
                AutoSize = true,
                Location = new Point(12, 12),
                Font = new Font("Segoe UI", 11.25F, FontStyle.Bold),
                Text = "...",
            };
            lblDetails = new Label
            {
                Location = new Point(12, 40),
                Size = new Size(476, 54),
            };
            txtMessages = new TextBox
            {
                Location = new Point(12, 98),
                Size = new Size(476, 88),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
            };
            Button btnUpdate = new Button { Text = "Update Now!", Location = new Point(12, 196), Size = new Size(120, 28) };
            btnUpdate.Click += (s, e) => UpdateNowRequested?.Invoke(this, EventArgs.Empty);
            Button btnManager = new Button { Text = "Manager...", Location = new Point(138, 196), Size = new Size(100, 28) };
            btnManager.Click += (s, e) => Program.LaunchManager();
            Button btnClose = new Button { Text = "Close", Location = new Point(388, 196), Size = new Size(100, 28) };
            btnClose.Click += (s, e) => Close();
            CancelButton = btnClose;

            Controls.AddRange(new Control[] { lblResult, lblDetails, txtMessages, btnUpdate, btnManager, btnClose });
        }

        public void ShowStatus(UpdateStatus s, string error)
        {
            if (s == null)
            {
                lblResult.Text = "Service not reachable";
                lblResult.ForeColor = StatusText.Bad;
                lblDetails.Text = (error ?? "") + "\r\nInstall / start the service from the DuckDNS2 Manager.";
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
    }
}
