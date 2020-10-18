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
        private DrwgLstMan dlm = new DrwgLstMan();

        public DrawingListManagerForm()
        {
            InitializeComponent();
            pathToDwgFolder = mySettings.Default.PathToDwgFolder;
            pathToDwgList = mySettings.Default.PathToDwgList;
            dGV1.DefaultCellStyle.BackColor = DefaultBackColor;
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

        /// <summary>
        /// Enumerate pdf files in the selected folder
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            dlm.EnumeratePdfFiles(pathToDwgFolder);
            textBox3.Text = dlm.drwgFileNameList.Count.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Load file name data
            dlm.ScanRescanFilesAndList(pathToDwgFolder);
            dlm.PopulateDrwgDataFromFileName();
            dlm.BuildFileNameDataTable();
            dlm.PopulateFileNameDataTable();
            dGV1.DataSource = dlm.FileNameData;

            foreach (DataGridViewColumn dc in dGV1.Columns)
            {
                dc.DefaultCellStyle.Font = new Font("Arial", 26F, GraphicsUnit.Pixel);
                dc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            #region LoadExcelData-GC-Populate
            //Load excel data
            dlm.ScanExcelFile(pathToDwgList);
            //Dispose of Excel objects
            //https://stackoverflow.com/questions/25134024/clean-up-excel-interop-objects-with-idisposable/25135685#25135685
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //Populate drwg data by data from Excel
            dlm.PopulateDrwgDataFromExcel();
            #endregion

            #region LoadMetaData-GC-Populate
            //Load data from metadata
            dlm.ReadMetadataData(pathToDwgFolder);
            //Run GC just to be sure that the pdf pointers are gone
            GC.Collect();
            GC.WaitForPendingFinalizers();
            dlm.PopulateDrwgDataFromMetadata();
            #endregion

            //Analyze data
            foreach (Drwg drwg in dlm.drwgList) drwg.CalculateState();

            #region Debug
            //StringBuilder sb = new StringBuilder();
            //List<string> list = new List<string>();
            ////Group all states and list them
            //var query = dlm.drwgList.GroupBy(x => x.State);
            //foreach (var gr in query) list.Add($"{(int)gr.Key} - {gr.Key}");
            //list.Sort();
            //sb.Append(string.Join("\n", list));

            //Just for fun list all states
            //for (int val = 0; val <= 16385; val++)
            //    sb.AppendLine($"{val} - {(Drwg.StateFlags)val}");
            //Output.OutputWriter(sb); 
            #endregion

            foreach (Drwg drwg in dlm.drwgList) dlm.AddStateToGridView(drwg, dlm.FileNameData);
            dlm.FileNameData.AcceptChanges();
            dGV1.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            foreach (Drwg drwg in dlm.drwgList)
            {
                dlm.
            }

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
            if (dGV1.Columns[col].HeaderText == dlm.fs._Select.ColumnName)
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
    }
}
