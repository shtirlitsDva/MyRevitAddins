﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
//using MoreLinq;
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
using mySettings = PDFExporter.Properties.Settings;
using Microsoft.WindowsAPICodePack.Dialogs;
using Shared;
using fi = Shared.Filter;
using PdfSharp.Pdf;

namespace PDFExporter
{
    public partial class PDFExporterForm : System.Windows.Forms.Form
    {
        private List<string> sheetSetNames;
        List<FileNames> fileNames = new List<FileNames>();
        private string selectedSheetSet;
        private string pathToExport;

        private readonly Dictionary<int, Dictionary<int, string>> paperSizeDict;

        private static UIApplication uiapp;
        private static UIDocument uidoc;
        private static Document doc;

        public PDFExporterForm(ExternalCommandData commandData)
        {
            InitializeComponent();

            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            //Init combobox (sheet set name)
            sheetSetNames = getSheetSetNames(doc);
            int index = sheetSetNames.IndexOf(mySettings.Default.selectedSheetSet);
            comboBox1.DataSource = sheetSetNames;
            if (index > -1) comboBox1.SelectedIndex = index;

            //Init pages n of nr box
            if (selectedSheetSet == "Current sheet only")
            {
                textBox4.Text = "1";
            }
            else
            {
                FilteredElementCollector col = new FilteredElementCollector(doc);
                var sheetSet = col.OfClass(typeof(ViewSheetSet))
                                  .Where(x => x.Name == selectedSheetSet)
                                  .Cast<ViewSheetSet>().FirstOrDefault();
                textBox4.Text = sheetSet.Views.Size.ToString();
            }


            //Init export folder path
            pathToExport = mySettings.Default.selectedFolderToExportTo;
            textBox2.Text = pathToExport;

            //Init Settings
            //Init Colour
            radioButton1.Checked = mySettings.Default.Setting_Colour_Colour;
            radioButton2.Checked = mySettings.Default.Setting_Colour_GrayScale;
            radioButton3.Checked = mySettings.Default.Setting_Colour_BlackWhite;


            #region PaperSizeDictionary
            //Init papersize dict
            paperSizeDict = createPaperSizeDictionary();
            #endregion
        }

        private List<string> getSheetSetNames(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ViewSheetSet));
            List<string> sheetSetNames = new List<string> { "Current sheet only" };
            foreach (var view in collector) sheetSetNames.Add(view.Name);
            return sheetSetNames;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedSheetSet = (string)comboBox1.SelectedItem;
            mySettings.Default.selectedSheetSet = selectedSheetSet;

            //Update nr of nr pages
            if (selectedSheetSet == "Current sheet only")
            {
                textBox4.Text = "1";
            }
            else
            {
                FilteredElementCollector col = new FilteredElementCollector(doc);
                var sheetSet = col.OfClass(typeof(ViewSheetSet))
                                  .Where(x => x.Name == selectedSheetSet)
                                  .Cast<ViewSheetSet>().FirstOrDefault();
                textBox4.Text = sheetSet.Views.Size.ToString();
            }
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

        /// <summary>
        /// Retrieves print setting created for the specific paper size and prints all sheets in the set.
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            //string sheetFileName;
            //string fullSheetFileName;
            //IList<string> fileNamesSource = new List<string>();
            //IList<string> fileNamesDefault = new List<string>();

            //Clear the fileNamesDestination
            fileNames.Clear();

            using (Transaction trans = new Transaction(doc))
            {
                //Update status window
                textBox8.Text = "Preparing print";

                trans.Start("Print!");

                List<ViewSheet> viewSheetList = new List<ViewSheet>();

                if (selectedSheetSet == "Current sheet only")
                {
                    Autodesk.Revit.DB.View view = doc.ActiveView;
                    viewSheetList.Add((ViewSheet)view);
                }
                else
                {
                    FilteredElementCollector col = new FilteredElementCollector(doc);
                    var sheetSet = col.OfClass(typeof(ViewSheetSet))
                                      .Where(x => x.Name == selectedSheetSet)
                                      .Cast<ViewSheetSet>().FirstOrDefault();
                    foreach (ViewSheet vs in sheetSet.Views) viewSheetList.Add(vs);
                }

                #region Old
                //FilteredElementCollector colPs = new FilteredElementCollector(doc);
                //var printSettings = colPs.OfClass(typeof(PrintSetting)).Cast<PrintSetting>().ToHashSet();

                //PrintManager pm = doc.PrintManager;
                //PrintSetup ps = pm.PrintSetup;
                //pm.SelectNewPrintDriver("Bluebeam PDF");
                //pm = doc.PrintManager;
                //ps = pm.PrintSetup;
                //var paperSizes = pm.PaperSizes; 
                #endregion

                string title = doc.Title; //<- THIS CAN CAUSE PROBLEMS RECOGNISING THE ORIGINAL FILE NAME

                int sheetCount = 0;

                foreach (ViewSheet sheet in viewSheetList)
                {
                    #region Naming
                    //var revisionType = sheet.GetCurrentRevision();

                    FileNames fileName = new FileNames();
                    fileNames.Add(fileName);

                    Parameter sheetNumberPar = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                    string sheetNumber = sheetNumberPar.AsString();
                    fileName.SheetNumber = sheetNumber;

                    Parameter sheetNamePar = sheet.get_Parameter(BuiltInParameter.SHEET_NAME);
                    string sheetName = sheetNamePar.AsString();
                    fileName.SheetName = sheetName;

                    Parameter curRevisionPar = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION);
                    string revision = curRevisionPar.AsString();
                    fileName.Revision = revision;

                    Parameter curRevisionDatePar = sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DATE);
                    string revisionDate = curRevisionDatePar.AsString();
                    fileName.RevisionDate = revisionDate;

                    Parameter sheetIssueDatePar = sheet.get_Parameter(BuiltInParameter.SHEET_ISSUE_DATE);
                    string sheetIssueDate = sheetIssueDatePar.AsString();
                    fileName.Date = sheetIssueDate;

                    //Collect and read correct scale
                    //We need to collect the title block instance to determine if manual scale is on
                    //If it is on, read manual scale, if not read ordinary scale
                    FilteredElementCollector col = new FilteredElementCollector(doc);
                    ElementParameterFilter epf = fi.ParameterValueGenericFilter(doc, sheet.SheetNumber, BuiltInParameter.SHEET_NUMBER);
                    var tb = col.OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilyInstance)).WherePasses(epf).FirstOrDefault();

                    Parameter curScalePar = sheet.LookupParameter("Scale");
                    if (curScalePar != null)
                    {
                        //Check to see if manual scale is on
                        if (tb != null && tb.LookupParameter("Manual Skala Tændt").AsInteger() == 1)
                        {
                            Parameter curScaleManualPar = sheet.LookupParameter("Manual skala");
                            if (curScaleManualPar != null) fileName.Scale = curScaleManualPar.AsString().Replace(" ", "");
                            else fileName.Scale = curScalePar.AsString().Replace(" ", "");
                        }
                        else fileName.Scale = curScalePar.AsString().Replace(" ", "");
                    }

                    //Look at this later
                    //Parameter sorting1Par = sheet.LookupParameter("RDK-COM-SHT-SORTING_1");
                    //string sorting1 = sorting1Par.AsString();
                    //Parameter sorting2Par = sheet.LookupParameter("RDK-COM-SHT-SORTING_2");
                    //string sorting2 = sorting2Par.AsString();
                    //fileName.DrawingListCategory = sorting1 + " - " + sorting2;

                    fileName.GenerateFileName();

                    fileName.FileNameWithPath = pathToExport + fileName.FileName; //Used to copy files later

                    fileName.DwfFileName = pathToExport + "DWF\\" + 
                        (fileName.FileName.Remove(fileName.FileName.Length - 3)) + "dwf";

                    string printfilename = Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments) + "\\" + fileName.FileName; //Used to satisfy bluebeam
                    //fileNamesSource.Add(printfilename);

                    //string defaultFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    //    + "\\" + title + " - Sheet - " + sheet.SheetNumber + " - " + sheet.Name + ".pdf";
                    //fileNamesDefault.Add(defaultFileName);

                    //pm.PrintToFileName = printfilename;
                    #endregion

                    //FamilyInstance titleBlock =
                    //    fi.GetElements<FamilyInstance, BuiltInParameter>(
                    //        doc, BuiltInParameter.SHEET_NUMBER, sheet.SheetNumber).FirstOrDefault();

                    //var widthPar = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                    //var width = Convert.ToInt32(widthPar.AsDouble().FtToMm().Round(0));

                    //var heightPar = titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                    //var height = Convert.ToInt32(heightPar.AsDouble().FtToMm().Round(0));

                    //var nameOfPaperSize = paperSizeDict[height][width];

                    PDFExportOptions options = new PDFExportOptions();
                    options.PaperFormat = ExportPaperFormat.Default;
                    options.ZoomType = ZoomType.Zoom;
                    options.ZoomPercentage = 100;
                    options.PaperPlacement = PaperPlacementType.Center;
                    options.PaperOrientation = PageOrientationType.Landscape;
                    options.Combine = true;
                    options.FileName = fileName.FileName;

                    doc.Export(pathToExport, new List<ElementId>() { sheet.Id }, options);

                    //var printSetting = (from PrintSetting pSetting in printSettings
                    //                    where pSetting.Name == nameOfPaperSize
                    //                    select pSetting).FirstOrDefault();

                    //ps.CurrentPrintSetting = printSetting ?? throw new Exception($"{sheet.Name} does not have a print setting!");

                    //pm.Apply();

                    //Feedback
                    sheetCount++;
                    textBox8.Text = "Sending " + sheetCount;

                    //pm.SubmitPrint(sheet);

                    //Also export to DWF
                    DWFExportOptions dwfOptions = new DWFExportOptions();
                    string dwfExportPath = pathToExport + "\\DWF";
                    System.IO.Directory.CreateDirectory(dwfExportPath);

                    ViewSet vs = new ViewSet();
                    vs.Insert(sheet);

                    if (File.Exists(fileName.DwfFileName)) File.Delete(fileName.DwfFileName);

                    doc.Export(dwfExportPath, fileName.FileName.Remove(fileName.FileName.Length - 4), vs, dwfOptions);

                    System.Threading.Thread.Sleep(3000);
                }



                trans.Commit();
            }
            ////File handling
            //if (WaitForFile(fileNamesSource.Last(), fileNamesDefault.Last()))
            //{
            //    var result = tryOpenFiles(fileNamesSource.Last(), fileNamesDefault.Last());

            //    if (result.Item1)
            //    {
            //        foreach (var files in fileNamesSource.Zip(fileNamesDestination, Tuple.Create))
            //        {
            //            if (File.Exists(files.Item2)) File.Delete(files.Item2);
            //            File.Move(files.Item1, files.Item2);
            //        }
            //    }
            //    else if (result.Item2)
            //    {
            //        foreach (var files in fileNamesDefault.Zip(fileNamesDestination, Tuple.Create))
            //        {
            //            if (File.Exists(files.Item2)) File.Delete(files.Item2);
            //            File.Move(files.Item1, files.Item2);
            //        }
            //    }
            //    else throw new Exception("Filename handling FAILED AGAIN!!!!!!!");

            //}
            //else Shared.BuildingCoder.BuildingCoderUtilities.ErrorMsg("The copying of files failed for some reason!");
        }

        /// <summary>
        /// Blocks until the file is not locked any more.
        /// </summary>
        /// <param name="ideal"></param>
        private static bool WaitForFile(string first, string second)
        {
            var numTries = 0;

            while (true)
            {
                ++numTries;
                if (File.Exists(first) || File.Exists(second))
                {
                    break;
                }

                if (numTries > 50) return false; //TODO: Set it to 1000
                // Wait for the lock to be released
                System.Threading.Thread.Sleep(300);
            }

            numTries = 0;

            while (true)
            {
                ++numTries;
                var result = tryOpenFiles(first, second);

                if (result.Item1 || result.Item2) return true;

                if (numTries > 1000) return false;
                // Wait for the lock to be released
                System.Threading.Thread.Sleep(300);

            }
        }

        private static (bool, bool) tryOpenFiles(string first, string second)
        {
            bool firstOK = false;
            bool secondOK = false;

            try
            {
                using (var fs = new FileStream(first, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100))
                {
                    fs.ReadByte();
                    // If we got this far the file is ready
                    firstOK = true;
                }
            }
            catch (Exception) { }

            try
            {
                using (var fs = new FileStream(second, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 100))
                {
                    fs.ReadByte();
                    // If we got this far the file is ready
                    secondOK = true;
                }
            }
            catch (Exception) { }

            return (firstOK, secondOK);
        }

        /// <summary>
        /// Creates print settings usable for the paper sizes of selected print set.
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            List<ViewSheet> viewSheetList = new List<ViewSheet>();

            if (selectedSheetSet == "Current sheet only")
            {
                Autodesk.Revit.DB.View view = doc.ActiveView;
                viewSheetList.Add((ViewSheet)view);
            }
            else
            {
                FilteredElementCollector col = new FilteredElementCollector(doc);
                var sheetSet = col.OfClass(typeof(ViewSheetSet))
                                  .Where(x => x.Name == selectedSheetSet)
                                  .Cast<ViewSheetSet>().FirstOrDefault();
                foreach (ViewSheet vs in sheetSet.Views) viewSheetList.Add(vs);
            }

            PrintManager pm = doc.PrintManager;
            PrintSetup ps = pm.PrintSetup;
            pm.SelectNewPrintDriver("Bluebeam PDF");
            pm = doc.PrintManager;
            ps = pm.PrintSetup;
            var paperSizes = pm.PaperSizes;

            FilteredElementCollector colPs = new FilteredElementCollector(doc);
            var printSettings = colPs.OfClass(typeof(PrintSetting)).Cast<PrintSetting>().ToHashSet();

            using (Transaction trans = new Transaction(doc))
            {
                trans.Start("PrintSettings");
                //PrintManager pm = doc.PrintManager;

                foreach (ViewSheet sheet in viewSheetList)
                {
                    FamilyInstance titleBlock =
                            fi.GetElements<FamilyInstance, BuiltInParameter>(doc, BuiltInParameter.SHEET_NUMBER, sheet.SheetNumber).FirstOrDefault();

                    var widthPar = titleBlock.get_Parameter(BuiltInParameter.SHEET_WIDTH);
                    var width = Convert.ToInt32(widthPar.AsDouble().FtToMm().Round(0));

                    var heightPar = titleBlock.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                    var height = Convert.ToInt32(heightPar.AsDouble().FtToMm().Round(0));

                    var nameOfPaperSize = paperSizeDict[height][width];

                    if (printSettings.Any(x => x.Name == nameOfPaperSize))
                    {
                        PrintSetting pset = printSettings.Where(x => x.Name == nameOfPaperSize).FirstOrDefault();
                        printSettings = printSettings.ExceptWhere(x => x.Name == nameOfPaperSize).ToHashSet();
                        doc.Delete(pset.Id);
                        doc.Regenerate();
                    }

                    pm.PrintRange = PrintRange.Select;
                    pm.PrintToFile = true;

                    PrintSetup pSetup = pm.PrintSetup;
                    pSetup.CurrentPrintSetting = pm.PrintSetup.InSession;
                    PrintParameters pParams = pSetup.CurrentPrintSetting.PrintParameters;

                    pParams.ZoomType = ZoomType.Zoom;
                    pParams.Zoom = 100;
                    pParams.PageOrientation = PageOrientationType.Landscape;
                    pParams.PaperPlacement = PaperPlacementType.Center;
                    if (radioButton1.Checked) pParams.ColorDepth = ColorDepthType.Color;
                    else if (radioButton2.Checked) pParams.ColorDepth = ColorDepthType.GrayScale;
                    else if (radioButton3.Checked) pParams.ColorDepth = ColorDepthType.BlackLine;
                    pParams.RasterQuality = RasterQualityType.Presentation;
                    pParams.HiddenLineViews = HiddenLineViewsType.VectorProcessing;
                    pParams.ViewLinksinBlue = false;
                    pParams.HideReforWorkPlanes = true;
                    pParams.HideUnreferencedViewTags = true;
                    pParams.HideCropBoundaries = true;
                    pParams.HideScopeBoxes = true;
                    pParams.ReplaceHalftoneWithThinLines = false;
                    pParams.MaskCoincidentLines = false;

                    var paperSize = (from PaperSize psize in paperSizes where psize.Name.Equals(nameOfPaperSize) select psize).FirstOrDefault();

                    pParams.PaperSize = paperSize ?? 
                        throw new Exception($"Printer does not have specified paper size! Check paper size configuration.\n" +
                                            $"{nameOfPaperSize}");

                    pm.Apply();

                    try
                    {
                        pm.PrintSetup.SaveAs(paperSize.Name);
                    }
                    catch (Exception)
                    {

                    }



                }

                trans.Commit();
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
                    { 210, "3x1_(891x210)_MM"},
                    { 420, "3x2_(891x420)_MM"},
                    { 630, "3x3_(891x630)_MM"},
                    { 840, "3x4_(891x840)_MM"},
                    { 1050, "3x5_(891x1050)_MM"},
                    { 1260, "3x6_(891x1260)_MM"},
                    { 1470, "3x7_(891x1470)_MM"},
                    { 1680, "3x8_(891x1680)_MM"},
                    { 1890, "3x9_(891x1890)_MM"},
                    { 2100, "3x10_(891x2100)_MM"}
                }},
                {2339, new Dictionary<int, string>
                {
                    { 1654, "ISO_A2_(420.00_x_594.00_MM)"}
                }},
                {3311, new Dictionary<int, string>
                {
                    { 2339, "ISO_A1_(594.00_x_841.00_MM)"}
                }},
                {4680, new Dictionary<int, string>
                {
                    { 3311, "ISO_A0_(841.00_x_1189.00_MM)"}
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

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) { mySettings.Default.Setting_Colour_Colour = true; }
            else mySettings.Default.Setting_Colour_Colour = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked) { mySettings.Default.Setting_Colour_GrayScale = true; }
            else mySettings.Default.Setting_Colour_GrayScale = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked) { mySettings.Default.Setting_Colour_BlackWhite = true; }
            else mySettings.Default.Setting_Colour_BlackWhite = false;
        }

        /// <summary>
        /// Copy files
        /// </summary>
        private void Button4_Click(object sender, EventArgs e)
        {
            foreach (FileNames fileName in fileNames)
            {
                string[] found = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"*{fileName.SheetNumber.Replace(".", "-")}*");
                if (found.ToList().Count < 1) continue;
                if (File.Exists(fileName.FileNameWithPath)) File.Delete(fileName.FileNameWithPath);

                string inputF = found[0];
                string outputF = fileName.FileNameWithPath;

                PdfDocument pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open(inputF);
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGNUMBER", new PdfString(fileName.SheetNumber)));
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGTITLE", new PdfString(fileName.SheetName)));
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGSCALE", new PdfString(fileName.Scale)));
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGDATE", new PdfString(fileName.Date)));
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGREVINDEX", new PdfString(fileName.Revision)));
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGREVDATE", new PdfString(fileName.RevisionDate)));
                pdfDoc.Info.Elements.Add(new KeyValuePair<string, PdfItem>("/DWGLSTCAT", new PdfString(fileName.DrawingListCategory)));

                pdfDoc.Save(outputF);
                pdfDoc.Close();

                File.Delete(inputF);
                //File.Move(found[0], fileName.FileNameWithPath);
            }
        }
    }

    internal class FileNames
    {
        public string SheetNumber { get; internal set; }
        public string SheetName { get; internal set; }
        public string Revision { get; internal set; } = string.Empty;
        public string Scale { get; internal set; } = "---";
        public string Date { get; internal set; }
        public string RevisionDate { get; internal set; } = string.Empty;
        public string FileName { get; internal set; }
        public string FileNameWithPath { get; internal set; }
        public string DrawingListCategory { get; internal set; }
        public string DwfFileName { get; internal set; }
        internal void GenerateFileName()
        {
            if (!Revision.IsNullOrEmpty())
            {
                FileName = SheetNumber + "-" + Revision + " - " + SheetName + ".pdf";
            }
            else FileName = SheetNumber + " - " + SheetName + ".pdf";
        }
    }
}


