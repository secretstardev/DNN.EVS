using System;
using System.Windows.Forms;

namespace PackageVerification.Desktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = "*.zip";
            ofd.Filter = "Package Files (*.zip)|";
            ofd.Multiselect = false;
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                packagePathTextbox.Text = ofd.FileName;
            }
        }

        private void VerifyButton_Click(object sender, EventArgs e)
        {
            BrowseButton.Enabled = false;
            VerifyButton.Enabled = false;
            Application.DoEvents();
            Models.Package package = VerificationController.TryParse(packagePathTextbox.Text);
            propertyGrid1.SelectedObject = package;
            dataGridView1.DataSource = package.Messages;
            package.CleanUp();
            BrowseButton.Enabled = true;
            VerifyButton.Enabled = true;
            Application.DoEvents();
        }
    }
}
