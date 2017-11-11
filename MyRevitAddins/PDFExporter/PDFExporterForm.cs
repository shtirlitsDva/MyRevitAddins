using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using mySettings = MGTek.PDFExporter.Properties.Settings;
using Microsoft.WindowsAPICodePack.Dialogs;
using Shared;
using fi = Shared.Filter;

namespace MGTek.PDFExporter
{
    public partial class PDFExporterForm : System.Windows.Forms.Form
    {
        private IList<string> sheetSetNames;
        private string selectedSheetSet;
        private string pathToExport;

        private static ExternalCommandData commandData;
        private static UIApplication uiapp;
        private static UIDocument uidoc;
        private static Document doc;
        private string _message;

        public PDFExporterForm(ExternalCommandData commandData, ref String message, ElementSet elements)
        {
            InitializeComponent();

            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            _message = message;

            //Init combobox (sheet set name)
            sheetSetNames = getSheetSetNames(doc);
            int index = sheetSetNames.IndexOf(mySettings.Default.selectedSheetSet);
            comboBox1.DataSource = sheetSetNames;
            if (index > -1) comboBox1.SelectedIndex = index;

            //Init export folder path
            pathToExport = mySettings.Default.selectedFolderToExportTo;
            textBox2.Text = pathToExport;
        }

        private IList<string> getSheetSetNames(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewSheetSet));
            return collector.Select(x => x.Name).ToList();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSheetSet = (string)comboBox1.SelectedItem;
            mySettings.Default.selectedSheetSet = selectedSheetSet;
        }

        private void PDFExporterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mySettings.Default.Save();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //https://stackoverflow.com/a/41511598/6073998
            if (string.IsNullOrEmpty(pathToExport)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToExport;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog()==CommonFileDialogResult.Ok)
            {
                pathToExport = dialog.FileName;
                mySettings.Default.selectedFolderToExportTo = pathToExport;
                textBox2.Text = pathToExport;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Export PDF");
                FilteredElementCollector col = new FilteredElementCollector(doc);

                var sheetSet = col.OfClass(typeof(ViewSheetSet)).Where(x => x.Name == selectedSheetSet).Cast<ViewSheetSet>().FirstOrDefault();

                foreach (ViewSheet sheet in sheetSet.Views)
                {
                    sheet.
                }

                tx.Commit();
            }

        }
    }
}
