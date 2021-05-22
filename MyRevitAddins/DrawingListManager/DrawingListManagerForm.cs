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
using Microsoft.WindowsAPICodePack.Dialogs;
using mySettings = MEPUtils.DrawingListManager.Properties.Settings;
using Color = System.Drawing.Color;

namespace MEPUtils.DrawingListManager
{
    public partial class DrawingListManagerForm : Form
    {
        private string pathToDwgFolder = string.Empty;
        private string pathToDwgList = string.Empty;
        private string pathToStagingFolder = string.Empty;
        private DrwgLstMan dlm = new DrwgLstMan();

        public DrawingListManagerForm()
        {
            InitializeComponent();
            pathToDwgFolder = mySettings.Default.PathToDwgFolder;
            pathToDwgList = mySettings.Default.PathToDwgList;
            pathToStagingFolder = mySettings.Default.PathToStagingFolder;
            dGV1.DefaultCellStyle.BackColor = DefaultBackColor;

            //Staging found textbox and consolidate button visibility
            textBox12.Visible = false;
            button6.Visible = false;

            //Cache reference to form
            dlm.dlmF = this;

            if (pathToDwgFolder.IsNotNoE() && Directory.Exists(pathToDwgFolder))
                textBox3.Text =
                    Directory.EnumerateFiles(
                        pathToDwgFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();

            if (pathToStagingFolder.IsNotNoE() && Directory.Exists(pathToStagingFolder))
                textBox11.Text =
                    Directory.EnumerateFiles(
                        pathToStagingFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();
        }

        /// <summary>
        /// Select directory with drawings.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //https://stackoverflow.com/a/41511598/6073998
            if (string.IsNullOrEmpty(pathToDwgFolder)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToDwgFolder;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathToDwgFolder = dialog.FileName + "\\";
                mySettings.Default.PathToDwgFolder = pathToDwgFolder;
                textBox2.Text = pathToDwgFolder;
                if (pathToDwgFolder.IsNotNoE() && Directory.Exists(pathToDwgFolder))
                    textBox3.Text =
                        Directory.EnumerateFiles(
                            pathToDwgFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();
            }
        }

        /// <summary>
        /// Select drawing list Excel file.
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //https://stackoverflow.com/a/41511598/6073998
            if (string.IsNullOrEmpty(pathToDwgList)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = Path.GetDirectoryName(pathToDwgList);
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathToDwgList = dialog.FileName;
                mySettings.Default.PathToDwgList = pathToDwgList;
                textBox1.Text = pathToDwgList;
            }
        }

        private void DrawingListManagerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mySettings.Default.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MainSequence ms = new MainSequence();
            dGV1.DataSource = null;
            dGV1.Refresh();
            dlm.ResetDgv(); //Clear dataTables so they won't show double
            ms.ExecuteMainSequence(dlm, dGV1, pathToDwgFolder, pathToDwgList);

            //this.Close();
            //TODO: implement case where drwg only exists in excel
        }

        private bool subscribedToCellValueChanged = false;
        private void dGV1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!subscribedToCellValueChanged)
            {
                dGV1.CellValueChanged += DGV1_CellValueChanged;
                subscribedToCellValueChanged = true;
            }
        }

        Color defBackColor;
        private void DGV1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int col = e.ColumnIndex; int row = e.RowIndex;

            //The section where we handle changes to "Select" column
            if (dGV1.Columns[col].HeaderText == dlm.fs.Select.ColumnName)
            {
                bool value = bool.Parse(dGV1.Rows[row].Cells[col].Value.ToString());
                if (value)
                {
                    defBackColor = dGV1.Rows[row].DefaultCellStyle.BackColor;
                    dGV1.Rows[row].DefaultCellStyle.BackColor = Color.DarkGray;
                }
                else
                {
                    dGV1.Rows[row].DefaultCellStyle.BackColor = defBackColor;
                }
            }
        }
        /// <summary>
        /// Select staging folder
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //https://stackoverflow.com/a/41511598/6073998
            if (string.IsNullOrEmpty(pathToStagingFolder)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToStagingFolder;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathToStagingFolder = dialog.FileName + "\\";
                mySettings.Default.PathToStagingFolder = pathToStagingFolder;
                textBox5.Text = pathToStagingFolder;

                if (pathToStagingFolder.IsNotNoE() && Directory.Exists(pathToStagingFolder))
                    textBox11.Text =
                        Directory.EnumerateFiles(
                            pathToStagingFolder, "*.pdf", SearchOption.TopDirectoryOnly).Count().ToString();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ConsolidateSequence cs = new ConsolidateSequence();
            cs.ExecuteConsolidate(dlm, pathToDwgFolder, pathToStagingFolder);
            flipConsolidateButton();
        }
        public void flipConsolidateButton()
        {
            textBox12.Visible = textBox12.Visible ? false : true;
            button6.Visible = button6.Visible ? false : true;
        }
    }
}
