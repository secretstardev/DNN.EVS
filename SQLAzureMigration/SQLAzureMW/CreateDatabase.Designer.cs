namespace SQLAzureMW
{
    partial class CreateDatabase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateDatabase));
            this.gbDatabaseSize = new System.Windows.Forms.GroupBox();
            this.lbMaxDatabaseSize = new System.Windows.Forms.Label();
            this.gbPreviewRegistration = new System.Windows.Forms.GroupBox();
            this.linkPreviewRegistration = new System.Windows.Forms.LinkLabel();
            this.gbNotRecommended = new System.Windows.Forms.GroupBox();
            this.linkEndOfLife = new System.Windows.Forms.LinkLabel();
            this.cbPerformanceLevel = new System.Windows.Forms.ComboBox();
            this.lablePerformance = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbMaxDbSize = new System.Windows.Forms.Label();
            this.cbEdition = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbNewDatabase = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnCreateDatabase = new System.Windows.Forms.Button();
            this.lbCollation = new System.Windows.Forms.Label();
            this.cbCollations = new System.Windows.Forms.ComboBox();
            this.gbDatabaseSize.SuspendLayout();
            this.gbPreviewRegistration.SuspendLayout();
            this.gbNotRecommended.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDatabaseSize
            // 
            resources.ApplyResources(this.gbDatabaseSize, "gbDatabaseSize");
            this.gbDatabaseSize.Controls.Add(this.lbMaxDatabaseSize);
            this.gbDatabaseSize.Controls.Add(this.gbPreviewRegistration);
            this.gbDatabaseSize.Controls.Add(this.gbNotRecommended);
            this.gbDatabaseSize.Controls.Add(this.cbPerformanceLevel);
            this.gbDatabaseSize.Controls.Add(this.lablePerformance);
            this.gbDatabaseSize.Controls.Add(this.label4);
            this.gbDatabaseSize.Controls.Add(this.lbMaxDbSize);
            this.gbDatabaseSize.Controls.Add(this.cbEdition);
            this.gbDatabaseSize.Name = "gbDatabaseSize";
            this.gbDatabaseSize.TabStop = false;
            // 
            // lbMaxDatabaseSize
            // 
            resources.ApplyResources(this.lbMaxDatabaseSize, "lbMaxDatabaseSize");
            this.lbMaxDatabaseSize.Name = "lbMaxDatabaseSize";
            // 
            // gbPreviewRegistration
            // 
            this.gbPreviewRegistration.Controls.Add(this.linkPreviewRegistration);
            resources.ApplyResources(this.gbPreviewRegistration, "gbPreviewRegistration");
            this.gbPreviewRegistration.Name = "gbPreviewRegistration";
            this.gbPreviewRegistration.TabStop = false;
            // 
            // linkPreviewRegistration
            // 
            resources.ApplyResources(this.linkPreviewRegistration, "linkPreviewRegistration");
            this.linkPreviewRegistration.Name = "linkPreviewRegistration";
            this.linkPreviewRegistration.TabStop = true;
            this.linkPreviewRegistration.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkPreviewRegistration_LinkClicked);
            // 
            // gbNotRecommended
            // 
            this.gbNotRecommended.Controls.Add(this.linkEndOfLife);
            this.gbNotRecommended.ForeColor = System.Drawing.Color.Red;
            resources.ApplyResources(this.gbNotRecommended, "gbNotRecommended");
            this.gbNotRecommended.Name = "gbNotRecommended";
            this.gbNotRecommended.TabStop = false;
            // 
            // linkEndOfLife
            // 
            resources.ApplyResources(this.linkEndOfLife, "linkEndOfLife");
            this.linkEndOfLife.Name = "linkEndOfLife";
            this.linkEndOfLife.TabStop = true;
            this.linkEndOfLife.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkEndOfLife_LinkClicked);
            // 
            // cbPerformanceLevel
            // 
            this.cbPerformanceLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPerformanceLevel.FormattingEnabled = true;
            resources.ApplyResources(this.cbPerformanceLevel, "cbPerformanceLevel");
            this.cbPerformanceLevel.Name = "cbPerformanceLevel";
            this.cbPerformanceLevel.SelectedIndexChanged += new System.EventHandler(this.cbPerformanceLevel_SelectedIndexChanged);
            // 
            // lablePerformance
            // 
            resources.ApplyResources(this.lablePerformance, "lablePerformance");
            this.lablePerformance.Name = "lablePerformance";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // lbMaxDbSize
            // 
            resources.ApplyResources(this.lbMaxDbSize, "lbMaxDbSize");
            this.lbMaxDbSize.Name = "lbMaxDbSize";
            // 
            // cbEdition
            // 
            this.cbEdition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbEdition.FormattingEnabled = true;
            resources.ApplyResources(this.cbEdition, "cbEdition");
            this.cbEdition.Name = "cbEdition";
            this.cbEdition.SelectedIndexChanged += new System.EventHandler(this.cbEdition_SelectedIndexChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // tbNewDatabase
            // 
            resources.ApplyResources(this.tbNewDatabase, "tbNewDatabase");
            this.tbNewDatabase.Name = "tbNewDatabase";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            // 
            // btnCreateDatabase
            // 
            resources.ApplyResources(this.btnCreateDatabase, "btnCreateDatabase");
            this.btnCreateDatabase.Name = "btnCreateDatabase";
            this.btnCreateDatabase.Click += new System.EventHandler(this.btnCreateDatabase_Click);
            // 
            // lbCollation
            // 
            resources.ApplyResources(this.lbCollation, "lbCollation");
            this.lbCollation.Name = "lbCollation";
            // 
            // cbCollations
            // 
            resources.ApplyResources(this.cbCollations, "cbCollations");
            this.cbCollations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbCollations.FormattingEnabled = true;
            this.cbCollations.Name = "cbCollations";
            this.cbCollations.SelectedIndexChanged += new System.EventHandler(this.cbCollations_SelectedIndexChanged);
            // 
            // CreateDatabase
            // 
            this.AcceptButton = this.btnCreateDatabase;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.Controls.Add(this.cbCollations);
            this.Controls.Add(this.lbCollation);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnCreateDatabase);
            this.Controls.Add(this.tbNewDatabase);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.gbDatabaseSize);
            this.Name = "CreateDatabase";
            this.Load += new System.EventHandler(this.CreateDatabase_Load);
            this.gbDatabaseSize.ResumeLayout(false);
            this.gbDatabaseSize.PerformLayout();
            this.gbPreviewRegistration.ResumeLayout(false);
            this.gbPreviewRegistration.PerformLayout();
            this.gbNotRecommended.ResumeLayout(false);
            this.gbNotRecommended.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbDatabaseSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbNewDatabase;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnCreateDatabase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbMaxDbSize;
        private System.Windows.Forms.ComboBox cbEdition;
        private System.Windows.Forms.ComboBox cbCollations;
        private System.Windows.Forms.Label lbCollation;
        private System.Windows.Forms.ComboBox cbPerformanceLevel;
        private System.Windows.Forms.Label lablePerformance;
        private System.Windows.Forms.GroupBox gbNotRecommended;
        private System.Windows.Forms.LinkLabel linkEndOfLife;
        private System.Windows.Forms.GroupBox gbPreviewRegistration;
        private System.Windows.Forms.LinkLabel linkPreviewRegistration;
        private System.Windows.Forms.Label lbMaxDatabaseSize;
    }
}