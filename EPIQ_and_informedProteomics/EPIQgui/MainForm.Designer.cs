namespace EPIQgui
{
    partial class MainForm
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
            this.mainTabCtrl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.gbLabel = new System.Windows.Forms.GroupBox();
            this.btnAddLabelSite = new System.Windows.Forms.Button();
            this.dgvLabelScheme = new System.Windows.Forms.DataGridView();
            this.btnLoadLabelScheme = new System.Windows.Forms.Button();
            this.btnSaveLabelScheme = new System.Windows.Forms.Button();
            this.lblAddLabelSite = new System.Windows.Forms.Label();
            this.comboboxLabelSite = new System.Windows.Forms.ComboBox();
            this.nudMultiplicity = new System.Windows.Forms.NumericUpDown();
            this.lblMultiplicity = new System.Windows.Forms.Label();
            this.rbCustomLabel = new System.Windows.Forms.RadioButton();
            this.rbPredefLabel = new System.Windows.Forms.RadioButton();
            this.lblPredefLabel = new System.Windows.Forms.Label();
            this.comboboxPredefLabel = new System.Windows.Forms.ComboBox();
            this.gbRT = new System.Windows.Forms.GroupBox();
            this.rbNoRtShift = new System.Windows.Forms.RadioButton();
            this.btnBrowseCustomRtStandard = new System.Windows.Forms.Button();
            this.tbCustomRtStandard = new System.Windows.Forms.TextBox();
            this.btnBrowseCustomRtModel = new System.Windows.Forms.Button();
            this.tbCustomRtModel = new System.Windows.Forms.TextBox();
            this.lblCustomRtStandard = new System.Windows.Forms.Label();
            this.lblCustomRtModel = new System.Windows.Forms.Label();
            this.rbCustomRt = new System.Windows.Forms.RadioButton();
            this.rbBuiltinRt = new System.Windows.Forms.RadioButton();
            this.lblBuiltinRt = new System.Windows.Forms.Label();
            this.comboboxLcType = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.gbAddOpts = new System.Windows.Forms.GroupBox();
            this.btnBrowseFasta = new System.Windows.Forms.Button();
            this.tbFasta = new System.Windows.Forms.TextBox();
            this.lblFasta = new System.Windows.Forms.Label();
            this.gbOutOptions = new System.Windows.Forms.GroupBox();
            this.btnBrowseXicDir = new System.Windows.Forms.Button();
            this.btnBrowseOutDir = new System.Windows.Forms.Button();
            this.tbXicMfile = new System.Windows.Forms.TextBox();
            this.lblXicMfile = new System.Windows.Forms.Label();
            this.cbWriteExcel = new System.Windows.Forms.CheckBox();
            this.lblOutDir = new System.Windows.Forms.Label();
            this.cbWriteXic = new System.Windows.Forms.CheckBox();
            this.tbOutPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.nudNumFrac = new System.Windows.Forms.NumericUpDown();
            this.lblNumFrac = new System.Windows.Forms.Label();
            this.lvFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnRmRep = new System.Windows.Forms.Button();
            this.btnRmCond = new System.Windows.Forms.Button();
            this.btnLoadID = new System.Windows.Forms.Button();
            this.btnLoadRaw = new System.Windows.Forms.Button();
            this.btnAddRep = new System.Windows.Forms.Button();
            this.btnAddCond = new System.Windows.Forms.Button();
            this.tvScheme = new System.Windows.Forms.TreeView();
            this.openMultiFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.pgbRun = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.openSingleFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lblVersion = new System.Windows.Forms.Label();
            this.mainTabCtrl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.gbLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLabelScheme)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMultiplicity)).BeginInit();
            this.gbRT.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.gbAddOpts.SuspendLayout();
            this.gbOutOptions.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumFrac)).BeginInit();
            this.SuspendLayout();
            // 
            // mainTabCtrl
            // 
            this.mainTabCtrl.Controls.Add(this.tabPage1);
            this.mainTabCtrl.Controls.Add(this.tabPage2);
            this.mainTabCtrl.Location = new System.Drawing.Point(9, 11);
            this.mainTabCtrl.Margin = new System.Windows.Forms.Padding(2);
            this.mainTabCtrl.Name = "mainTabCtrl";
            this.mainTabCtrl.SelectedIndex = 0;
            this.mainTabCtrl.Size = new System.Drawing.Size(1164, 566);
            this.mainTabCtrl.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.gbLabel);
            this.tabPage1.Controls.Add(this.gbRT);
            this.tabPage1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(1156, 540);
            this.tabPage1.TabIndex = 1;
            this.tabPage1.Text = "Label Configuration";
            // 
            // gbLabel
            // 
            this.gbLabel.Controls.Add(this.btnAddLabelSite);
            this.gbLabel.Controls.Add(this.dgvLabelScheme);
            this.gbLabel.Controls.Add(this.btnLoadLabelScheme);
            this.gbLabel.Controls.Add(this.btnSaveLabelScheme);
            this.gbLabel.Controls.Add(this.lblAddLabelSite);
            this.gbLabel.Controls.Add(this.comboboxLabelSite);
            this.gbLabel.Controls.Add(this.nudMultiplicity);
            this.gbLabel.Controls.Add(this.lblMultiplicity);
            this.gbLabel.Controls.Add(this.rbCustomLabel);
            this.gbLabel.Controls.Add(this.rbPredefLabel);
            this.gbLabel.Controls.Add(this.lblPredefLabel);
            this.gbLabel.Controls.Add(this.comboboxPredefLabel);
            this.gbLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gbLabel.Location = new System.Drawing.Point(5, 5);
            this.gbLabel.Name = "gbLabel";
            this.gbLabel.Size = new System.Drawing.Size(1146, 313);
            this.gbLabel.TabIndex = 16;
            this.gbLabel.TabStop = false;
            this.gbLabel.Text = "Labeling Scheme";
            // 
            // btnAddLabelSite
            // 
            this.btnAddLabelSite.Enabled = false;
            this.btnAddLabelSite.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddLabelSite.Location = new System.Drawing.Point(343, 159);
            this.btnAddLabelSite.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddLabelSite.Name = "btnAddLabelSite";
            this.btnAddLabelSite.Size = new System.Drawing.Size(117, 23);
            this.btnAddLabelSite.TabIndex = 17;
            this.btnAddLabelSite.Text = "Add Label Site";
            this.btnAddLabelSite.UseVisualStyleBackColor = true;
            // 
            // dgvLabelScheme
            // 
            this.dgvLabelScheme.AllowUserToAddRows = false;
            this.dgvLabelScheme.AllowUserToDeleteRows = false;
            this.dgvLabelScheme.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLabelScheme.GridColor = System.Drawing.SystemColors.AppWorkspace;
            this.dgvLabelScheme.Location = new System.Drawing.Point(472, 28);
            this.dgvLabelScheme.Name = "dgvLabelScheme";
            this.dgvLabelScheme.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dgvLabelScheme.RowTemplate.Height = 23;
            this.dgvLabelScheme.Size = new System.Drawing.Size(669, 279);
            this.dgvLabelScheme.TabIndex = 16;
            this.dgvLabelScheme.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLabelScheme_CellContentClick);
            // 
            // btnLoadLabelScheme
            // 
            this.btnLoadLabelScheme.Enabled = false;
            this.btnLoadLabelScheme.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnLoadLabelScheme.Location = new System.Drawing.Point(324, 284);
            this.btnLoadLabelScheme.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoadLabelScheme.Name = "btnLoadLabelScheme";
            this.btnLoadLabelScheme.Size = new System.Drawing.Size(135, 23);
            this.btnLoadLabelScheme.TabIndex = 15;
            this.btnLoadLabelScheme.Text = "Load Labeling Scheme";
            this.btnLoadLabelScheme.UseVisualStyleBackColor = true;
            // 
            // btnSaveLabelScheme
            // 
            this.btnSaveLabelScheme.Enabled = false;
            this.btnSaveLabelScheme.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnSaveLabelScheme.Location = new System.Drawing.Point(185, 284);
            this.btnSaveLabelScheme.Margin = new System.Windows.Forms.Padding(2);
            this.btnSaveLabelScheme.Name = "btnSaveLabelScheme";
            this.btnSaveLabelScheme.Size = new System.Drawing.Size(135, 23);
            this.btnSaveLabelScheme.TabIndex = 14;
            this.btnSaveLabelScheme.Text = "Save Labeling Scheme";
            this.btnSaveLabelScheme.UseVisualStyleBackColor = true;
            // 
            // lblAddLabelSite
            // 
            this.lblAddLabelSite.AutoSize = true;
            this.lblAddLabelSite.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblAddLabelSite.Location = new System.Drawing.Point(118, 162);
            this.lblAddLabelSite.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAddLabelSite.Name = "lblAddLabelSite";
            this.lblAddLabelSite.Size = new System.Drawing.Size(76, 13);
            this.lblAddLabelSite.TabIndex = 13;
            this.lblAddLabelSite.Text = "Add Label Site";
            // 
            // comboboxLabelSite
            // 
            this.comboboxLabelSite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxLabelSite.Enabled = false;
            this.comboboxLabelSite.FormattingEnabled = true;
            this.comboboxLabelSite.Location = new System.Drawing.Point(219, 160);
            this.comboboxLabelSite.Name = "comboboxLabelSite";
            this.comboboxLabelSite.Size = new System.Drawing.Size(117, 21);
            this.comboboxLabelSite.TabIndex = 12;
            // 
            // nudMultiplicity
            // 
            this.nudMultiplicity.Enabled = false;
            this.nudMultiplicity.Location = new System.Drawing.Point(219, 199);
            this.nudMultiplicity.Margin = new System.Windows.Forms.Padding(2);
            this.nudMultiplicity.Name = "nudMultiplicity";
            this.nudMultiplicity.Size = new System.Drawing.Size(117, 20);
            this.nudMultiplicity.TabIndex = 11;
            this.nudMultiplicity.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lblMultiplicity
            // 
            this.lblMultiplicity.AutoSize = true;
            this.lblMultiplicity.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblMultiplicity.Location = new System.Drawing.Point(120, 199);
            this.lblMultiplicity.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMultiplicity.Name = "lblMultiplicity";
            this.lblMultiplicity.Size = new System.Drawing.Size(74, 13);
            this.lblMultiplicity.TabIndex = 10;
            this.lblMultiplicity.Text = "Set Multiplicity";
            // 
            // rbCustomLabel
            // 
            this.rbCustomLabel.AutoSize = true;
            this.rbCustomLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rbCustomLabel.Location = new System.Drawing.Point(25, 124);
            this.rbCustomLabel.Margin = new System.Windows.Forms.Padding(2);
            this.rbCustomLabel.Name = "rbCustomLabel";
            this.rbCustomLabel.Size = new System.Drawing.Size(167, 17);
            this.rbCustomLabel.TabIndex = 6;
            this.rbCustomLabel.Text = "Use Custom Labeling Scheme";
            this.rbCustomLabel.UseVisualStyleBackColor = true;
            this.rbCustomLabel.Click += new System.EventHandler(this.RbCustomLabel_Click);
            // 
            // rbPredefLabel
            // 
            this.rbPredefLabel.AutoSize = true;
            this.rbPredefLabel.Checked = true;
            this.rbPredefLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rbPredefLabel.Location = new System.Drawing.Point(25, 28);
            this.rbPredefLabel.Margin = new System.Windows.Forms.Padding(2);
            this.rbPredefLabel.Name = "rbPredefLabel";
            this.rbPredefLabel.Size = new System.Drawing.Size(183, 17);
            this.rbPredefLabel.TabIndex = 5;
            this.rbPredefLabel.TabStop = true;
            this.rbPredefLabel.Text = "Use Predefined Labeling Scheme";
            this.rbPredefLabel.UseVisualStyleBackColor = true;
            this.rbPredefLabel.Click += new System.EventHandler(this.RbPredefLabel_Click);
            // 
            // lblPredefLabel
            // 
            this.lblPredefLabel.AutoSize = true;
            this.lblPredefLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblPredefLabel.Location = new System.Drawing.Point(22, 53);
            this.lblPredefLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPredefLabel.Name = "lblPredefLabel";
            this.lblPredefLabel.Size = new System.Drawing.Size(173, 13);
            this.lblPredefLabel.TabIndex = 3;
            this.lblPredefLabel.Text = "Select PredefinedLabeling Scheme";
            // 
            // comboboxPredefLabel
            // 
            this.comboboxPredefLabel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxPredefLabel.FormattingEnabled = true;
            this.comboboxPredefLabel.Location = new System.Drawing.Point(219, 51);
            this.comboboxPredefLabel.Name = "comboboxPredefLabel";
            this.comboboxPredefLabel.Size = new System.Drawing.Size(241, 21);
            this.comboboxPredefLabel.TabIndex = 1;
            this.comboboxPredefLabel.SelectedIndexChanged += new System.EventHandler(this.ComboboxPredefLabel_SelectedIndexChanged);
            // 
            // gbRT
            // 
            this.gbRT.Controls.Add(this.rbNoRtShift);
            this.gbRT.Controls.Add(this.btnBrowseCustomRtStandard);
            this.gbRT.Controls.Add(this.tbCustomRtStandard);
            this.gbRT.Controls.Add(this.btnBrowseCustomRtModel);
            this.gbRT.Controls.Add(this.tbCustomRtModel);
            this.gbRT.Controls.Add(this.lblCustomRtStandard);
            this.gbRT.Controls.Add(this.lblCustomRtModel);
            this.gbRT.Controls.Add(this.rbCustomRt);
            this.gbRT.Controls.Add(this.rbBuiltinRt);
            this.gbRT.Controls.Add(this.lblBuiltinRt);
            this.gbRT.Controls.Add(this.comboboxLcType);
            this.gbRT.Location = new System.Drawing.Point(5, 324);
            this.gbRT.Name = "gbRT";
            this.gbRT.Size = new System.Drawing.Size(768, 211);
            this.gbRT.TabIndex = 2;
            this.gbRT.TabStop = false;
            this.gbRT.Text = "RT Shift Prediction";
            // 
            // rbNoRtShift
            // 
            this.rbNoRtShift.AutoSize = true;
            this.rbNoRtShift.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rbNoRtShift.Location = new System.Drawing.Point(25, 179);
            this.rbNoRtShift.Name = "rbNoRtShift";
            this.rbNoRtShift.Size = new System.Drawing.Size(162, 17);
            this.rbNoRtShift.TabIndex = 15;
            this.rbNoRtShift.TabStop = true;
            this.rbNoRtShift.Text = "Don\'t Use RT shift Prediction";
            this.rbNoRtShift.UseVisualStyleBackColor = true;
            this.rbNoRtShift.Click += new System.EventHandler(this.rbNoRtShift_Click);
            // 
            // btnBrowseCustomRtStandard
            // 
            this.btnBrowseCustomRtStandard.Enabled = false;
            this.btnBrowseCustomRtStandard.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnBrowseCustomRtStandard.Location = new System.Drawing.Point(693, 139);
            this.btnBrowseCustomRtStandard.Name = "btnBrowseCustomRtStandard";
            this.btnBrowseCustomRtStandard.Size = new System.Drawing.Size(61, 23);
            this.btnBrowseCustomRtStandard.TabIndex = 14;
            this.btnBrowseCustomRtStandard.Text = "Browse";
            this.btnBrowseCustomRtStandard.UseVisualStyleBackColor = true;
            this.btnBrowseCustomRtStandard.Click += new System.EventHandler(this.BtnBrowseRtStandard_Click);
            // 
            // tbCustomRtStandard
            // 
            this.tbCustomRtStandard.Enabled = false;
            this.tbCustomRtStandard.Location = new System.Drawing.Point(185, 141);
            this.tbCustomRtStandard.Name = "tbCustomRtStandard";
            this.tbCustomRtStandard.Size = new System.Drawing.Size(502, 20);
            this.tbCustomRtStandard.TabIndex = 13;
            // 
            // btnBrowseCustomRtModel
            // 
            this.btnBrowseCustomRtModel.Enabled = false;
            this.btnBrowseCustomRtModel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnBrowseCustomRtModel.Location = new System.Drawing.Point(693, 109);
            this.btnBrowseCustomRtModel.Name = "btnBrowseCustomRtModel";
            this.btnBrowseCustomRtModel.Size = new System.Drawing.Size(61, 23);
            this.btnBrowseCustomRtModel.TabIndex = 12;
            this.btnBrowseCustomRtModel.Text = "Browse";
            this.btnBrowseCustomRtModel.UseVisualStyleBackColor = true;
            this.btnBrowseCustomRtModel.Click += new System.EventHandler(this.BtnBrowseRtModel_Click);
            // 
            // tbCustomRtModel
            // 
            this.tbCustomRtModel.Enabled = false;
            this.tbCustomRtModel.Location = new System.Drawing.Point(185, 111);
            this.tbCustomRtModel.Name = "tbCustomRtModel";
            this.tbCustomRtModel.Size = new System.Drawing.Size(502, 20);
            this.tbCustomRtModel.TabIndex = 11;
            // 
            // lblCustomRtStandard
            // 
            this.lblCustomRtStandard.AutoSize = true;
            this.lblCustomRtStandard.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblCustomRtStandard.Location = new System.Drawing.Point(22, 141);
            this.lblCustomRtStandard.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCustomRtStandard.Name = "lblCustomRtStandard";
            this.lblCustomRtStandard.Size = new System.Drawing.Size(144, 13);
            this.lblCustomRtStandard.TabIndex = 10;
            this.lblCustomRtStandard.Text = "Select RT Shift Standard File";
            // 
            // lblCustomRtModel
            // 
            this.lblCustomRtModel.AutoSize = true;
            this.lblCustomRtModel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblCustomRtModel.Location = new System.Drawing.Point(22, 114);
            this.lblCustomRtModel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCustomRtModel.Name = "lblCustomRtModel";
            this.lblCustomRtModel.Size = new System.Drawing.Size(130, 13);
            this.lblCustomRtModel.TabIndex = 8;
            this.lblCustomRtModel.Text = "Select RT Shift Model File";
            // 
            // rbCustomRt
            // 
            this.rbCustomRt.AutoSize = true;
            this.rbCustomRt.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rbCustomRt.Location = new System.Drawing.Point(25, 89);
            this.rbCustomRt.Margin = new System.Windows.Forms.Padding(2);
            this.rbCustomRt.Name = "rbCustomRt";
            this.rbCustomRt.Size = new System.Drawing.Size(156, 17);
            this.rbCustomRt.TabIndex = 6;
            this.rbCustomRt.Text = "Use Custom RT Shift Model";
            this.rbCustomRt.UseVisualStyleBackColor = true;
            this.rbCustomRt.Click += new System.EventHandler(this.rbCustomRt_Click);
            // 
            // rbBuiltinRt
            // 
            this.rbBuiltinRt.AutoSize = true;
            this.rbBuiltinRt.Checked = true;
            this.rbBuiltinRt.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rbBuiltinRt.Location = new System.Drawing.Point(25, 28);
            this.rbBuiltinRt.Margin = new System.Windows.Forms.Padding(2);
            this.rbBuiltinRt.Name = "rbBuiltinRt";
            this.rbBuiltinRt.Size = new System.Drawing.Size(152, 17);
            this.rbBuiltinRt.TabIndex = 5;
            this.rbBuiltinRt.TabStop = true;
            this.rbBuiltinRt.Text = "Use Built-in RT Shift Model";
            this.rbBuiltinRt.UseVisualStyleBackColor = true;
            this.rbBuiltinRt.Click += new System.EventHandler(this.rbBuiltinRt_Click);
            // 
            // lblBuiltinRt
            // 
            this.lblBuiltinRt.AutoSize = true;
            this.lblBuiltinRt.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblBuiltinRt.Location = new System.Drawing.Point(22, 53);
            this.lblBuiltinRt.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblBuiltinRt.Name = "lblBuiltinRt";
            this.lblBuiltinRt.Size = new System.Drawing.Size(145, 13);
            this.lblBuiltinRt.TabIndex = 3;
            this.lblBuiltinRt.Text = "Select Built-in RT Shift Model";
            // 
            // comboboxLcType
            // 
            this.comboboxLcType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboboxLcType.FormattingEnabled = true;
            this.comboboxLcType.Location = new System.Drawing.Point(185, 50);
            this.comboboxLcType.Name = "comboboxLcType";
            this.comboboxLcType.Size = new System.Drawing.Size(569, 21);
            this.comboboxLcType.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.AutoScroll = true;
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.gbAddOpts);
            this.tabPage2.Controls.Add(this.gbOutOptions);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(1156, 540);
            this.tabPage2.TabIndex = 0;
            this.tabPage2.Text = "File Path Configuration";
            // 
            // gbAddOpts
            // 
            this.gbAddOpts.Controls.Add(this.btnBrowseFasta);
            this.gbAddOpts.Controls.Add(this.tbFasta);
            this.gbAddOpts.Controls.Add(this.lblFasta);
            this.gbAddOpts.Location = new System.Drawing.Point(585, 412);
            this.gbAddOpts.Name = "gbAddOpts";
            this.gbAddOpts.Size = new System.Drawing.Size(563, 120);
            this.gbAddOpts.TabIndex = 2;
            this.gbAddOpts.TabStop = false;
            this.gbAddOpts.Text = "Aditional Input Files";
            // 
            // btnBrowseFasta
            // 
            this.btnBrowseFasta.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnBrowseFasta.Location = new System.Drawing.Point(494, 24);
            this.btnBrowseFasta.Name = "btnBrowseFasta";
            this.btnBrowseFasta.Size = new System.Drawing.Size(61, 23);
            this.btnBrowseFasta.TabIndex = 6;
            this.btnBrowseFasta.Text = "Browse";
            this.btnBrowseFasta.UseVisualStyleBackColor = true;
            this.btnBrowseFasta.Click += new System.EventHandler(this.BtnBrowseFasta_Click);
            // 
            // tbFasta
            // 
            this.tbFasta.Location = new System.Drawing.Point(108, 26);
            this.tbFasta.Name = "tbFasta";
            this.tbFasta.Size = new System.Drawing.Size(379, 20);
            this.tbFasta.TabIndex = 4;
            // 
            // lblFasta
            // 
            this.lblFasta.AutoSize = true;
            this.lblFasta.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblFasta.Location = new System.Drawing.Point(5, 29);
            this.lblFasta.Name = "lblFasta";
            this.lblFasta.Size = new System.Drawing.Size(97, 13);
            this.lblFasta.TabIndex = 5;
            this.lblFasta.Text = "RevCat Fasta Path";
            // 
            // gbOutOptions
            // 
            this.gbOutOptions.Controls.Add(this.btnBrowseXicDir);
            this.gbOutOptions.Controls.Add(this.btnBrowseOutDir);
            this.gbOutOptions.Controls.Add(this.tbXicMfile);
            this.gbOutOptions.Controls.Add(this.lblXicMfile);
            this.gbOutOptions.Controls.Add(this.cbWriteExcel);
            this.gbOutOptions.Controls.Add(this.lblOutDir);
            this.gbOutOptions.Controls.Add(this.cbWriteXic);
            this.gbOutOptions.Controls.Add(this.tbOutPath);
            this.gbOutOptions.Location = new System.Drawing.Point(4, 412);
            this.gbOutOptions.Name = "gbOutOptions";
            this.gbOutOptions.Size = new System.Drawing.Size(563, 120);
            this.gbOutOptions.TabIndex = 1;
            this.gbOutOptions.TabStop = false;
            this.gbOutOptions.Text = "Output Options";
            // 
            // btnBrowseXicDir
            // 
            this.btnBrowseXicDir.Enabled = false;
            this.btnBrowseXicDir.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnBrowseXicDir.Location = new System.Drawing.Point(494, 85);
            this.btnBrowseXicDir.Name = "btnBrowseXicDir";
            this.btnBrowseXicDir.Size = new System.Drawing.Size(61, 23);
            this.btnBrowseXicDir.TabIndex = 9;
            this.btnBrowseXicDir.Text = "Browse";
            this.btnBrowseXicDir.UseVisualStyleBackColor = true;
            this.btnBrowseXicDir.Click += new System.EventHandler(this.BtnBrowseXicDir_Click);
            // 
            // btnBrowseOutDir
            // 
            this.btnBrowseOutDir.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnBrowseOutDir.Location = new System.Drawing.Point(494, 20);
            this.btnBrowseOutDir.Name = "btnBrowseOutDir";
            this.btnBrowseOutDir.Size = new System.Drawing.Size(61, 23);
            this.btnBrowseOutDir.TabIndex = 3;
            this.btnBrowseOutDir.Text = "Browse";
            this.btnBrowseOutDir.UseVisualStyleBackColor = true;
            this.btnBrowseOutDir.Click += new System.EventHandler(this.BtnBrowseOutDir_Click);
            // 
            // tbXicMfile
            // 
            this.tbXicMfile.Enabled = false;
            this.tbXicMfile.Location = new System.Drawing.Point(233, 87);
            this.tbXicMfile.Name = "tbXicMfile";
            this.tbXicMfile.Size = new System.Drawing.Size(253, 20);
            this.tbXicMfile.TabIndex = 7;
            // 
            // lblXicMfile
            // 
            this.lblXicMfile.AutoSize = true;
            this.lblXicMfile.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblXicMfile.Location = new System.Drawing.Point(127, 89);
            this.lblXicMfile.Name = "lblXicMfile";
            this.lblXicMfile.Size = new System.Drawing.Size(93, 13);
            this.lblXicMfile.TabIndex = 8;
            this.lblXicMfile.Text = "XIC mfile Directory";
            // 
            // cbWriteExcel
            // 
            this.cbWriteExcel.AutoSize = true;
            this.cbWriteExcel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cbWriteExcel.Location = new System.Drawing.Point(8, 56);
            this.cbWriteExcel.Name = "cbWriteExcel";
            this.cbWriteExcel.Size = new System.Drawing.Size(270, 17);
            this.cbWriteExcel.TabIndex = 1;
            this.cbWriteExcel.Text = "Write Output in xlsx file format (MS Excel is required)";
            this.cbWriteExcel.UseVisualStyleBackColor = true;
            // 
            // lblOutDir
            // 
            this.lblOutDir.AutoSize = true;
            this.lblOutDir.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblOutDir.Location = new System.Drawing.Point(5, 25);
            this.lblOutDir.Name = "lblOutDir";
            this.lblOutDir.Size = new System.Drawing.Size(84, 13);
            this.lblOutDir.TabIndex = 2;
            this.lblOutDir.Text = "Output Directory";
            // 
            // cbWriteXic
            // 
            this.cbWriteXic.AutoSize = true;
            this.cbWriteXic.ForeColor = System.Drawing.SystemColors.ControlText;
            this.cbWriteXic.Location = new System.Drawing.Point(8, 89);
            this.cbWriteXic.Name = "cbWriteXic";
            this.cbWriteXic.Size = new System.Drawing.Size(109, 17);
            this.cbWriteXic.TabIndex = 4;
            this.cbWriteXic.Text = "Write XIC .m Files";
            this.cbWriteXic.UseVisualStyleBackColor = true;
            this.cbWriteXic.CheckedChanged += new System.EventHandler(this.CbWriteXic_CheckedChanged);
            // 
            // tbOutPath
            // 
            this.tbOutPath.Location = new System.Drawing.Point(95, 22);
            this.tbOutPath.Name = "tbOutPath";
            this.tbOutPath.Size = new System.Drawing.Size(392, 20);
            this.tbOutPath.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.nudNumFrac);
            this.groupBox1.Controls.Add(this.lblNumFrac);
            this.groupBox1.Controls.Add(this.lvFiles);
            this.groupBox1.Controls.Add(this.btnRmRep);
            this.groupBox1.Controls.Add(this.btnRmCond);
            this.groupBox1.Controls.Add(this.btnLoadID);
            this.groupBox1.Controls.Add(this.btnLoadRaw);
            this.groupBox1.Controls.Add(this.btnAddRep);
            this.groupBox1.Controls.Add(this.btnAddCond);
            this.groupBox1.Controls.Add(this.tvScheme);
            this.groupBox1.Location = new System.Drawing.Point(4, 5);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(1148, 389);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Set Expremental Scheme && Load Files";
            // 
            // nudNumFrac
            // 
            this.nudNumFrac.Location = new System.Drawing.Point(166, 361);
            this.nudNumFrac.Margin = new System.Windows.Forms.Padding(2);
            this.nudNumFrac.Name = "nudNumFrac";
            this.nudNumFrac.Size = new System.Drawing.Size(58, 20);
            this.nudNumFrac.TabIndex = 9;
            this.nudNumFrac.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudNumFrac.ValueChanged += new System.EventHandler(this.NudNumFrac_ValueChanged);
            // 
            // lblNumFrac
            // 
            this.lblNumFrac.AutoSize = true;
            this.lblNumFrac.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblNumFrac.Location = new System.Drawing.Point(4, 363);
            this.lblNumFrac.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblNumFrac.Name = "lblNumFrac";
            this.lblNumFrac.Size = new System.Drawing.Size(146, 13);
            this.lblNumFrac.TabIndex = 8;
            this.lblNumFrac.Text = "Number of Fractionated Runs";
            // 
            // lvFiles
            // 
            this.lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.lvFiles.FullRowSelect = true;
            this.lvFiles.GridLines = true;
            this.lvFiles.HideSelection = false;
            this.lvFiles.Location = new System.Drawing.Point(238, 17);
            this.lvFiles.Margin = new System.Windows.Forms.Padding(2);
            this.lvFiles.Name = "lvFiles";
            this.lvFiles.Size = new System.Drawing.Size(906, 339);
            this.lvFiles.TabIndex = 7;
            this.lvFiles.UseCompatibleStateImageBehavior = false;
            this.lvFiles.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Index";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Condition";
            this.columnHeader2.Width = 90;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Replicate";
            this.columnHeader3.Width = 90;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Fraction";
            this.columnHeader4.Width = 90;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Raw File Name";
            this.columnHeader5.Width = 400;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "ID File Name";
            this.columnHeader6.Width = 400;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Raw File Full Path";
            this.columnHeader7.Width = 700;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "ID File Full Path";
            this.columnHeader8.Width = 750;
            // 
            // btnRmRep
            // 
            this.btnRmRep.AllowDrop = true;
            this.btnRmRep.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRmRep.Location = new System.Drawing.Point(119, 333);
            this.btnRmRep.Margin = new System.Windows.Forms.Padding(2);
            this.btnRmRep.Name = "btnRmRep";
            this.btnRmRep.Size = new System.Drawing.Size(105, 23);
            this.btnRmRep.TabIndex = 6;
            this.btnRmRep.Text = "Remove Replicate";
            this.btnRmRep.UseVisualStyleBackColor = true;
            this.btnRmRep.Click += new System.EventHandler(this.BtnRmRep_Click);
            // 
            // btnRmCond
            // 
            this.btnRmCond.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRmCond.Location = new System.Drawing.Point(119, 306);
            this.btnRmCond.Margin = new System.Windows.Forms.Padding(2);
            this.btnRmCond.Name = "btnRmCond";
            this.btnRmCond.Size = new System.Drawing.Size(105, 23);
            this.btnRmCond.TabIndex = 5;
            this.btnRmCond.Text = "Remove Condition";
            this.btnRmCond.UseVisualStyleBackColor = true;
            this.btnRmCond.Click += new System.EventHandler(this.BtnRmCond_Click);
            // 
            // btnLoadID
            // 
            this.btnLoadID.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnLoadID.Location = new System.Drawing.Point(1029, 362);
            this.btnLoadID.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoadID.Name = "btnLoadID";
            this.btnLoadID.Size = new System.Drawing.Size(115, 23);
            this.btnLoadID.TabIndex = 4;
            this.btnLoadID.Text = "Load ID Files";
            this.btnLoadID.UseVisualStyleBackColor = true;
            this.btnLoadID.Click += new System.EventHandler(this.BtnLoadID_Click);
            // 
            // btnLoadRaw
            // 
            this.btnLoadRaw.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnLoadRaw.Location = new System.Drawing.Point(910, 362);
            this.btnLoadRaw.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoadRaw.Name = "btnLoadRaw";
            this.btnLoadRaw.Size = new System.Drawing.Size(115, 23);
            this.btnLoadRaw.TabIndex = 3;
            this.btnLoadRaw.Text = "Load Spectrum Files";
            this.btnLoadRaw.UseVisualStyleBackColor = true;
            this.btnLoadRaw.Click += new System.EventHandler(this.BtnLoadRaw_Click);
            // 
            // btnAddRep
            // 
            this.btnAddRep.AllowDrop = true;
            this.btnAddRep.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddRep.Location = new System.Drawing.Point(4, 332);
            this.btnAddRep.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddRep.Name = "btnAddRep";
            this.btnAddRep.Size = new System.Drawing.Size(105, 23);
            this.btnAddRep.TabIndex = 2;
            this.btnAddRep.Text = "Add Replicate";
            this.btnAddRep.UseVisualStyleBackColor = true;
            this.btnAddRep.Click += new System.EventHandler(this.BtnAddRep_Click);
            // 
            // btnAddCond
            // 
            this.btnAddCond.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddCond.Location = new System.Drawing.Point(4, 306);
            this.btnAddCond.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddCond.Name = "btnAddCond";
            this.btnAddCond.Size = new System.Drawing.Size(105, 23);
            this.btnAddCond.TabIndex = 1;
            this.btnAddCond.Text = "Add Condition";
            this.btnAddCond.UseVisualStyleBackColor = true;
            this.btnAddCond.Click += new System.EventHandler(this.BtnAddCond_Click);
            // 
            // tvScheme
            // 
            this.tvScheme.Location = new System.Drawing.Point(4, 17);
            this.tvScheme.Margin = new System.Windows.Forms.Padding(2);
            this.tvScheme.Name = "tvScheme";
            this.tvScheme.Size = new System.Drawing.Size(220, 280);
            this.tvScheme.TabIndex = 0;
            this.tvScheme.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TvScheme_AfterSelect);
            // 
            // openMultiFileDialog
            // 
            this.openMultiFileDialog.Multiselect = true;
            // 
            // btnRun
            // 
            this.btnRun.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRun.Location = new System.Drawing.Point(1008, 582);
            this.btnRun.Margin = new System.Windows.Forms.Padding(2);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(79, 36);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.BtnRun_Click);
            // 
            // btnStop
            // 
            this.btnStop.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(1092, 581);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(79, 37);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // pgbRun
            // 
            this.pgbRun.Location = new System.Drawing.Point(526, 582);
            this.pgbRun.Name = "pgbRun";
            this.pgbRun.Size = new System.Drawing.Size(477, 36);
            this.pgbRun.Step = 100;
            this.pgbRun.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pgbRun.TabIndex = 3;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.Location = new System.Drawing.Point(13, 591);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(90, 20);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Text = "Status: Idle";
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(1099, 10);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(69, 13);
            this.lblVersion.TabIndex = 5;
            this.lblVersion.Text = "Version 0.1.0";
            this.lblVersion.Click += new System.EventHandler(this.lblVersion_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(1184, 622);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pgbRun);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.mainTabCtrl);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "MainForm";
            this.Text = "EPIQ";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainTabCtrl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.gbLabel.ResumeLayout(false);
            this.gbLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLabelScheme)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMultiplicity)).EndInit();
            this.gbRT.ResumeLayout(false);
            this.gbRT.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.gbAddOpts.ResumeLayout(false);
            this.gbAddOpts.PerformLayout();
            this.gbOutOptions.ResumeLayout(false);
            this.gbOutOptions.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumFrac)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl mainTabCtrl;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TreeView tvScheme;
        private System.Windows.Forms.Button btnAddRep;
        private System.Windows.Forms.Button btnAddCond;
        private System.Windows.Forms.Button btnLoadID;
        private System.Windows.Forms.Button btnLoadRaw;
        private System.Windows.Forms.Button btnRmRep;
        private System.Windows.Forms.Button btnRmCond;
        private System.Windows.Forms.ListView lvFiles;
        private System.Windows.Forms.NumericUpDown nudNumFrac;
        private System.Windows.Forms.Label lblNumFrac;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.OpenFileDialog openMultiFileDialog;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ProgressBar pgbRun;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox gbAddOpts;
        private System.Windows.Forms.GroupBox gbOutOptions;
        private System.Windows.Forms.Button btnBrowseOutDir;
        private System.Windows.Forms.CheckBox cbWriteExcel;
        private System.Windows.Forms.Label lblOutDir;
        private System.Windows.Forms.TextBox tbOutPath;
        private System.Windows.Forms.Button btnBrowseFasta;
        private System.Windows.Forms.TextBox tbFasta;
        private System.Windows.Forms.Label lblFasta;
        private System.Windows.Forms.CheckBox cbWriteXic;
        private System.Windows.Forms.Button btnBrowseXicDir;
        private System.Windows.Forms.TextBox tbXicMfile;
        private System.Windows.Forms.Label lblXicMfile;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.OpenFileDialog openSingleFileDialog;
        private System.Windows.Forms.ComboBox comboboxLcType;
        private System.Windows.Forms.GroupBox gbRT;
        private System.Windows.Forms.Label lblBuiltinRt;
        private System.Windows.Forms.RadioButton rbCustomRt;
        private System.Windows.Forms.RadioButton rbBuiltinRt;
        private System.Windows.Forms.Button btnBrowseCustomRtStandard;
        private System.Windows.Forms.TextBox tbCustomRtStandard;
        private System.Windows.Forms.Button btnBrowseCustomRtModel;
        private System.Windows.Forms.TextBox tbCustomRtModel;
        private System.Windows.Forms.Label lblCustomRtStandard;
        private System.Windows.Forms.Label lblCustomRtModel;
        private System.Windows.Forms.RadioButton rbNoRtShift;
        private System.Windows.Forms.GroupBox gbLabel;
        private System.Windows.Forms.RadioButton rbCustomLabel;
        private System.Windows.Forms.RadioButton rbPredefLabel;
        private System.Windows.Forms.Label lblPredefLabel;
        private System.Windows.Forms.ComboBox comboboxPredefLabel;
        private System.Windows.Forms.Label lblAddLabelSite;
        private System.Windows.Forms.ComboBox comboboxLabelSite;
        private System.Windows.Forms.NumericUpDown nudMultiplicity;
        private System.Windows.Forms.Label lblMultiplicity;
        private System.Windows.Forms.DataGridView dgvLabelScheme;
        private System.Windows.Forms.Button btnLoadLabelScheme;
        private System.Windows.Forms.Button btnSaveLabelScheme;
        private System.Windows.Forms.Button btnAddLabelSite;
        private System.Windows.Forms.Label lblVersion;

    }
}

