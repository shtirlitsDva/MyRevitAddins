using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using System.IO;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;
using dh = Shared.DataHandler;

namespace MEPUtils
{
    public class InsulationHandler
    {
        /// <summary>
        /// This method is used to set and save settings for insulation creation for Pipe Accessories (valves etc.)
        /// </summary>
        public Result ExecuteInsulationSettings(ExternalCommandData cData)
        {
            InsulationSettingsWindow isw = new InsulationSettingsWindow(cData);
            isw.ShowDialog();
            isw.Close();
            using (Stream stream = new FileStream(isw.PathToSettingsXml, FileMode.Create, FileAccess.Write))
            {
                isw.Settings.WriteXml(stream);
            }

            return Result.Succeeded;
        }

        public static Result CreateAllInsulation(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;

            //Collect all the elements to insulate
            var pipes = fi.GetElements(doc, BuiltInCategory.OST_PipeCurves);
            var fittings = fi.GetElements(doc, BuiltInCategory.OST_PipeFitting);
            var accessories = fi.GetElements(doc, BuiltInCategory.OST_PipeAccessory);

            foreach (Element element in pipes)
            {
                InsulateElement(doc, element);
            }


            return Result.Succeeded;
        }

        private static DataTable GetInsulationSettings(Document doc)
        {
            //Manage Insulation creation settings
            //Test if settings file exist
            string pn = doc.ProjectInformation.Name;
            string pathToSettingsXml =
                Environment.ExpandEnvironmentVariables(
                    $"%AppData%\\MyRevitAddins\\MEPUtils\\Settings.{pn}.Insulation.xml"); //Magic text?
            bool settingsExist = File.Exists(pathToSettingsXml);

            //Initialize an empty datatable
            DataTable settings = new DataTable("InsulationSettings");

            if (settingsExist) //Read file if exists
            {
                using (Stream stream = new FileStream(pathToSettingsXml, FileMode.Open, FileAccess.Read))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(stream);
                    settings = ds.Tables[0];
                }
            }
            else
                throw new Exception(
                    "Insulation creation settings file does not exist! Run configuration routine first!");
            return settings;
        }

        private static DataTable GetInsulationParameters()
        {
            //Manage Insulation parameters settings
            string pathToInsulationExcel =
                Environment.ExpandEnvironmentVariables("%AppData%\\MyRevitAddins\\MEPUtils\\Insulation.xlsx");
            bool fileExists = File.Exists(pathToInsulationExcel);
            if (!fileExists)
                throw new Exception(
                    @"No insulation configuration file exists at: %AppData%\MyRevitAddins\MEPUtils\Insulation.xlsx");

            DataSet insulationDataSet = DataHandler.ImportExcelToDataSet(pathToInsulationExcel, "YES");
            DataTable insulationData = DataHandler.ReadDataTable(insulationDataSet.Tables, "Insulation");
            return insulationData;
        }

        private static void InsulateElement(Document doc, Element e)
        {
            var insPar = GetInsulationParameters();
            var insSet = GetInsulationSettings(doc);

            //Read common configuration values
            string sysAbbr = e.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

            //Read pipeinsulation type and get the type
            string pipeInsulationName = dh.ReadParameterFromDataTable(sysAbbr, insPar, "Type");
            PipeInsulationType pipeInsulationType =
                fi.GetElements<PipeInsulationType>(doc, pipeInsulationName, BuiltInParameter.ALL_MODEL_TYPE_NAME).FirstOrDefault();
            if (pipeInsulationType == null) throw new Exception($"PipeInsulationType {pipeInsulationName} does not exist!");
            
            //Process the elements
            if (e is Pipe pipe)
            {
                //Start by reading the PipeInsulationType name from settings and so on.
                double dia = pipe.Diameter.FtToMm().Round(0);
                string insThicknessAsReadFromDataTable = dh.ReadParameterFromDataTable(sysAbbr, insPar, dia.ToString());
                double insThickness = double.Parse(insThicknessAsReadFromDataTable).Round(0).MmToFt();
            }

            
        }

        public static Result DeleteAllPipeInsulation(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;

            var allInsulation = fi.GetElements<PipeInsulation>(doc);
            if (allInsulation == null) return Result.Failed;
            else if (allInsulation.Count == 0) return Result.Failed;

            Transaction tx = new Transaction(doc);
            tx.Start("Delete all insulation!");
            foreach (Element el in allInsulation) doc.Delete(el.Id);
            tx.Commit();

            return Result.Succeeded;
        }
    }
}
