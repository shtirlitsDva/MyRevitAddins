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

        private void InsulateElement(Document doc, Element e)
        {
            var insulationData = GetInsulationParameters();
            var settings = GetInsulationSettings(doc);

            string pipeInsulationName = "Rørisolering";

            var allInsulationTypes = fi.GetElements<PipeInsulationType>(doc);
            var insulationType = (from PipeInsulationType pit in allInsulationTypes
                where pit.Name == pipeInsulationName
                select pit).FirstOrDefault();

            if (insulationType == null)
            {
                ut.ErrorMsg("Create Pipe Insulation Type with name " + pipeInsulationName);
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
