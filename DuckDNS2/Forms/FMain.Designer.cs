namespace DuckDNS2
{
    partial class FMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grpService = new System.Windows.Forms.GroupBox();
            this.lblSvcPath = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnUninstall = new System.Windows.Forms.Button();
            this.btnInstall = new System.Windows.Forms.Button();
            this.lblSvcStatus = new System.Windows.Forms.Label();
            this.lblSvcCaption = new System.Windows.Forms.Label();
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.cbInterval = new System.Windows.Forms.ComboBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.lblToken = new System.Windows.Forms.Label();
            this.btnAdvanced = new System.Windows.Forms.Button();
            this.txtDomain = new System.Windows.Forms.TextBox();
            this.lblDomain = new System.Windows.Forms.Label();
            this.grpStatus = new System.Windows.Forms.GroupBox();
            this.btnUpdateNow = new System.Windows.Forms.Button();
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.lblDetails = new System.Windows.Forms.Label();
            this.lblResult = new System.Windows.Forms.Label();
            this.chkTrayAllUsers = new System.Windows.Forms.CheckBox();
            this.btnOpenLog = new System.Windows.Forms.Button();
            this.btnStartTray = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.grpService.SuspendLayout();
            this.grpConfig.SuspendLayout();
            this.grpStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpService
            // 
            this.grpService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpService.Controls.Add(this.lblSvcPath);
            this.grpService.Controls.Add(this.btnStop);
            this.grpService.Controls.Add(this.btnStart);
            this.grpService.Controls.Add(this.btnUninstall);
            this.grpService.Controls.Add(this.btnInstall);
            this.grpService.Controls.Add(this.lblSvcStatus);
            this.grpService.Controls.Add(this.lblSvcCaption);
            this.grpService.Location = new System.Drawing.Point(12, 12);
            this.grpService.Name = "grpService";
            this.grpService.Size = new System.Drawing.Size(499, 108);
            this.grpService.TabIndex = 0;
            this.grpService.TabStop = false;
            this.grpService.Text = "Windows Service";
            // 
            // lblSvcPath
            // 
            this.lblSvcPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSvcPath.AutoEllipsis = true;
            this.lblSvcPath.ForeColor = System.Drawing.Color.DimGray;
            this.lblSvcPath.Location = new System.Drawing.Point(12, 44);
            this.lblSvcPath.Name = "lblSvcPath";
            this.lblSvcPath.Size = new System.Drawing.Size(475, 18);
            this.lblSvcPath.TabIndex = 0;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(372, 68);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(112, 28);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop Service";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(252, 68);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(112, 28);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start Service";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnUninstall
            // 
            this.btnUninstall.Location = new System.Drawing.Point(132, 68);
            this.btnUninstall.Name = "btnUninstall";
            this.btnUninstall.Size = new System.Drawing.Size(112, 28);
            this.btnUninstall.TabIndex = 2;
            this.btnUninstall.Text = "Uninstall Service";
            this.btnUninstall.UseVisualStyleBackColor = true;
            this.btnUninstall.Click += new System.EventHandler(this.btnUninstall_Click);
            // 
            // btnInstall
            // 
            this.btnInstall.Location = new System.Drawing.Point(12, 68);
            this.btnInstall.Name = "btnInstall";
            this.btnInstall.Size = new System.Drawing.Size(112, 28);
            this.btnInstall.TabIndex = 1;
            this.btnInstall.Text = "Install Service";
            this.btnInstall.UseVisualStyleBackColor = true;
            this.btnInstall.Click += new System.EventHandler(this.btnInstall_Click);
            // 
            // lblSvcStatus
            // 
            this.lblSvcStatus.AutoSize = true;
            this.lblSvcStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblSvcStatus.Location = new System.Drawing.Point(60, 22);
            this.lblSvcStatus.Name = "lblSvcStatus";
            this.lblSvcStatus.Size = new System.Drawing.Size(19, 16);
            this.lblSvcStatus.TabIndex = 5;
            this.lblSvcStatus.Text = "...";
            // 
            // lblSvcCaption
            // 
            this.lblSvcCaption.AutoSize = true;
            this.lblSvcCaption.Location = new System.Drawing.Point(12, 24);
            this.lblSvcCaption.Name = "lblSvcCaption";
            this.lblSvcCaption.Size = new System.Drawing.Size(45, 15);
            this.lblSvcCaption.TabIndex = 6;
            this.lblSvcCaption.Text = "Status:";
            // 
            // grpConfig
            // 
            this.grpConfig.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConfig.Controls.Add(this.btnSave);
            this.grpConfig.Controls.Add(this.btnImport);
            this.grpConfig.Controls.Add(this.cbInterval);
            this.grpConfig.Controls.Add(this.lblInterval);
            this.grpConfig.Controls.Add(this.txtToken);
            this.grpConfig.Controls.Add(this.lblToken);
            this.grpConfig.Controls.Add(this.btnAdvanced);
            this.grpConfig.Controls.Add(this.txtDomain);
            this.grpConfig.Controls.Add(this.lblDomain);
            this.grpConfig.Location = new System.Drawing.Point(12, 128);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Size = new System.Drawing.Size(499, 148);
            this.grpConfig.TabIndex = 1;
            this.grpConfig.TabStop = false;
            this.grpConfig.Text = "DuckDNS Configuration";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(355, 112);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(132, 28);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save && Apply";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(84, 112);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(190, 28);
            this.btnImport.TabIndex = 4;
            this.btnImport.Text = "Import old DuckDNS.cfg...";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // cbInterval
            // 
            this.cbInterval.FormattingEnabled = true;
            this.cbInterval.Items.AddRange(new object[] {
            "5m",
            "10m",
            "15m",
            "30m",
            "1h",
            "2h",
            "6h",
            "12h",
            "1d"});
            this.cbInterval.Location = new System.Drawing.Point(84, 85);
            this.cbInterval.Name = "cbInterval";
            this.cbInterval.Size = new System.Drawing.Size(100, 23);
            this.cbInterval.TabIndex = 3;
            this.cbInterval.TextChanged += new System.EventHandler(this.cbInterval_TextChanged);
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(12, 88);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(49, 15);
            this.lblInterval.TabIndex = 6;
            this.lblInterval.Text = "Interval:";
            // 
            // txtToken
            // 
            this.txtToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtToken.Location = new System.Drawing.Point(84, 55);
            this.txtToken.Name = "txtToken";
            this.txtToken.Size = new System.Drawing.Size(403, 23);
            this.txtToken.TabIndex = 2;
            this.txtToken.UseSystemPasswordChar = true;
            this.txtToken.Enter += new System.EventHandler(this.txtToken_Enter);
            this.txtToken.Leave += new System.EventHandler(this.txtToken_Leave);
            // 
            // lblToken
            // 
            this.lblToken.AutoSize = true;
            this.lblToken.Location = new System.Drawing.Point(12, 58);
            this.lblToken.Name = "lblToken";
            this.lblToken.Size = new System.Drawing.Size(43, 15);
            this.lblToken.TabIndex = 7;
            this.lblToken.Text = "Token:";
            // 
            // btnAdvanced
            // 
            this.btnAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdvanced.Location = new System.Drawing.Point(393, 24);
            this.btnAdvanced.Name = "btnAdvanced";
            this.btnAdvanced.Size = new System.Drawing.Size(94, 25);
            this.btnAdvanced.TabIndex = 1;
            this.btnAdvanced.Text = "Advanced...";
            this.btnAdvanced.UseVisualStyleBackColor = true;
            this.btnAdvanced.Click += new System.EventHandler(this.btnAdvanced_Click);
            // 
            // txtDomain
            // 
            this.txtDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDomain.Location = new System.Drawing.Point(84, 25);
            this.txtDomain.Name = "txtDomain";
            this.txtDomain.Size = new System.Drawing.Size(303, 23);
            this.txtDomain.TabIndex = 0;
            // 
            // lblDomain
            // 
            this.lblDomain.AutoSize = true;
            this.lblDomain.Location = new System.Drawing.Point(12, 28);
            this.lblDomain.Name = "lblDomain";
            this.lblDomain.Size = new System.Drawing.Size(54, 15);
            this.lblDomain.TabIndex = 8;
            this.lblDomain.Text = "Domain:";
            // 
            // grpStatus
            // 
            this.grpStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpStatus.Controls.Add(this.btnUpdateNow);
            this.grpStatus.Controls.Add(this.txtMessages);
            this.grpStatus.Controls.Add(this.lblDetails);
            this.grpStatus.Controls.Add(this.lblResult);
            this.grpStatus.Location = new System.Drawing.Point(12, 284);
            this.grpStatus.Name = "grpStatus";
            this.grpStatus.Size = new System.Drawing.Size(499, 204);
            this.grpStatus.TabIndex = 2;
            this.grpStatus.TabStop = false;
            this.grpStatus.Text = "Last Update (reported by the service)";
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUpdateNow.Location = new System.Drawing.Point(12, 166);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(132, 28);
            this.btnUpdateNow.TabIndex = 1;
            this.btnUpdateNow.Text = "Update Now!";
            this.btnUpdateNow.UseVisualStyleBackColor = true;
            this.btnUpdateNow.Click += new System.EventHandler(this.btnUpdateNow_Click);
            // 
            // txtMessages
            // 
            this.txtMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessages.Location = new System.Drawing.Point(12, 104);
            this.txtMessages.Multiline = true;
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ReadOnly = true;
            this.txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessages.Size = new System.Drawing.Size(475, 56);
            this.txtMessages.TabIndex = 0;
            // 
            // lblDetails
            // 
            this.lblDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDetails.Location = new System.Drawing.Point(12, 48);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new System.Drawing.Size(475, 52);
            this.lblDetails.TabIndex = 2;
            // 
            // lblResult
            // 
            this.lblResult.AutoSize = true;
            this.lblResult.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold);
            this.lblResult.Location = new System.Drawing.Point(12, 22);
            this.lblResult.Name = "lblResult";
            this.lblResult.Size = new System.Drawing.Size(20, 18);
            this.lblResult.TabIndex = 3;
            this.lblResult.Text = "...";
            // 
            // chkTrayAllUsers
            // 
            this.chkTrayAllUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTrayAllUsers.AutoSize = true;
            this.chkTrayAllUsers.Location = new System.Drawing.Point(14, 503);
            this.chkTrayAllUsers.Name = "chkTrayAllUsers";
            this.chkTrayAllUsers.Size = new System.Drawing.Size(231, 19);
            this.chkTrayAllUsers.TabIndex = 3;
            this.chkTrayAllUsers.Text = "Show the tray icon at logon (all users)";
            this.chkTrayAllUsers.UseVisualStyleBackColor = true;
            this.chkTrayAllUsers.CheckedChanged += new System.EventHandler(this.chkTrayAllUsers_CheckedChanged);
            // 
            // btnOpenLog
            // 
            this.btnOpenLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOpenLog.Location = new System.Drawing.Point(12, 526);
            this.btnOpenLog.Name = "btnOpenLog";
            this.btnOpenLog.Size = new System.Drawing.Size(96, 28);
            this.btnOpenLog.TabIndex = 4;
            this.btnOpenLog.Text = "Open Log";
            this.btnOpenLog.UseVisualStyleBackColor = true;
            this.btnOpenLog.Click += new System.EventHandler(this.btnOpenLog_Click);
            // 
            // btnStartTray
            // 
            this.btnStartTray.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnStartTray.Location = new System.Drawing.Point(114, 526);
            this.btnStartTray.Name = "btnStartTray";
            this.btnStartTray.Size = new System.Drawing.Size(120, 28);
            this.btnStartTray.TabIndex = 5;
            this.btnStartTray.Text = "Start Tray Icon";
            this.btnStartTray.UseVisualStyleBackColor = true;
            this.btnStartTray.Click += new System.EventHandler(this.btnStartTray_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAbout.Location = new System.Drawing.Point(240, 526);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(80, 28);
            this.btnAbout.TabIndex = 6;
            this.btnAbout.Text = "About";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(415, 526);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 28);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Interval = 2000;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(523, 566);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.btnStartTray);
            this.Controls.Add(this.btnOpenLog);
            this.Controls.Add(this.chkTrayAllUsers);
            this.Controls.Add(this.grpStatus);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.grpService);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "FMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DuckDNS2 Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FMain_FormClosing);
            this.Load += new System.EventHandler(this.FMain_Load);
            this.grpService.ResumeLayout(false);
            this.grpService.PerformLayout();
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.grpStatus.ResumeLayout(false);
            this.grpStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpService;
        private System.Windows.Forms.Label lblSvcCaption;
        private System.Windows.Forms.Label lblSvcStatus;
        private System.Windows.Forms.Label lblSvcPath;
        private System.Windows.Forms.Button btnInstall;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblDomain;
        private System.Windows.Forms.TextBox txtDomain;
        private System.Windows.Forms.Button btnAdvanced;
        private System.Windows.Forms.Label lblToken;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.ComboBox cbInterval;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.GroupBox grpStatus;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.Label lblDetails;
        private System.Windows.Forms.TextBox txtMessages;
        private System.Windows.Forms.Button btnUpdateNow;
        private System.Windows.Forms.CheckBox chkTrayAllUsers;
        private System.Windows.Forms.Button btnOpenLog;
        private System.Windows.Forms.Button btnStartTray;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Timer timerRefresh;
    }
}
