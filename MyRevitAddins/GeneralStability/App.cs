#region Namespaces
using System;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace Revit_PCF_Importer
{
    [Transaction(TransactionMode.Manual)]
    class App : IExternalApplication
    {
        public const string pcfImporterButtonToolTip = "Import PCF piping data into Revit";

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
            RibbonPanel rvtRibbonPanel = application.CreateRibbonPanel("PCF Tools");
            PushButtonData data = new PushButtonData("PCFImporter", "Revit PCF Importer", ExecutingAssemblyPath, "Revit_PCF_Importer.FormCaller");
            data.ToolTip = pcfImporterButtonToolTip;
            //data.Image = NewBitmapImage(exe, "PCF_Functions.ImgPcfExport16.png");
            //data.LargeImage = NewBitmapImage(exe, "PCF_Functions.ImgPcfExport32.png");
            PushButton pushButton = rvtRibbonPanel.AddItem(data) as PushButton;
        }

        [Transaction(TransactionMode.Manual)]
        class FormCaller : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                try
                {
                    PCF_Importer_form fm = new PCF_Importer_form(commandData, ref message);
                    fm.ShowDialog();
                    Properties.Settings.Default.Save();
                    fm.Close();
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
    }
}
