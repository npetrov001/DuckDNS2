using System;
using System.Windows.Forms;

namespace DuckDNS2
{
    partial class FSettings : Form
    {
        private readonly DDns ddns;

        private FSettings(DDns ddns)
        {
            InitializeComponent();
            this.ddns = ddns;
            LoadData();
        }

        /// <summary>
        /// Shows the advanced settings. Changes are applied to <paramref name="ddns"/> only on OK.
        /// </summary>
        public static bool Execute(IWin32Window owner, DDns ddns)
        {
            using (FSettings f = new FSettings(ddns))
            {
                if (f.ShowDialog(owner) != DialogResult.OK)
                    return false;
                f.SaveData();
                return true;
            }
        }

        private void LoadData()
        {
            cbMulti.Checked = ddns.Domain == "*";
            // work on copies so Cancel really discards edits
            foreach (DDnsDomain d in ddns.Domains)
                ListViewItemFromDomain(Clone(d));
            LookNFeel();
        }

        private void SaveData()
        {
            if (cbMulti.Checked)
            {
                ddns.Domain = "*";
                ddns.Domains.Clear();
                foreach (ListViewItem lvi in lvList.Items)
                    ddns.Domains.Add(lvi.Tag as DDnsDomain);
            }
            else if (ddns.Domain == "*")
            {
                ddns.Domain = lvList.Items.Count > 0 ? lvList.Items[0].Name : "";
            }
        }

        private static DDnsDomain Clone(DDnsDomain d)
        {
            return new DDnsDomain
            {
                Domain = d.Domain,
                ResolutionMode = d.ResolutionMode,
                ResolutionValue = d.ResolutionValue,
                BindIP = d.BindIP,
            };
        }

        private void LookNFeel()
        {
            bool multi = cbMulti.Checked;
            bool one = lvList.SelectedItems.Count == 1;
            lvList.Enabled = multi;
            btAdd.Enabled = multi;
            btEdit.Enabled = multi && one;
            btDel.Enabled = multi && one;
        }

        private ListViewItem ListViewItemFromDomain(DDnsDomain d)
        {
            ListViewItem lvi = lvList.Items.Add(d.Domain);
            lvi.Tag = d;
            UpdateItem(lvi);
            return lvi;
        }

        private void UpdateItem(ListViewItem lvi)
        {
            DDnsDomain d = lvi.Tag as DDnsDomain;
            if (d == null)
                return;
            lvi.Name = lvi.Text = d.Domain;
            while (lvi.SubItems.Count < 2)
                lvi.SubItems.Add("");
            string rdesc = "";
            switch (d.ResolutionMode)
            {
                case DDnsResolutionMode.Server:
                    rdesc = "by DuckDNS";
                    break;
                case DDnsResolutionMode.Local:
                    rdesc = "local";
                    break;
                case DDnsResolutionMode.Fixed:
                    rdesc = "fixed IP: " + d.ResolutionValue;
                    break;
                case DDnsResolutionMode.Host:
                    rdesc = "host " + d.ResolutionValue;
                    break;
                case DDnsResolutionMode.WebService:
                    rdesc = "url " + d.ResolutionValue;
                    break;
            }
            if (!string.IsNullOrEmpty(d.BindIP))
                rdesc += " (bind " + d.BindIP + ")";
            lvi.SubItems[1].Text = rdesc;
        }

        private void cbMulti_CheckedChanged(object sender, EventArgs e)
        {
            LookNFeel();
            if (cbMulti.Checked && ddns.Domain != "*" && !string.IsNullOrEmpty(ddns.Domain))
            {
                bool found = false;
                foreach (ListViewItem lvi in lvList.Items)
                    if (ddns.Domain == lvi.Name)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    ListViewItemFromDomain(new DDnsDomain { Domain = ddns.Domain, ResolutionMode = DDnsResolutionMode.Server });
            }
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            DDnsDomain d = new DDnsDomain { Domain = "", ResolutionMode = DDnsResolutionMode.Server };
            if (!FEditDomain.Execute(this, d))
                return;
            ListViewItem lvi = ListViewItemFromDomain(d);
            lvi.Selected = true;
            lvi.EnsureVisible();
        }

        private void EditSelected()
        {
            if (lvList.SelectedItems.Count != 1)
                return;
            ListViewItem lvi = lvList.SelectedItems[0];
            if (FEditDomain.Execute(this, lvi.Tag as DDnsDomain))
                UpdateItem(lvi);
        }

        private void btEdit_Click(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void lvList_DoubleClick(object sender, EventArgs e)
        {
            EditSelected();
        }

        private void lvList_SelectedIndexChanged(object sender, EventArgs e)
        {
            LookNFeel();
        }

        private void lvList_Resize(object sender, EventArgs e)
        {
            // second column fills the remaining width
            int w = lvList.ClientSize.Width - columnHeader1.Width;
            if (w > 50)
                columnHeader2.Width = w;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (lvList.SelectedItems.Count != 1)
                return;
            ListViewItem lvi = lvList.SelectedItems[0];
            if (MessageBox.Show(this, "Remove domain '" + lvi.Name + "'?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lvi.Remove();
                LookNFeel();
            }
        }
    }
}
