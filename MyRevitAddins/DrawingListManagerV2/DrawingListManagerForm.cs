using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mySettings = MEPUtils.DrawingListManagerV2.Properties.Settings;
using Color = System.Drawing.Color;

namespace MEPUtils.DrawingListManagerV2
{
    public partial class DrawingListManagerForm : Form
    {
        private string pathToReleasedFolder = string.Empty;
        private string pathToDwgList = string.Empty;
        private string pathToStagingFolder = string.Empty;

        public DrawingListManagerForm()
        {
            InitializeComponent();
            pathToReleasedFolder = mySettings.Default.PathToDwgFolder;
            pathToDwgList = mySettings.Default.PathToDwgList;
            pathToStagingFolder = mySettings.Default.PathToStagingFolder;
            dGV1.DefaultCellStyle.BackColor = DefaultBackColor;
            textBox2.Text = pathToReleasedFolder;
            textBox1.Text = pathToDwgList;
            textBox5.Text = pathToStagingFolder;

            //Staging found textbox and consolidate button visibility
            textBox12.Visible = false;
            button6.Visible = false;

            if (pathToReleasedFolder.IsNotNoE() && Directory.Exists(pathToReleasedFolder))
                textBox3.Text =
                    Directory.EnumerateFiles(
                        pathToReleasedFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();

            if (pathToStagingFolder.IsNotNoE() && Directory.Exists(pathToStagingFolder))
                textBox11.Text =
                    Directory.EnumerateFiles(
                        pathToStagingFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();

            //Init dgv
            dGV1.CellFormatting += dGV1_CellFormatting!;
            dGV1.CellToolTipTextNeeded += dGV1_CellToolTipTextNeeded!;
        }

        #region Buttons
        /// <summary>
        /// Select directory with drawings.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (string.IsNullOrEmpty(pathToReleasedFolder)) dialog.InitialDirectory =
                    Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToReleasedFolder;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathToReleasedFolder = dialog.SelectedPath + "\\";
                mySettings.Default.PathToDwgFolder = pathToReleasedFolder;
                textBox2.Text = pathToReleasedFolder;
                if (pathToReleasedFolder.IsNotNoE() && Directory.Exists(pathToReleasedFolder))
                    textBox3.Text =
                        Directory.EnumerateFiles(
                            pathToReleasedFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();
            }
        }

        /// <summary>
        /// Select drawing list Excel file.
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Choose drawing list excel file: ",
                DefaultExt = "xlsx",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                FilterIndex = 0,
            };
            if (string.IsNullOrEmpty(pathToDwgList))
                dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = Path.GetDirectoryName(pathToDwgList);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathToDwgList = dialog.FileName;
                mySettings.Default.PathToDwgList = pathToDwgList;
                textBox1.Text = pathToDwgList;
            }
        }

        /// <summary>
        /// Select staging folder
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (string.IsNullOrEmpty(pathToStagingFolder)) dialog.InitialDirectory =
                    Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToStagingFolder;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pathToStagingFolder = dialog.SelectedPath + "\\";
                mySettings.Default.PathToStagingFolder = pathToStagingFolder;
                textBox5.Text = pathToStagingFolder;

                if (pathToStagingFolder.IsNotNoE() && Directory.Exists(pathToStagingFolder))
                    textBox11.Text =
                        Directory.EnumerateFiles(
                            pathToStagingFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();
            }
        }

        /// <summary>
        /// (Re-) Scan button
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            dGV1.DataSource = null;
            dGV1.Refresh();

            var releasedDrawings =
                FileService.GetDrawingInfosFromDirectory(
                    pathToReleasedFolder, DrawingInfoTypeEnum.Released);
            var stagingDrawings =
                FileService.GetDrawingInfosFromDirectory(
                    pathToStagingFolder, DrawingInfoTypeEnum.Staging);
            var excelDrawings =
                ExcelService.GetDrawingInfosFromExcel(
                    pathToDwgList);

            var analysisResult = AnalysisService.AnalyseDrawings(
                releasedDrawings, stagingDrawings, excelDrawings);

            dGV1.DataSource = analysisResult
                .OrderBy(x => x.DrawingNumber.ToString()).ToList();
        }

        /// <summary>
        /// Consolidate button
        /// </summary>
        private void button6_Click(object sender, EventArgs e)
        {
            //ConsolidateSequence cs = new ConsolidateSequence();
            //cs.ExecuteConsolidate(dlm, pathToDwgFolder, pathToStagingFolder);
            //flipConsolidateButton();
            //button4_Click(null, null);
        }
        #endregion

        #region Events
        private bool subscribedToCellValueChanged = false;
        private void dGV1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!subscribedToCellValueChanged)
            {
                dGV1.CellValueChanged += DGV1_CellValueChanged!;
                subscribedToCellValueChanged = true;
            }
        }
        Color defBackColor;
        private void DGV1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //int col = e.ColumnIndex; int row = e.RowIndex;

            ////The section where we handle changes to "Select" column
            //if (dGV1.Columns[col].HeaderText == dlm.fs.Select.ColumnName)
            //{
            //    bool value = bool.Parse(dGV1.Rows[row].Cells[col].Value.ToString());
            //    if (value)
            //    {
            //        defBackColor = dGV1.Rows[row].DefaultCellStyle.BackColor;
            //        dGV1.Rows[row].DefaultCellStyle.BackColor = Color.DarkGray;
            //    }
            //    else
            //    {
            //        dGV1.Rows[row].DefaultCellStyle.BackColor = defBackColor;
            //    }
            //}
        }
        private void DrawingListManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mySettings.Default.PathToDwgFolder = pathToReleasedFolder;
            mySettings.Default.PathToDwgList = pathToDwgList;
            mySettings.Default.PathToStagingFolder = pathToStagingFolder;
            mySettings.Default.Save();
        }
        private void dGV1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            var dataGridView = (DataGridView)sender;
            var daar = (DrawingAttributeAnalysisResult)
                dataGridView[e.ColumnIndex, e.RowIndex].Value;
            if (daar != null)
                e.CellStyle = daar.CellStyle;  // Set the style of the cell
        }
        private void dGV1_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            var dataGridView = (DataGridView)sender;
            var daar = (DrawingAttributeAnalysisResult)
                dataGridView[e.ColumnIndex, e.RowIndex].Value;
            if (daar != null)
                e.ToolTipText = daar.ToolTip;  // Set the tool tip of the cell
        }
        #endregion

        public void flipConsolidateButton()
        {
            textBox12.Visible = textBox12.Visible ? false : true;
            button6.Visible = button6.Visible ? false : true;
        }
    }
}
