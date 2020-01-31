using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Epiq;

namespace EPIQgui
{
    public partial class MainForm : Form
    {
        private string xicMfileDirPath = "";
        private string outDirPath = "";

        public MainForm()
        {
            InitializeComponent();
            InitializeExTree();
            InitializeLvFiles();
            InitializeComboboxLcType();
            InitializeLabelScheme();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void InitializeLvFiles()
        {
            TreeToLvFiles();
        }


        private void InitializeExTree()
        {
            AddCondition();
        }

        private void InitializeComboboxLcType()
        {
            comboboxLcType.Items.AddRange(BuiltInRtModels.RtModelLcTypes);
            comboboxLcType.SelectedIndex = 0;
        }




        private void TreeToLvFiles()
        {
            lvFiles.Items.Clear();
            int indexCounter = 1;
            foreach (TreeNode condNode in tvScheme.Nodes)
            {
                foreach (TreeNode repNode in condNode.Nodes)
                {
                    var specFilePaths = (SpecFilePath[]) repNode.Tag;
                    for (int i=1; i < specFilePaths.Length+1; i++)
                    {
                        lvFiles.Items.Add(
                            new ListViewItem(
                                new string[]
                                {
                                    Convert.ToString(indexCounter++),
                                    condNode.Text.Split(' ')[1],
                                    repNode.Text.Split(' ')[1],
                                    i.ToString(),
                                    specFilePaths[i - 1].RawFileName,
                                    specFilePaths[i - 1].IdFileName,
                                    specFilePaths[i - 1].RawPath,
                                    specFilePaths[i - 1].IdPath,
                                }));
                    }
                }
            }
        }


        #region SetLabelScheme

        private void InitializeLabelScheme()
        {
            comboboxLabelSite.Items.AddRange(LabelingSchemes.LabelSites.ToArray());
            comboboxLabelSite.SelectedIndex = 0;

            comboboxPredefLabel.Items.AddRange(LabelingSchemes.PredefinedLabelingSchemes);
            comboboxPredefLabel.SelectedIndex = 0;

            LabelSchemeStringToDgvLabelScheme(LabelingSchemes.PredefinedLabelingSchemeToLabelStrings[comboboxPredefLabel.SelectedItem.ToString()]);
        }

        private void ComboboxPredefLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            LabelSchemeStringToDgvLabelScheme(LabelingSchemes.PredefinedLabelingSchemeToLabelStrings[comboboxPredefLabel.SelectedItem.ToString()]);
        }


        private void RbPredefLabel_Click(object sender, EventArgs e)
        {
            lblPredefLabel.ForeColor = SystemColors.ControlText;
            comboboxPredefLabel.Enabled = true;

            lblAddLabelSite.ForeColor = SystemColors.GrayText;
            lblMultiplicity.ForeColor = SystemColors.GrayText;
            btnAddLabelSite.Enabled = false;
            comboboxLabelSite.Enabled = false;
            nudMultiplicity.Enabled = false;
            LabelSchemeStringToDgvLabelScheme(LabelingSchemes.PredefinedLabelingSchemeToLabelStrings[comboboxPredefLabel.SelectedItem.ToString()]);

            btnSaveLabelScheme.Enabled = false;
            btnLoadLabelScheme.Enabled = false;
        }

        private void RbCustomLabel_Click(object sender, EventArgs e)
        {
            lblPredefLabel.ForeColor = SystemColors.GrayText;
            comboboxPredefLabel.Enabled = false;

            lblAddLabelSite.ForeColor = SystemColors.ControlText;
            lblMultiplicity.ForeColor = SystemColors.ControlText;
            btnAddLabelSite.Enabled = true;
            comboboxLabelSite.Enabled = true;
            nudMultiplicity.Enabled = true;
            dgvLabelScheme.Rows.Clear();
            dgvLabelScheme.Columns.Clear();

            btnSaveLabelScheme.Enabled = true;
            btnLoadLabelScheme.Enabled = true;
        }


        private void LabelSchemeStringToDgvLabelScheme(string[] labelSchemeStrings)
        {
            List<List<string>> rowInfos = new List<List<string>>();
            List<string> labelSiteNames = new List<string>();
            dgvLabelScheme.Rows.Clear();
            dgvLabelScheme.Columns.Clear();
            if (comboboxPredefLabel.SelectedItem.ToString()==LabelingSchemes.LabelFreeName)
                return;

            for (var i=0; i<labelSchemeStrings.Length; i++)
            {
                var fields  = labelSchemeStrings[i].Split(' ');
                var labelName = LabelingSchemes.DictLabelSitesToLong[fields[0]];
                labelSiteNames.Add(labelName);
                dgvLabelScheme.Columns.Add(labelName + "_Attached Deuteriums", "Attached Deuteriums");
                dgvLabelScheme.Columns[2*i].Tag = labelName;
                dgvLabelScheme.Columns.Add(labelName + "_Increased Mass by Label(Da)", "Increased Mass by Label(Da)");
                dgvLabelScheme.Columns[2*i+1].Tag = labelName;

                for (var j= 1; j<fields.Length; j++)
                {
                    if (i == 0)
                        rowInfos.Add(fields[j].Split('_').ToList());
                    else
                        rowInfos[j-1].AddRange(fields[j].Split('_').ToList());
                }
            }

            for (var i=0; i<rowInfos.Count; i++)
            {
                dgvLabelScheme.Rows.Add(rowInfos[i].ToArray());
                dgvLabelScheme.Rows[i].HeaderCell.Value = "Label " + (i + 1);
                dgvLabelScheme.Rows[i].ReadOnly = true;
            }

            foreach (DataGridViewColumn column in dgvLabelScheme.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
                column.ReadOnly = true;
            }
           
            dgvLabelScheme.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            var autosizedHeight = dgvLabelScheme.ColumnHeadersHeight;
            dgvLabelScheme.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvLabelScheme.ColumnHeadersHeight = (int) (autosizedHeight * 1.667);
            dgvLabelScheme.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgvLabelScheme.CellPainting += new DataGridViewCellPaintingEventHandler(dgvLabelScheme_CellPainting);
            dgvLabelScheme.Paint += new PaintEventHandler(dgvLabelScheme_Paint);
            dgvLabelScheme.Scroll += new ScrollEventHandler(dgvLabelScheme_Scroll);
            dgvLabelScheme.ColumnWidthChanged += new DataGridViewColumnEventHandler(dgvLabelScheme_ColumnWidthChanged);
        }


        private void dgvLabelScheme_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            Rectangle rtHeader = dgvLabelScheme.DisplayRectangle;
            rtHeader.Height = dgvLabelScheme.ColumnHeadersHeight / 3;
            dgvLabelScheme.Invalidate(rtHeader);
        }

        private void dgvLabelScheme_Scroll(object sender, ScrollEventArgs e)
        {
            Rectangle rtHeader = dgvLabelScheme.DisplayRectangle;
            rtHeader.Height = dgvLabelScheme.ColumnHeadersHeight / 3;
            dgvLabelScheme.Invalidate(rtHeader);
        }

        private void dgvLabelScheme_Paint(object sender, PaintEventArgs e)
        {
            var labelSites = dgvLabelScheme.Columns.Cast<DataGridViewColumn>().Select(col => col.Tag.ToString()).ToArray();
            for (int j = 0; j < labelSites.Length; )
            {
                Rectangle r1 = dgvLabelScheme.GetCellDisplayRectangle(j, -1, true);
                int w2 = dgvLabelScheme.GetCellDisplayRectangle(j + 1, -1, true).Width;
                r1.X += 1;
                r1.Y += 1;
                r1.Width = r1.Width + w2 - 2;
                r1.Height = r1.Height / 3 - 2;
                e.Graphics.FillRectangle(new SolidBrush(dgvLabelScheme.ColumnHeadersDefaultCellStyle.BackColor), r1);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(labelSites[j],
                dgvLabelScheme.ColumnHeadersDefaultCellStyle.Font,
                new SolidBrush(dgvLabelScheme.ColumnHeadersDefaultCellStyle.ForeColor),
                r1,
                format);
                j += 2;
            }
        }

        private void dgvLabelScheme_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex > -1)
            {
                Rectangle r2 = e.CellBounds;
                r2.Y += e.CellBounds.Height / 2;
                r2.Height = e.CellBounds.Height / 2;
                e.PaintBackground(r2, true);
                e.PaintContent(r2);
                e.Handled = true;
            }
        }


        

        #endregion




        #region AddCondOrRep

        private void BtnAddCond_Click(object sender, EventArgs e)
        {
            AddCondition();
            TreeToLvFiles();
        }

        private void BtnAddRep_Click(object sender, EventArgs e)
        {
            if (tvScheme.SelectedNode == null)
            {
                MessageBox.Show("Condition is not selected.\nPlease select a condition to add a replicate.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var condSelected = tvScheme.SelectedNode.FullPath.Count(f => f == '\\') == 0;
                if (condSelected)
                {
                    AddReplicate(tvScheme.SelectedNode);
                }
                else
                {
                    MessageBox.Show("Condition is not selected.\nPlease select a condition to add a replicate.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            TreeToLvFiles();
        }

        private void AddCondition()
        { var cNodeName = String.Format("Condition {0}", tvScheme.Nodes.Count+1);
            var cNode = tvScheme.Nodes.Add(cNodeName);

            AddReplicate(cNode);
        }

        private void AddReplicate(TreeNode condNode)
        {
            var crNodeName = String.Format("Replicate {0}", condNode.Nodes.Count + 1);
            condNode.Nodes.Add(crNodeName);
            condNode.ExpandAll();

            int fracCount = 1;
            var crfNodeName = fracCount == 1 ? String.Format("{0} Fractionated Run", fracCount) : String.Format("{0} Fractionated Runs", fracCount);
            TreeNode crNode = condNode.Nodes[condNode.Nodes.Count - 1];
            crNode.Nodes.Add(crfNodeName);
            crNode.ExpandAll();

            var specFilePaths = new SpecFilePath[fracCount];
            for (int i = 0; i < fracCount; i++)
                specFilePaths[i] = new SpecFilePath();
            crNode.Tag = specFilePaths;
        }

        

        #endregion


        #region RmCondOrRep

        
        private void BtnRmCond_Click(object sender, EventArgs e)
        {
            if (tvScheme.SelectedNode == null)
            {
                MessageBox.Show("Condition is not selected.\nPlease select a condition to be removed.",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RmCond();
            TreeToLvFiles();
        }


        private void BtnRmRep_Click(object sender, EventArgs e)
        {
            if (tvScheme.SelectedNode == null)
            {
                MessageBox.Show("Replicated is not selected.\nPlease select a replicate to be removed",
                @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RmRep();
            TreeToLvFiles();
        }


        private void RmCond()
        {

            ReasignNodeNumber();
        }


        private void RmRep()
        {

            ReasignNodeNumber();
        }


        private void ReasignNodeNumber()
        {
            
        }

        #endregion


        #region HandleFractionNumbers

        private void TvScheme_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvScheme.SelectedNode != null)
            {
                var nodeDepth = tvScheme.SelectedNode.FullPath.Count(f => f == '\\');
                if (nodeDepth == 1 || nodeDepth == 2)
                {
                    int curNum = 1;
                    if (nodeDepth == 1)
                    {
                        SpecFilePath[] paths = (SpecFilePath[]) tvScheme.SelectedNode.Tag;
                        curNum = paths.Length;
                    }
                    else if (nodeDepth == 2)
                    {
                        SpecFilePath[] paths = (SpecFilePath[]) tvScheme.SelectedNode.Parent.Tag;
                        curNum = paths.Length;
                    }
                    nudNumFrac.Value = curNum;
                }
            }
        }

        private void NudNumFrac_ValueChanged(object sender, EventArgs e)
        {
            if (tvScheme.SelectedNode != null)
            {
                var nodeDepth = tvScheme.SelectedNode.FullPath.Count(f => f == '\\');
                if (nodeDepth == 1 || nodeDepth == 2)
                {
                    if (nudNumFrac.Value < 1)
                    {
                        MessageBox.Show("At least 1 run per each replicate is required.",
                        @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        nudNumFrac.Value = 1;
                        return;
                    }

                    int prevNum;
                    TreeNode repNode = null;
                    if (nodeDepth == 1)
                    {
                        repNode = tvScheme.SelectedNode;
                    }
                    else
                    if (nodeDepth == 2)
                    {
                        repNode = tvScheme.SelectedNode.Parent;
                    }
                    SpecFilePath[] paths = (SpecFilePath[]) repNode.Tag;
                    prevNum = paths.Length;
                    ChangeFractions(prevNum, Convert.ToInt32(nudNumFrac.Value), repNode);
                    return;
                }
            }
            MessageBox.Show("Replicate is not selected.\nPlease select a replicate or it's child \"fractionated run\" node to add a replicate.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);


        }

        private void ChangeFractions(int prevNum, int curNum, TreeNode repNode)
        {
            var prevSpecFilePaths = (SpecFilePath[]) repNode.Tag;
            var newSpecFilePaths = new SpecFilePath[curNum];
            for (var i = 0; i < Math.Min(prevNum, curNum); i++)
            {
                newSpecFilePaths[i] = prevSpecFilePaths[i];
            }
            for (var i = prevNum; i < curNum; i++)
            {
                newSpecFilePaths[i] = new SpecFilePath();
            }
            repNode.Tag = newSpecFilePaths;
            repNode.Nodes[0].Text = curNum == 1 ?
                                    String.Format("{0} Fractionated Run", curNum) :
                                    String.Format("{0} Fractionated Runs", curNum);
            TreeToLvFiles();
        }

        

        #endregion


        #region LoadFiles

        private void BtnLoadRaw_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count > 0)
            {
                openMultiFileDialog.Title = String.Format("Select {0} Raw Files", lvFiles.SelectedItems.Count);
                openMultiFileDialog.Filter = "Thermo raw files (*.raw)|*.raw|mzML files(*.mzml)|*.mzml|All files (*.*)|*.*";
                while (true)
                {
                    if (openMultiFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (openMultiFileDialog.FileNames.Length != lvFiles.SelectedItems.Count)
                        {
                            var moreLess = openMultiFileDialog.FileNames.Length < lvFiles.SelectedItems.Count
                                ? "more"
                                : "less";
                            MessageBox.Show(
                                String.Format(
                                    "You selected {0} rows in the list, but loaded {1} files.\nPlease select {2} number of files.",
                                    lvFiles.SelectedItems.Count, openMultiFileDialog.FileNames.Length, moreLess),
                                @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }

                        for (var i = 0; i < lvFiles.SelectedItems.Count; i++)
                        {
                            var rawPath = openMultiFileDialog.FileNames[i];
                            var listItem = lvFiles.SelectedItems[i];
                            var condIdx = Convert.ToInt32(listItem.SubItems[1].Text) - 1;
                            var repIdx = Convert.ToInt32(listItem.SubItems[2].Text) - 1;
                            var fracIdx = Convert.ToInt32(listItem.SubItems[3].Text) - 1;
                            SpecFilePath[] repSpecFilePathArr =
                                (SpecFilePath[]) tvScheme.Nodes[condIdx].Nodes[repIdx].Tag;
                            repSpecFilePathArr[fracIdx].AddRawPath(rawPath);
                        }
                        TreeToLvFiles();
                        return;
                    }
                    return;
                }
            }
            else
            {
                MessageBox.Show(@"Please select more than one row from above list",
                                @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BtnLoadID_Click(object sender, EventArgs e)
        {
            if (lvFiles.SelectedItems.Count > 0)
            {
                openMultiFileDialog.Title = String.Format("Select {0} ID Files", lvFiles.SelectedItems.Count);
                openMultiFileDialog.Filter = "tab separated txt files (*.txt;*.tsv)|*.txt;*.tsv|All files (*.*)|*.*";
                while (true)
                {
                    if (openMultiFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (openMultiFileDialog.FileNames.Length != lvFiles.SelectedItems.Count)
                        {
                            var moreLess = openMultiFileDialog.FileNames.Length < lvFiles.SelectedItems.Count ? "more" : "less";
                            MessageBox.Show(String.Format("You selected {0} rows in the list, but loaded {1} files.\nPlease select {2} number of files.",
                                lvFiles.SelectedItems.Count, openMultiFileDialog.FileNames.Length, moreLess),
                            @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }

                        for (var i = 0; i < lvFiles.SelectedItems.Count; i++)
                        {
                            var idPath = openMultiFileDialog.FileNames[i];
                            var listItem = lvFiles.SelectedItems[i];
                            var condIdx = Convert.ToInt32(listItem.SubItems[1].Text) - 1;
                            var repIdx = Convert.ToInt32(listItem.SubItems[2].Text) - 1;
                            var fracIdx = Convert.ToInt32(listItem.SubItems[3].Text) - 1;
                            SpecFilePath[] repSpecFilePathArr = (SpecFilePath[]) tvScheme.Nodes[condIdx].Nodes[repIdx].Tag;
                            repSpecFilePathArr[fracIdx].AddIdPath(idPath);
                        }
                        TreeToLvFiles();
                        return;
                    }
                    return;
                }
            }
            else
            {
                MessageBox.Show("Please select more than one row from above list",
                                @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        #endregion


        #region Run

        private List<string> ListViewToRunParams()
        {
            var rowList = new List<string>();
            foreach (ListViewItem item in lvFiles.Items)
            {
                var fields = new List<string>();
                fields.Add(String.Format("Spec File {0}", item.SubItems[0].Text));
                fields.Add(item.SubItems[1].Text);
                fields.Add(item.SubItems[2].Text);
                fields.Add(item.SubItems[3].Text);
                fields.Add(item.SubItems[6].Text);
                fields.Add(item.SubItems[7].Text);
                rowList.Add(String.Join("\t", fields));
            }
            return rowList;
        }

        private bool RawFileNotAllSet()
        {
            return false;
        }

        private bool IdFileNotAllSet()
        {
            return false;
        }

        private List<string> RtPredictionOptionToRunParams()
        {
            var ret = new List<string>();
            ret.Add("# RT Shift Prediction Option");

            if (rbNoRtShift.Checked)
            {
                ret.Add("RT Shift Prediction Option\t0");
                ret.Add("");
                return ret;
            }
            if (rbBuiltinRt.Checked)
            {
                ret.Add("RT Shift Prediction Option\t1");
                ret.Add(String.Format("RT Shift Model LC Type\t{0}", comboboxLcType.Text));
                var modelPaths = BuiltInRtModels.GetRtBuiltInRtModelPaths(comboboxLcType.Text);
                ret.Add(String.Format("RT Shift Model Path\t{0}", modelPaths[0]));
                ret.Add(String.Format("RT Shift Standard Path\t{0}", modelPaths[1]));
                ret.Add("");
                return ret;
            }
            if (rbCustomRt.Checked)
            {
                ret.Add("RT Shift Prediction Option\t2");
                ret.Add(String.Format("RT Shift Model Path\t{0}", tbCustomRtModel.Text));
                ret.Add(String.Format("RT Shift Standard Path\t{0}", tbCustomRtStandard.Text));
                ret.Add("");
                return ret;
            }
            throw new Exception("Must select one of RT shift options.");
        }

        private List<string> LabelingSchemeToRunParams()
        {
            var ret = new List<string>();
            ret.Add("# Label Strings");
            if (rbPredefLabel.Checked)
            {
                ret.Add("Use Predefined Labeling Scheme\tTrue");
                ret.Add(String.Format("Predefined Labeling Scheme\t{0}", comboboxPredefLabel.SelectedItem.ToString()));
            }
            else if (rbCustomLabel.Checked)
            {
                ret.Add("Use Predefined Labeling Scheme\tFalse");
            }
            return ret;
        }

        private bool WriteRunParams(out string runParamPath)
        {
            runParamPath = Path.Combine(outDirPath, "EPIQ_run_params.txt");
            if (outDirPath == "")
            {
                MessageBox.Show(@"Output directory is not properly set up.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            using (var writer = new StreamWriter(runParamPath))
            {
                if (tbFasta.Text == "")
                {
                    MessageBox.Show(@"revCat fasta file path is not properly set up.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (!File.Exists(tbFasta.Text))
                {
                    MessageBox.Show(@"revCat fasta file does not exists.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (cbWriteXic.Checked && xicMfileDirPath=="")
                {
                    MessageBox.Show(@"Xic file Output directory is not properly set up.", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                else if (RawFileNotAllSet() || IdFileNotAllSet())
                {
                    return false;
                }
                else
                {
                    foreach (var lblSchemeRow in LabelingSchemeToRunParams())
                    {
                        writer.WriteLine(lblSchemeRow);
                    }

                    writer.WriteLine("# Additional Information Files");
                    writer.WriteLine("Fasta File Path\t{0}", tbFasta.Text);
                    writer.WriteLine();

                    foreach (var rtFileRow in RtPredictionOptionToRunParams())
                    {
                        writer.WriteLine(rtFileRow);
                    }

                    writer.WriteLine("# Options");
                    writer.WriteLine("Tolerence\t{0}", "5");
                    writer.WriteLine("Write Excel\t{0}", cbWriteExcel.Checked);
                    writer.WriteLine("Write XIC .m Files\t{0}", cbWriteXic.Checked);
                    writer.WriteLine();

                    writer.WriteLine("# Output Dir");
                    writer.WriteLine("Quantification Output Dir\t{0}", outDirPath);
                    writer.WriteLine("XIC mfile Dir\t{0}", xicMfileDirPath);
                    writer.WriteLine();

                    writer.WriteLine("# Spectrum File Paths");
                    writer.WriteLine(String.Join("\t", new string[]{"Spec File Index", "Condition Index", "Replicate Index", "Fractionation Index", "Raw File Path", "ID File Path"}));
                    foreach (var specFileRow in ListViewToRunParams())
                    {
                        writer.WriteLine(specFileRow);
                    }
                    return true;
                }
            }
        }


        private void BtnRun_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Go!");
            string runParamPath;
            if (WriteRunParams(out runParamPath))
            {
                var runParams = new RunParams(runParamPath);
                runParams.Run();
            }
            MessageBox.Show(@"Done!", @"Done", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);


        }

        

        #endregion

        private void BtnBrowseOutDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = @"Select output directory";

            var pathList = new List<string[]>();
            foreach (ListViewItem item in lvFiles.Items)
            {
                if (item.SubItems[6].Text.Length > 1)
                    pathList.Add(item.SubItems[6].Text.Split(Path.DirectorySeparatorChar));
                if (item.SubItems[7].Text.Length > 1) 
                    pathList.Add(item.SubItems[7].Text.Split(Path.DirectorySeparatorChar));
            }
            if (pathList.Count == 1)
            {
                folderBrowserDialog.SelectedPath = String.Join(Path.DirectorySeparatorChar.ToString(), pathList[0]);
            }
            else if (pathList.Count > 1)
            {
                folderBrowserDialog.SelectedPath = String.Join(Path.DirectorySeparatorChar.ToString(), 
                    pathList.First().Take(pathList.Min(x => x.Length)).TakeWhile((dir, i) => pathList.All(p => p[i] == dir)));
            }

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outDirPath = folderBrowserDialog.SelectedPath + @"\";
                tbOutPath.Text = outDirPath;
            }
        }


        private void CbWriteXic_CheckedChanged(object sender, EventArgs e)
        {
            if (cbWriteXic.Checked)
            {
                tbXicMfile.Enabled = true;
                lblXicMfile.ForeColor = SystemColors.ControlText;
                btnBrowseXicDir.Enabled = true;
            }
            else
            {
                tbXicMfile.Enabled = false;
                lblXicMfile.ForeColor = SystemColors.GrayText;
                btnBrowseXicDir.Enabled = false;
            }
        }

        #region BrowseBtns
        private void BtnBrowseXicDir_Click(object sender, EventArgs e)
        {
            folderBrowserDialog.Description = @"Select XIC output directory";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                xicMfileDirPath = folderBrowserDialog.SelectedPath + @"\";
                tbXicMfile.Text = xicMfileDirPath;
            }
        }


        private void BtnBrowseFasta_Click(object sender, EventArgs e)
        {
            openSingleFileDialog.Title = "Select revCat.fasta File";
            openSingleFileDialog.Filter = "revCat fasta files (*.revCat.fasta;*.revCat.fa)|*.revCat.fasta;*.revCat.fa|All files (*.*)|*.*";
            if (openSingleFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbFasta.Text = openSingleFileDialog.FileNames[0];
            }
        }


        private void BtnBrowseRtModel_Click(object sender, EventArgs e)
        {
            openSingleFileDialog.Title = "Select Model File";
            openSingleFileDialog.Filter = "Model files (*.model)|*.model";
            if (openSingleFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbCustomRtModel.Text = openSingleFileDialog.FileNames[0];
            }
        }


        private void BtnBrowseRtStandard_Click(object sender, EventArgs e)
        {
            openSingleFileDialog.Title = "Select Standard File";
            openSingleFileDialog.Filter = "Standard files (*.standard)|*.standard";
            if (openSingleFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbCustomRtStandard.Text = openSingleFileDialog.FileNames[0];
            }
        }
        #endregion


        #region Radio Button RT


        private void rbBuiltinRt_Click(object sender, EventArgs e)
        {
            lblBuiltinRt.ForeColor = SystemColors.ControlText;
            comboboxLcType.Enabled = true;

            lblCustomRtModel.ForeColor = SystemColors.GrayText;
            lblCustomRtStandard.ForeColor = SystemColors.GrayText;
            tbCustomRtModel.Enabled = false;
            tbCustomRtStandard.Enabled = false;
            btnBrowseCustomRtModel.Enabled = false;
            btnBrowseCustomRtStandard.Enabled = false;
        }

        private void rbCustomRt_Click(object sender, EventArgs e)
        {
            lblCustomRtModel.ForeColor = SystemColors.ControlText;
            lblCustomRtStandard.ForeColor = SystemColors.ControlText;
            tbCustomRtModel.Enabled = true;
            tbCustomRtStandard.Enabled = true;
            btnBrowseCustomRtModel.Enabled = true;
            btnBrowseCustomRtStandard.Enabled = true;

            lblBuiltinRt.ForeColor = SystemColors.GrayText;
            comboboxLcType.Enabled = false;
        }

        private void rbNoRtShift_Click(object sender, EventArgs e)
        {
            lblBuiltinRt.ForeColor = SystemColors.GrayText;
            comboboxLcType.Enabled = false;
            lblCustomRtModel.ForeColor = SystemColors.GrayText;
            lblCustomRtStandard.ForeColor = SystemColors.GrayText;
            tbCustomRtModel.Enabled = false;
            tbCustomRtStandard.Enabled = false;
            btnBrowseCustomRtModel.Enabled = false;
            btnBrowseCustomRtStandard.Enabled = false;
        }

        

        #endregion


        #region ScaleListView

        private void ScaleListViewColumns(ListView listview, SizeF factor)
        {
            foreach (ColumnHeader column in listview.Columns)
            {
                column.Width = (int)Math.Round(column.Width * factor.Width);
            }
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            ScaleListViewColumns(lvFiles, factor);
        }







        #endregion

        private void dgvLabelScheme_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void lblVersion_Click(object sender, EventArgs e)
        {

        }
    }
}
