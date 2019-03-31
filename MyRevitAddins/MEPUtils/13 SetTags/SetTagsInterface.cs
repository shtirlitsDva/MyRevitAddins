using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils.SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using MoreLinq;
//using Shared;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using System.Windows.Input;

namespace MEPUtils.SetTags
{
    public partial class SetTagsInterface : System.Windows.Forms.Form
    {
        UIApplication uiApp;
        Document doc;
        UIDocument uidoc;

        Selection selection;
        ICollection<ElementId> selIds;

        string pathToDataFile;

        DataSet dataSet;
        LinkedList<DataRow> linkedListRows = new LinkedList<DataRow>();
        LinkedListNode<DataRow> curNode;

        //Modeless stuff

        public SetTagsInterface(ExternalCommandData commandData)
        {
            InitializeComponent();
            uiApp = commandData.Application;
            doc = commandData.Application.ActiveUIDocument.Document;
            uidoc = uiApp.ActiveUIDocument;

            selection = uidoc.Selection;
            selIds = selection.GetElementIds();

            pathToDataFile = MEPUtils.Properties.Settings.Default.SetTags_pathToDataFile;
            textBox2.Text = pathToDataFile;

        }

        bool ctrl = false;

        /// <summary>
        /// Loads data from Excel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            if ((int)Keyboard.Modifiers == 2) ctrl = true;

            if (ctrl || string.IsNullOrEmpty(pathToDataFile))
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                //CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                if (string.IsNullOrEmpty(pathToDataFile)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
                else dialog.InitialDirectory = pathToDataFile;
                //if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                dialog.ShowDialog();
                //{
                pathToDataFile = dialog.FileName;
                MEPUtils.Properties.Settings.Default.SetTags_pathToDataFile = pathToDataFile;
                textBox2.Text = pathToDataFile;
                //}
            }

            if (File.Exists(pathToDataFile))
            {
                dataSet = ImportExcelToDataSet(pathToDataFile, "Yes");
                List<string> dataTableNames = new List<string>();
                foreach (DataTable item in dataSet.Tables) dataTableNames.Add(item.TableName);
                BasicChooserForm form = new BasicChooserForm(dataTableNames);
                form.ShowDialog();
                string sheetName = form.strTR;
                if (string.IsNullOrEmpty(sheetName)) return;
                DataTable dataTable = ReadDataTable(dataSet.Tables, sheetName);
                foreach (DataRow row in dataTable.Rows) linkedListRows.AddLast(row);

                dataGridView1.ColumnCount = dataTable.Columns.Count;
                int i = 0;
                foreach (DataColumn item in dataTable.Columns)
                {
                    dataGridView1.Columns[i].Name = item.ColumnName;
                    i++;
                }
                curNode = linkedListRows.First;
                dataGridView1.Rows.Add(curNode.Value.ItemArray);
                dataGridView1.Rows.Add();

                i = 0;
                foreach (DataColumn item in dataTable.Columns)
                {
                    dataGridView1.Rows[1].Cells[i].Value = "";
                    i++;
                }
            }
            else
            {
                textBox2.Text = "File does not exist!";
                pathToDataFile = "";
            }

        }

        private void SetTagsInterface_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            MEPUtils.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Previous button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            LinkedListNode<DataRow> prevListNode = curNode.Previous;
            if (prevListNode == null) return;
            curNode = prevListNode;
            dataGridView1.Rows[0].SetValues(curNode.Value.ItemArray);
        }

        /// <summary>
        /// Next button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            LinkedListNode<DataRow> nextListNode = curNode.Next;
            if (nextListNode == null) return;
            curNode = nextListNode;
            dataGridView1.Rows[0].SetValues(curNode.Value.ItemArray);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            selection = uidoc.Selection;
            selIds = selection.GetElementIds();

            if (selIds.Count > 1)
            {
                ErrorMsg("More than one element selected! Please select only one element.");
                return;
            }
            if (selIds.Count < 1)
            {
                ErrorMsg("No element selected! Please select only one element.");
                return;
            }
            ElementId elId = selIds.FirstOrDefault();

            AsyncUpdateParameterValues cmd = new AsyncUpdateParameterValues(elId, dataGridView1);

            AsyncCommandManager.PostCommand(cmd);
        }

        public static void ErrorMsg(string msg)
        {
            Debug.WriteLine(msg);
            WinForms.MessageBox.Show(msg,
              "Error",
              WinForms.MessageBoxButtons.OK,
              WinForms.MessageBoxIcon.Error);
        }

        public static DataSet ImportExcelToDataSet(string fileName, string dataHasHeaders)
        {
            //On connection strings http://www.connectionstrings.com/excel/#p84
            string connectionString =
                string.Format(
                    "provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR={1};IMEX=1\"",
                    fileName, dataHasHeaders);

            DataSet data = new DataSet();

            foreach (string sheetName in GetExcelSheetNames(connectionString))
            {
                using (OleDbConnection con = new OleDbConnection(connectionString))
                {
                    var dataTable = new DataTable();
                    string query = string.Format("SELECT * FROM [{0}]", sheetName);
                    con.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, con);
                    adapter.Fill(dataTable);

                    //Remove ' and $ from sheetName
                    Regex rgx = new Regex("[^a-zA-Z0-9 _-]");
                    string tableName = rgx.Replace(sheetName, "");

                    dataTable.TableName = tableName;
                    data.Tables.Add(dataTable);
                }
            }

            if (data == null) ErrorMsg("Data set is null");
            if (data.Tables.Count < 1) ErrorMsg("Table count in DataSet is 0");

            return data;
        }

        static string[] GetExcelSheetNames(string connectionString)
        {
            //OleDbConnection con = null;
            DataTable dt = null;
            using (OleDbConnection con = new OleDbConnection(connectionString))
            {
                con.Open();
                dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            }

            if (dt == null) return null;

            string[] excelSheetNames = new string[dt.Rows.Count];
            int i = 0;

            foreach (DataRow row in dt.Rows)
            {
                excelSheetNames[i] = row["TABLE_NAME"].ToString();
                i++;
            }

            return excelSheetNames;
        }

        public static DataTable ReadDataTable(DataTableCollection dataTableCollection, string tableName)
        {
            return (from DataTable dtbl in dataTableCollection where dtbl.TableName == tableName select dtbl).FirstOrDefault();
        }

        private class AsyncUpdateParameterValues : IAsyncCommand
        {
            private ElementId SelectedElementId { get; set; }
            private DataGridView Dgw { get; set; }

            private AsyncUpdateParameterValues() { }

            public AsyncUpdateParameterValues(ElementId selectedElementId, DataGridView dgw)
            {
                SelectedElementId = selectedElementId;
                Dgw = dgw;
            }

            public void Execute(Document doc)
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Update parameter values");

                    int i = 0;
                    foreach (DataGridViewColumn column in Dgw.Columns)
                    {
                        //Test to see if there's a name of parameter specified
                        var parNameValue = Dgw.Rows[1].Cells[i].Value;

                        if (parNameValue == null) { i++; continue; }

                        string parName = parNameValue.ToString();

                        if (string.IsNullOrEmpty(parName)) { i++; continue; }

                        Element el = doc.GetElement(SelectedElementId);

                        Parameter parToSet = el.LookupParameter(parName);
                        if (parToSet == null) throw new Exception($"Parameter name {parName} does not exist for element {el.Id.ToString()}!");

                        var parValue = Dgw.Rows[0].Cells[i].Value;

                        if (parValue == null) { i++; continue; }

                        parToSet.Set(parValue.ToString());

                        i++;
                    }
                    tx.Commit();
                }
            }
        }
        ////private void textBox1_TextChanged(object sender, EventArgs e) => DistanceToKeep = textBox1.Text;

        //private void InputBoxBasic_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    ValueToSet = textBox1.Text;
        //}

        //private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter) this.Close();
        //}

        //private void InputBoxBasic_Shown(object sender, EventArgs e)
        //{
        //    textBox1.Text = ValueToSet;
        //    textBox1.SelectionStart = 0;
        //    textBox1.SelectionLength = textBox1.Text.Length;
        //}
    }
}
