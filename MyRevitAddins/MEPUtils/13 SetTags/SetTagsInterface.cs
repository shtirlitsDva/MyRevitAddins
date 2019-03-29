using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils._00_SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Input;
using dbg = Shared.Dbg;
using fi = Shared.Filter;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using mp = Shared.MepUtils;
using tr = Shared.Transformation;

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
        LinkedList<DataRow> linkedListRows = null;
        LinkedListNode<DataRow> curNode;

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

        private void button1_Click(object sender, EventArgs e)
        {
            
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

            if (ctrl || pathToDataFile.IsNullOrEmpty())
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                if (pathToDataFile.IsNullOrEmpty()) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
                else dialog.InitialDirectory = pathToDataFile;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    pathToDataFile = dialog.FileName;
                    MEPUtils.Properties.Settings.Default.SetTags_pathToDataFile = pathToDataFile;
                    textBox2.Text = pathToDataFile;
                }
            }

            if (File.Exists(pathToDataFile))
            {
                dataSet = DataHandler.ImportExcelToDataSet(pathToDataFile, "Yes");
                List<string> dataTableNames = new List<string>();
                foreach (DataTable item in dataSet.Tables) dataTableNames.Add(item.TableName);
                BaseFormTableLayoutPanel_Basic form = new BaseFormTableLayoutPanel_Basic(dataTableNames);
                DataTable dataTable = DataHandler.ReadDataTable(dataSet.Tables, form.strTR);
                foreach (DataRow row in dataTable.Rows) linkedListRows.AddLast(row);

                dataGridView1.ColumnCount = dataTable.Columns.Count;
                int i = 0;
                foreach (DataColumn item in dataTable.Columns)
                {
                    dataGridView1.Columns[i].Name = item.ColumnName;
                    curNode = linkedListRows.First;
                    dataGridView1.Rows.Add(curNode.Value.ItemArray);
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
