#region Namespaces
using System;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
//using adWin = Autodesk.Windows;
using PlaceSupport;
using cn = ConnectConnectors.ConnectConnectors;
using tl = TotalLineLength.TotalLineLength;
using piv = PipeInsulationVisibility.PipeInsulationVisibility;
using ped = PED.InitPED;
using mep = MEPUtils.MEPUtilsClass;
using Shared;

#endregion

namespace MyRibbonPanel
{
    [Transaction(TransactionMode.Manual)]
    class App : IExternalApplication
    {
        public const string myRibbonPanelToolTip = "My Own Ribbon Panel";

        //Method to get the button image
        BitmapImage NewBitmapImage(Assembly a, string imageName)
        {
            Stream s = a.GetManifestResourceStream(imageName);

            BitmapImage img = new BitmapImage();

            img.BeginInit();
            img.StreamSource = s;
            img.EndInit();

            return img;
        }

        // get the absolute path of this assembly
        static string ExecutingAssemblyPath = Assembly.GetExecutingAssembly().Location;
        // get ref to assembly
        Assembly exe = Assembly.GetExecutingAssembly();

        public Result OnStartup(UIControlledApplication application)
        {
            AddMenu(application);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private void AddMenu(UIControlledApplication application)
        {
            RibbonPanel rvtRibbonPanel = application.CreateRibbonPanel("MyRevitAddins");

            //ConnectConnectors
            PushButtonData data = new PushButtonData("ConnectConnectors", "Cons", ExecutingAssemblyPath,
                "MyRibbonPanel.ConnectConnectors");
            data.ToolTip = myRibbonPanelToolTip;
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgConnectConnectors16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgConnectConnectors32.png");
            PushButton connectCons = rvtRibbonPanel.AddItem(data) as PushButton;

            //TotalLineLengths
            data = new PushButtonData("TotalLineLengths", "Length", ExecutingAssemblyPath, "MyRibbonPanel.TotalLineLengths");
            data.ToolTip = myRibbonPanelToolTip;
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgTotalLineLength16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgTotalLineLength32.png");
            PushButton totLentgths = rvtRibbonPanel.AddItem(data) as PushButton;

            //PipeInsulationVisibility
            data = new PushButtonData("PipeInsulationVisibility", "Visible", ExecutingAssemblyPath, "MyRibbonPanel.PipeInsulationVisibility");
            data.ToolTip = myRibbonPanelToolTip;
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPipeInsulationVisibility16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPipeInsulationVisibility32.png");
            PushButton pipeInsulationVisibility = rvtRibbonPanel.AddItem(data) as PushButton;

            //PlaceSupports
            data = new PushButtonData("PlaceSupports", "Supports", ExecutingAssemblyPath, "MyRibbonPanel.PlaceSupports");
            data.ToolTip = myRibbonPanelToolTip;
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPlaceSupport16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPlaceSupport32.png");
            PushButton placeSupports = rvtRibbonPanel.AddItem(data) as PushButton;

            //PED
            data = new PushButtonData("PED", "PED", ExecutingAssemblyPath, "MyRibbonPanel.PEDclass");
            data.ToolTip = myRibbonPanelToolTip;
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPED16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPED32.png");
            PushButton PED = rvtRibbonPanel.AddItem(data) as PushButton;

            //MEPUtils
            data = new PushButtonData("MEPUtils", "MEP", ExecutingAssemblyPath, "MyRibbonPanel.MEPUtilsCaller");
            data.ToolTip = myRibbonPanelToolTip;
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgMEPUtils16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgMEPUtils32.png");
            PushButton MEPUtils = rvtRibbonPanel.AddItem(data) as PushButton;

            //PDFExporter
            data = new PushButtonData("PDFExport", "PDF", ExecutingAssemblyPath, "MyRibbonPanel.PDFExporter");
            data.ToolTip = "Exports selected sheet set to PDF.\nRequires BlueBeam";
            data.Image = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPDF16.png");
            data.LargeImage = NewBitmapImage(exe, "MyRibbonPanel.Resources.ImgPDF32.png");
            PushButton PDFExporter = rvtRibbonPanel.AddItem(data) as PushButton;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class ConnectConnectors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document))
                {
                    

                    trans.Start("Connect the Connectors!");
                    cn.ConnectTheConnectors(commandData);
                    trans.Commit();
                }
                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TotalLineLengths : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document))
                {
                    trans.Start("Calculate total length of selected lines!");
                    tl.TotalLineLengths(commandData);
                    trans.Commit();
                }
                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PipeInsulationVisibility : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                using (Transaction trans = new Transaction(commandData.Application.ActiveUIDocument.Document))
                {
                    trans.Start("Toggle Pipe Insulation visibility!");
                    piv.TogglePipeInsulationVisibility(commandData);
                    trans.Commit();
                }
                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PlaceSupports : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result result = PlaceSupport.PlaceSupport.StartPlaceSupportsProcedure(commandData);
            return result;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PEDclass : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            using (TransactionGroup txGp = new TransactionGroup(doc))
            {
                txGp.Start("Initialize PED data");

                using (Transaction trans1 = new Transaction(doc))
                {
                    trans1.Start("Create parameters");
                    ped ped = new ped();
                    ped.CreateElementBindings(commandData);
                    trans1.Commit();
                }

                using (Transaction trans2 = new Transaction(doc))
                {
                    trans2.Start("Populate parameters");
                    ped ped = new ped();
                    ped.PopulateParameters(commandData);
                    trans2.Commit();
                }

                txGp.Assimilate();
            }

            return Result.Succeeded;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class MEPUtilsCaller : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result result = mep.FormCaller(commandData);
            return result;
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class PDFExporter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result result = MEPUtils.PDFExporter.ExportPDF(commandData);
            return result;
        }
    }
}

