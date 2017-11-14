using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
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
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathToExport = dialog.FileName + "\\";
                mySettings.Default.selectedFolderToExportTo = pathToExport;
                textBox2.Text = pathToExport;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            FilteredElementCollector col = new FilteredElementCollector(doc);

            var sheetSet = col.OfClass(typeof(ViewSheetSet)).Where(x => x.Name == selectedSheetSet).Cast<ViewSheetSet>().FirstOrDefault();

            foreach (ViewSheet sheet in sheetSet.Views)
            {
                PrintManager pm = doc.PrintManager;

                pm.PrintRange = PrintRange.Select;
                //pm.SelectNewPrintDriver("Adobe PDF");
                //pm.SelectNewPrintDriver("HP PS Printer");
                pm.SelectNewPrintDriver("Bluebeam PDF");
                pm.PrintToFile = true;

                //string sheetFileNamePs = sheet.SheetNumber + " - " + sheet.Name + ".ps";
                string sheetFileNamePdf = sheet.SheetNumber + " - " + sheet.Name + ".pdf";
                //string fullFileNamePs = pathToExport + sheetFileNamePs;
                string fullFileNamePdf = pathToExport + sheetFileNamePdf;
                pm.PrintToFileName = fullFileNamePdf;

                SetPDFSettings(sheetFileNamePdf, pathToExport);

                PrintSetup pSetup = pm.PrintSetup;
                PrintParameters pParams = pSetup.CurrentPrintSetting.PrintParameters;

                pParams.ZoomType = ZoomType.Zoom;
                pParams.Zoom = 100;
                //TODO: pParams.PageOrientation = PageOrientationType.Landscape???
                pParams.PaperPlacement = PaperPlacementType.Center;
                pParams.ColorDepth = ColorDepthType.Color;
                pParams.RasterQuality = RasterQualityType.Presentation;
                pParams.HiddenLineViews = HiddenLineViewsType.VectorProcessing;
                pParams.ViewLinksinBlue = false;
                pParams.HideReforWorkPlanes = true;
                pParams.HideUnreferencedViewTags = true;
                pParams.HideCropBoundaries = true;
                pParams.HideScopeBoxes = true;
                pParams.ReplaceHalftoneWithThinLines = false;
                pParams.MaskCoincidentLines = false;

                //TODO: PaperSize handling.

                pm.Apply();

                //pSetup.SaveAs("Temporary export");
                
                pm.SubmitPrint(sheet);
            }
        }

        private string CreatePdf(string outputPath, string fileName)
        {
            string ret;

            try
            {
                string command = $"gswin32c -q -dNOPAUSE -sDEVICE=pdfwrite -sOutputFile=\"{outputPath}\" -fc:\\\"{fileName}\"";

                Process pdfProcess = new Process();

                StreamWriter writer;
                StreamReader reader;

                ProcessStartInfo info = new ProcessStartInfo("cmd");
                info.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardInput = true;
                info.RedirectStandardOutput = true;

                pdfProcess.StartInfo = info;
                pdfProcess.Start();

                writer = pdfProcess.StandardInput;
                reader = pdfProcess.StandardOutput;
                writer.AutoFlush = true;

                writer.WriteLine(command);

                writer.Close();

                ret = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ret;
        }

        /// <summary>
        /// Set's next file print location/name to prevent PDF prompting for file name.
        /// </summary>
        /// <param name="destFileName">Full file name with extension.</param>
        /// <param name="dirName">Directory path.</param>
        private void SetPDFSettings(string destFileName, string dirName)
        {
            try
            {
                // if (PrinterName != "Adobe PDF") return;
                var pjcKey = Registry.CurrentUser.OpenSubKey(@"Software\Adobe\Acrobat Distiller\PrinterJobControl", true);
                var appPath = @"E:\programs\IDSP18\Revit 2018\Revit.exe";
                pjcKey?.SetValue(appPath, destFileName);
                pjcKey?.SetValue("LastPdfPortFolder - Revit.exe", dirName);
            }
            catch (Exception)
            {
                Autodesk.Revit.UI.TaskDialog.Show("ERROR", "Couldn't access PDF driver registry settings");
            }
        }
    }
}

