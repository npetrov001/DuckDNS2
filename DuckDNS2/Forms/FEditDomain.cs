using System;
using System.Windows.Forms;

namespace DuckDNS2
{
    partial class FEditDomain : Form
    {
        private readonly DDnsDomain d;

        private FEditDomain(DDnsDomain d)
        {
            InitializeComponent();
            this.d = d;
            InitMasterData();
            LoadData();
        }

        /// <summary>Edits <paramref name="d"/> in place. Returns false (and leaves it untouched) on Cancel.</summary>
        public static bool Execute(IWin32Window owner, DDnsDomain d)
        {
            using (FEditDomain f = new FEditDomain(d))
                return f.ShowDialog(owner) == DialogResult.OK;
        }

        private void InitMasterData()
        {
            cbResolve.Items.Clear();
            foreach (DDnsResolutionMode rm in Enum.GetValues(typeof(DDnsResolutionMode)))
                cbResolve.Items.Add(rm);
            cbIP.Items.Clear();
            cbIP.Items.Add("<any>");
            foreach (IpInfo ip in Windows.ListInterfacesIPs())
                cbIP.Items.Add(ip);
        }

        private void LoadData()
        {
            tbName.Text = d.Domain;
            cbResolve.SelectedItem = d.ResolutionMode;
            tbRValue.Text = d.ResolutionValue;
            string ip = d.BindIP;
            cbIP.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(ip))
                foreach (object o in cbIP.Items)
                    if ((o is IpInfo) && ((IpInfo)o).Address == ip)
                        cbIP.SelectedItem = o;
            UpdateTitle();
        }

        private void SaveData()
        {
            d.Domain = tbName.Text.Trim();
            d.ResolutionMode = (DDnsResolutionMode)cbResolve.SelectedItem;
            d.ResolutionValue = tbRValue.Visible ? tbRValue.Text.Trim() : "";
            d.BindIP = cbIP.SelectedIndex <= 0 ? "" : ((IpInfo)cbIP.SelectedItem).Address;
        }

        private void UpdateTitle()
        {
            string n = tbName.Text.Trim();
            Text = n.Length == 0 ? "Edit Domain" : "Edit Domain - " + n;
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            if (tbName.Text.Trim().Length == 0)
            {
                MessageBox.Show(this, "Please enter the domain name.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbName.Focus();
                return;
            }
            if (tbRValue.Visible && tbRValue.Text.Trim().Length == 0)
            {
                MessageBox.Show(this, "Please enter the " + lbRValue.Text.TrimEnd(':').ToLowerInvariant() + ".", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbRValue.Focus();
                return;
            }
            SaveData();
            DialogResult = DialogResult.OK;
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        private void cbResolve_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((DDnsResolutionMode)cbResolve.SelectedItem)
            {
                case DDnsResolutionMode.Server:
                    lbRModeInfo.Text = "Let DuckDNS resolve the external IP address of your computer.";
                    lbRValue.Visible = false;
                    tbRValue.Visible = false;
                    break;
                case DDnsResolutionMode.Local:
                    lbRModeInfo.Text = "This computer resolves its own IP address.";
                    lbRValue.Visible = false;
                    tbRValue.Visible = false;
                    break;
                case DDnsResolutionMode.Fixed:
                    lbRModeInfo.Text = "Set the IP to a fixed value.";
                    lbRValue.Text = "IP address:";
                    lbRValue.Visible = true;
                    tbRValue.Visible = true;
                    break;
                case DDnsResolutionMode.Host:
                    lbRModeInfo.Text = "Get the IP by host name as seen by this computer.";
                    lbRValue.Text = "Hostname:";
                    lbRValue.Visible = true;
                    tbRValue.Visible = true;
                    break;
                case DDnsResolutionMode.WebService:
                    lbRModeInfo.Text = "Call a web service (GET) or read a file to retrieve the IP address.";
                    lbRValue.Text = "URL / File:";
                    lbRValue.Visible = true;
                    tbRValue.Visible = true;
                    break;
            }
        }
    }
}
