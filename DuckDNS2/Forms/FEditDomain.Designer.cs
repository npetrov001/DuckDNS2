namespace DuckDNS2
{
    partial class FEditDomain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbResolve = new System.Windows.Forms.ComboBox();
            this.lbRModeInfo = new System.Windows.Forms.Label();
            this.lbRValue = new System.Windows.Forms.Label();
            this.tbRValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbIP = new System.Windows.Forms.ComboBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Domain:";
            //
            // tbName
            //
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(132, 12);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(276, 23);
            this.tbName.TabIndex = 1;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "IP resolution mode:";
            //
            // cbResolve
            //
            this.cbResolve.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbResolve.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbResolve.FormattingEnabled = true;
            this.cbResolve.Location = new System.Drawing.Point(132, 44);
            this.cbResolve.Name = "cbResolve";
            this.cbResolve.Size = new System.Drawing.Size(276, 23);
            this.cbResolve.TabIndex = 3;
            this.cbResolve.SelectedIndexChanged += new System.EventHandler(this.cbResolve_SelectedIndexChanged);
            //
            // lbRModeInfo
            //
            this.lbRModeInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbRModeInfo.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lbRModeInfo.Location = new System.Drawing.Point(132, 72);
            this.lbRModeInfo.Name = "lbRModeInfo";
            this.lbRModeInfo.Size = new System.Drawing.Size(276, 32);
            this.lbRModeInfo.TabIndex = 4;
            this.lbRModeInfo.Text = "...";
            //
            // lbRValue
            //
            this.lbRValue.AutoSize = true;
            this.lbRValue.Location = new System.Drawing.Point(12, 111);
            this.lbRValue.Name = "lbRValue";
            this.lbRValue.Size = new System.Drawing.Size(16, 15);
            this.lbRValue.TabIndex = 5;
            this.lbRValue.Text = "...";
            //
            // tbRValue
            //
            this.tbRValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbRValue.Location = new System.Drawing.Point(132, 108);
            this.tbRValue.Name = "tbRValue";
            this.tbRValue.Size = new System.Drawing.Size(276, 23);
            this.tbRValue.TabIndex = 6;
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 143);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "Bind to IP:";
            //
            // cbIP
            //
            this.cbIP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbIP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbIP.FormattingEnabled = true;
            this.cbIP.Location = new System.Drawing.Point(132, 140);
            this.cbIP.Name = "cbIP";
            this.cbIP.Size = new System.Drawing.Size(276, 23);
            this.cbIP.TabIndex = 8;
            //
            // btOk
            //
            this.btOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btOk.Location = new System.Drawing.Point(236, 182);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(84, 28);
            this.btOk.TabIndex = 9;
            this.btOk.Text = "OK";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            //
            // btCancel
            //
            this.btCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(324, 182);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(84, 28);
            this.btCancel.TabIndex = 10;
            this.btCancel.Text = "Cancel";
            this.btCancel.UseVisualStyleBackColor = true;
            //
            // FEditDomain
            //
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(420, 222);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.cbIP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbRValue);
            this.Controls.Add(this.lbRValue);
            this.Controls.Add(this.lbRModeInfo);
            this.Controls.Add(this.cbResolve);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FEditDomain";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Domain";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbResolve;
        private System.Windows.Forms.Label lbRModeInfo;
        private System.Windows.Forms.Label lbRValue;
        private System.Windows.Forms.TextBox tbRValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbIP;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
    }
}
