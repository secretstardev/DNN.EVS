using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using SQLAzureMWUtils;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SQLAzureMW
{
    public partial class CreateDatabase : Form
    {
        private TargetServerInfo _TargetServerInfo = null;
        private string _DatabaseName;
        private int _major = 0;

        public string DatabaseName
        {
            get { return _DatabaseName; }
            set { _DatabaseName = value; }
        }

        public CreateDatabase(TargetServerInfo TargetServer)
        {
            InitializeComponent();
            _TargetServerInfo = TargetServer;

            Init();
        }

        private void Init()
        {
            List<Collation> colList = CollationHelper.GetCollationList();
            cbCollations.DataSource = colList;
            string dbCol = CommonFunc.GetAppSettingsStringValue("DBCollation");
            if (dbCol.Length > 0)
            {
                int index = 0;
                foreach (Collation col in colList)
                {
                    if (dbCol.Equals(col.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        cbCollations.SelectedIndex = index;
                        break;
                    }
                    ++index;
                }
            }

            if (_TargetServerInfo.ServerType != ServerTypes.AzureSQLDB)
            {
                gbDatabaseSize.Enabled = false;
                return;
            }

            gbDatabaseSize.Enabled = true;
            _major = Convert.ToInt32(_TargetServerInfo.Version.Substring(0, 2));

            List<DatabaseEdition> editions = new List<DatabaseEdition>();

            // Web      = 100 MB, 1 GB, 5 GB
            // Business = 10, 20, 30, 40, 50, 100, 150
            // Basic    = 100 MB, 500 MB, 1, 2
            // Standard = 100 MB, 500 MB, 1, 2, 5, 10, 20, 30, 40, 50, 100, 150, 200, 250
            // Premium  = 100 MB, 500 MB, 1, 2, 5, 10, 20, 30, 40, 50, 100, 150, 200, 250, 300, 400, 500

            DatabaseEdition webEdition = new DatabaseEdition(new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseWebEdition, "web"));
            DatabaseEdition busEdition = new DatabaseEdition(new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseBusinessEdition, "business"));
            DatabaseEdition basEdition = new DatabaseEdition(new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseBasicEdition, "basic"));
            DatabaseEdition stdEdition = new DatabaseEdition(new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseStandardEdition, "standard"));
            DatabaseEdition preEdition = new DatabaseEdition(new KeyValuePair<string, string>(Properties.Resources.CreateDatabasePremiumEdition, "premium"));

            KeyValuePair<string, string> dbSize100mb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeOneHundredMB, "100 MB");
            KeyValuePair<string, string> dbSize500mb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeFiveHundredMB, "500 MB");
            KeyValuePair<string, string> dbSize1gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeOneGB, "1 GB");
            KeyValuePair<string, string> dbSize2gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeTwoGB, "2 GB");
            KeyValuePair<string, string> dbSize5gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeFiveGB, "5 GB");
            KeyValuePair<string, string> dbSize10gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeTenGB, "10 GB");
            KeyValuePair<string, string> dbSize20gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeTwentyGB, "20 GB");
            KeyValuePair<string, string> dbSize30gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeThirtyGB, "30 GB");
            KeyValuePair<string, string> dbSize40gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeFourtyGB, "40 GB");
            KeyValuePair<string, string> dbSize50gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeFiftyGB, "50 GB");
            KeyValuePair<string, string> dbSize100gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeOneHundredGB, "100 GB");
            KeyValuePair<string, string> dbSize150gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeOneHundredFiftyGB, "150 GB");
            KeyValuePair<string, string> dbSize200gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeTwoHundredGB, "200 GB");
            KeyValuePair<string, string> dbSize250gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeTwoHundredFiftyGB, "250 GB");
            KeyValuePair<string, string> dbSize300gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeThreeHundredGB, "300 GB");
            KeyValuePair<string, string> dbSize400gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeFourHundredGB, "400 GB");
            KeyValuePair<string, string> dbSize500gb = new KeyValuePair<string, string>(Properties.Resources.CreateDatabaseSizeFiveHundredGB, "500 GB");

            webEdition.Size.Add(dbSize100mb);
            webEdition.Size.Add(dbSize1gb);
            webEdition.Size.Add(dbSize5gb);

            busEdition.Size.Add(dbSize10gb);
            busEdition.Size.Add(dbSize20gb);
            busEdition.Size.Add(dbSize30gb);
            busEdition.Size.Add(dbSize40gb);
            busEdition.Size.Add(dbSize50gb);
            busEdition.Size.Add(dbSize100gb);
            busEdition.Size.Add(dbSize150gb);

            basEdition.Size.Add(dbSize100mb);
            basEdition.Size.Add(dbSize500mb);
            basEdition.Size.Add(dbSize1gb);
            basEdition.Size.Add(dbSize2gb);

            stdEdition.Size.Add(dbSize100mb);
            stdEdition.Size.Add(dbSize500mb);
            stdEdition.Size.Add(dbSize1gb);
            stdEdition.Size.Add(dbSize2gb);
            stdEdition.Size.Add(dbSize5gb);
            stdEdition.Size.Add(dbSize10gb);
            stdEdition.Size.Add(dbSize20gb);
            stdEdition.Size.Add(dbSize30gb);
            stdEdition.Size.Add(dbSize40gb);
            stdEdition.Size.Add(dbSize50gb);
            stdEdition.Size.Add(dbSize100gb);
            stdEdition.Size.Add(dbSize150gb);
            stdEdition.Size.Add(dbSize200gb);
            stdEdition.Size.Add(dbSize250gb);

            preEdition.Size.Add(dbSize100mb);
            preEdition.Size.Add(dbSize500mb);
            preEdition.Size.Add(dbSize1gb);
            preEdition.Size.Add(dbSize2gb);
            preEdition.Size.Add(dbSize5gb);
            preEdition.Size.Add(dbSize10gb);
            preEdition.Size.Add(dbSize20gb);
            preEdition.Size.Add(dbSize30gb);
            preEdition.Size.Add(dbSize40gb);
            preEdition.Size.Add(dbSize50gb);
            preEdition.Size.Add(dbSize100gb);
            preEdition.Size.Add(dbSize150gb);
            preEdition.Size.Add(dbSize200gb);
            preEdition.Size.Add(dbSize250gb);
            preEdition.Size.Add(dbSize300gb);
            preEdition.Size.Add(dbSize400gb);
            preEdition.Size.Add(dbSize500gb);

            webEdition.PerformanceLevel.Add(new KeyValuePair<string, string>("", ""));
            busEdition.PerformanceLevel.Add(new KeyValuePair<string, string>("", ""));

            basEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelBasicFiveDTUs, ""));

            int eol_Year = Convert.ToInt32(CommonFunc.GetAppSettingsStringValue("EndOfLife_Year"));
            int eol_Month = Convert.ToInt32(CommonFunc.GetAppSettingsStringValue("EndOfLife_Month"));
            int eol_Day = Convert.ToInt32(CommonFunc.GetAppSettingsStringValue("EndOfLife_Day"));

            DateTime eol_dt = new DateTime(eol_Year, eol_Month, eol_Day);
            bool past_eol = DateTime.Compare(DateTime.Now, eol_dt) < 0 ? false : true;

            stdEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelStandardS0, "S0"));
            stdEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelStandardS1, "S1"));
            stdEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelStandardS2, "S2"));

            preEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelPremiumP1, "P1"));
            preEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelPremiump2, "P2"));
            preEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelPremiumP3, "P3"));

            if (_major > 11)
            {
                stdEdition.PerformanceLevel.Add(new KeyValuePair<string, string>(Properties.Resources.PerformanceLevelStandardS3, "S3"));
            }
            else
            {
                if (!past_eol) editions.Add(webEdition);
                if (!past_eol) editions.Add(busEdition);
            }

            editions.Add(basEdition);
            editions.Add(stdEdition);
            editions.Add(preEdition);

            cbEdition.DataSource = editions;
            cbEdition.DisplayMember = "Display";
            cbEdition.SelectedItem = stdEdition;
        }

        private void btnCreateDatabase_Click(object sender, EventArgs e)
        {
            if (tbNewDatabase.Text.Length == 0)
            {
                MessageBox.Show(label1.Text, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbNewDatabase.Focus();
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                TargetProcessor tp = new TargetProcessor();
                _TargetServerInfo.TargetDatabase = tbNewDatabase.Text;
                if (tp.DoesDatabaseExist(_TargetServerInfo))
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show(Properties.Resources.MessageDatabaseExists, Properties.Resources.DatabaseExists, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tbNewDatabase.Focus();
                }
                else
                {
                    string size = "";
                    string edition = "";
                    string performanceLevel = "";

                    if (_TargetServerInfo.ServerType == ServerTypes.AzureSQLDB)
                    {
                        edition = ((DatabaseEdition)cbEdition.SelectedItem).Edition.Value;
                        performanceLevel = ((KeyValuePair<string, string>)cbPerformanceLevel.SelectedItem).Value;

                        if (_major < 12)
                        {
                            size = lbMaxDatabaseSize.Text;
                        }
                    }
                    DatabaseCreateStatus dcs = tp.CreateDatabase(_TargetServerInfo, ((Collation)cbCollations.SelectedValue).Name, edition, size, performanceLevel, false);

                    if (dcs == DatabaseCreateStatus.Success)
                    {
                        DatabaseName = "[" + tbNewDatabase.Text + "]";
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        // Ok, the database is in the process of being created, but the calling function
                        // will have to check back later to see if it has been created.  From what I have
                        // heard, it can take Azure SQL DB up to 30 minutes to deploy.

                        DialogResult = DialogResult.Retry;
                    }
                    Close();
                    this.Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(CommonFunc.GetLowestException(ex), Properties.Resources.ErrorCreatingDB, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateDatabase_Load(object sender, EventArgs e)
        {
            tbNewDatabase.Focus();
        }

        private void cbEdition_SelectedIndexChanged(object sender, EventArgs e)
        {
            DatabaseEdition edition = (DatabaseEdition) cbEdition.SelectedItem;
            lbMaxDatabaseSize.Text = edition.Size.Last().Value;

            cbPerformanceLevel.DataSource = edition.PerformanceLevel;
            cbPerformanceLevel.DisplayMember = "key";

            int originalLevel = cbPerformanceLevel.SelectedIndex;

            switch (edition.Edition.Value)
            {
                case "web":
                    gbPreviewRegistration.Visible = lablePerformance.Visible = cbPerformanceLevel.Visible = false;
                    gbNotRecommended.Visible = true;
                    break;

                case "business":
                    gbPreviewRegistration.Visible = lablePerformance.Visible = cbPerformanceLevel.Visible = false;
                    gbNotRecommended.Visible = true;
                    break;

                case "basic":
                    cbPerformanceLevel.SelectedIndex = 0;
                    gbPreviewRegistration.Visible = lablePerformance.Visible = cbPerformanceLevel.Visible = true;
                    gbNotRecommended.Visible = false;
                    break;

                case "standard":
                    cbPerformanceLevel.SelectedIndex = 2;
                    gbPreviewRegistration.Visible = lablePerformance.Visible = cbPerformanceLevel.Visible = true;
                    gbNotRecommended.Visible = false;
                    break;

                case "premium":
                    cbPerformanceLevel.SelectedIndex = 0;
                    gbPreviewRegistration.Visible = lablePerformance.Visible = cbPerformanceLevel.Visible = true;
                    gbNotRecommended.Visible = false;
                    break;

                default:
                    break;
            }

            if (originalLevel == cbPerformanceLevel.SelectedIndex)
            {
                cbPerformanceLevel_SelectedIndexChanged(null, null);
            }
        }

        private void cbCollations_SelectedIndexChanged(object sender, EventArgs e)
        {
            string col = "";

            if (((Collation) cbCollations.SelectedValue).Name.Length > 0)
            {
                col = " (" + ((Collation)cbCollations.SelectedValue).Name + ")";
            }
            lbCollation.Text = CommonFunc.FormatString(Properties.Resources.DatabaseCollation, col);
        }

        private void cbPerformanceLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_major > 11) return;

        }

        private void linkEndOfLife_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = CommonFunc.GetAppSettingsStringValue("EndOfLife_Link");
            if (link.Length > 0) Process.Start(link);
        }

        private void linkPreviewRegistration_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string link = CommonFunc.GetAppSettingsStringValue("ServiceTiersPerformanceLevels_Link");
            if (link.Length > 0) Process.Start(link);
        }
    }
}
