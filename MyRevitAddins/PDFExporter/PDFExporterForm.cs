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
using System.Text.RegularExpressions;
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

        private readonly Dictionary<int, Dictionary<int, string>> paperSizeDict;

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


            #region PaperSizeDictionary
            //Init papersize dict
            paperSizeDict = createPaperSizeDictionary();
            #endregion
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

                var revisionType = sheet.GetCurrentRevision();
                string sheetFileName;

                if (revisionType.IntegerValue != -1)
                {
                    Revision revision = (Revision)doc.GetElement(revisionType);
                    int revSequence = revision.SequenceNumber;
                    sheetFileName = sheet.SheetNumber + "-" + IndexToLetter(revSequence) + " - " + sheet.Name + ".pdf";
                }
                else sheetFileName = sheet.SheetNumber + " - " + sheet.Name + ".pdf";

                string fullFileName = pathToExport + sheetFileName;
                pm.PrintToFileName = fullFileName;

                //SetPDFSettings(sheetFileNamePdf, pathToExport);

                PrintSetup pSetup = pm.PrintSetup;
                pSetup.CurrentPrintSetting = pm.PrintSetup.InSession;
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

                //TODO: PaperSize handling
                var filterSheetNumber = fi.ParameterValueFilter(sheet.SheetNumber, BuiltInParameter.SHEET_NUMBER);
                FilteredElementCollector bCol = new FilteredElementCollector(doc);
                var titleBlock = bCol.OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .OfClass(typeof(FamilyInstance))
                    .WherePasses(filterSheetNumber)
                    .Cast<FamilyInstance>()
                    .FirstOrDefault();

                var widthPar = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                var width = Convert.ToInt32(widthPar.AsDouble().FtToMm().Round(0));

                var heightPar = titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                var height = Convert.ToInt32(heightPar.AsDouble().FtToMm().Round(0));

                var paperSizes = pm.PaperSizes;

                var nameOfPaperSize = paperSizeDict[height][width];

                var paperSize = (from PaperSize ps in paperSizes where ps.Name.Equals(nameOfPaperSize) select ps).FirstOrDefault();

                pParams.PaperSize = paperSize;

                pm.Apply();
                pm.SubmitPrint(sheet);
            }
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

        private Dictionary<int, Dictionary<int, string>> createPaperSizeDictionary()
        {
            return new Dictionary<int, Dictionary<int, string>>
            {
                {297, new Dictionary<int, string>
                {
                    { 210, "1x1_(297x210)_MM"},
                    { 420, "1x2_(297x420)_MM"},
                    { 630, "1x3_(297x630)_MM"},
                    { 840, "1x4_(297x840)_MM"},
                    { 1050, "1x5_(297x1050)_MM"},
                    { 1260, "1x6_(297x1260)_MM"},
                    { 1470, "1x7_(297x1470)_MM"},
                    { 1680, "1x8_(297x1680)_MM"},
                    { 1890, "1x9_(297x1890)_MM"},
                    { 2100, "1x10_(297x2100)_MM"}
                }},
                {446, new Dictionary<int, string>
                {
                    { 210, "1,5x1_(446x210)_MM"},
                    { 420, "1,5x2_(446x420)_MM"},
                    { 630, "1,5x3_(446x630)_MM"},
                    { 840, "1,5x4_(446x840)_MM"},
                    { 1050, "1,5x5_(446x1050)_MM"},
                    { 1260, "1,5x6_(446x1260)_MM"},
                    { 1470, "1,5x7_(446x1470)_MM"},
                    { 1680, "1,5x8_(446x1680)_MM"},
                    { 1890, "1,5x9_(446x1890)_MM"},
                    { 2100, "1,5x10_(446x2100)_MM"}
                }},
                {594, new Dictionary<int, string>
                {
                    { 210, "2x1_(594x210)_MM"},
                    { 420, "2x2_(594x420)_MM"},
                    { 630, "2x3_(594x630)_MM"},
                    { 840, "2x4_(594x840)_MM"},
                    { 1050, "2x5_(594x1050)_MM"},
                    { 1260, "2x6_(594x1260)_MM"},
                    { 1470, "2x7_(594x1470)_MM"},
                    { 1680, "2x8_(594x1680)_MM"},
                    { 1890, "2x9_(594x1890)_MM"},
                    { 2100, "2x10_(594x2100)_MM"}
                }},
                {891, new Dictionary<int, string>
                {
                    { 210, "3x1_(893x210)_MM"},
                    { 420, "3x2_(893x420)_MM"},
                    { 630, "3x3_(893x630)_MM"},
                    { 840, "3x4_(893x840)_MM"},
                    { 1050, "3x5_(893x1050)_MM"},
                    { 1260, "3x6_(893x1260)_MM"},
                    { 1470, "3x7_(893x1470)_MM"},
                    { 1680, "3x8_(893x1680)_MM"},
                    { 1890, "3x9_(893x1890)_MM"},
                    { 2100, "3x10_(893x2100)_MM"}
                }}
            };
        }

        static readonly string[] Columns = new[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q",
            "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF",
            "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS",
            "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "BH"
        };
        public static string IndexToLetter(int index)
        {
            if (index <= 0) throw new IndexOutOfRangeException("index must be a positive number");

            return Columns[index - 1];
        }
    }
}

