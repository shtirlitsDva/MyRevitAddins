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
using Autodesk.Revit.DB.Mechanical;
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

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create all insulation");

                foreach (Element element in pipes) InsulateElement(doc, element);
                foreach (Element element in fittings) InsulateElement(doc, element);
                foreach (Element element in accessories) InsulateElement(doc, element);
                
                tx.Commit();
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
            if (pipeInsulationName == null) return;
            PipeInsulationType pipeInsulationType =
                fi.GetElements<PipeInsulationType>(doc, pipeInsulationName, BuiltInParameter.ALL_MODEL_TYPE_NAME).FirstOrDefault();

            //Declare insulation thickness vars
            double insThickness = 0;
            double dia;

            //Process the elements
            if (e is Pipe pipe)
            {
                //Start by reading the PipeInsulationType name from settings and so on.
                dia = pipe.Diameter.FtToMm().Round(0);
            }
            else if (e is FamilyInstance fi)
            {
                //Case: Pipe Accessory with defined setting
                //Read element insulation settings
                if (insSet.AsEnumerable().Any(row => row.Field<string>("FamilyAndType")
                == e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString()))
                {
                    var query = insSet.AsEnumerable()
                        .Where(row => row.Field<string>("FamilyAndType") == e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString())
                        .Select(row => row.Field<string>("AddInsulation"));
                    bool value = bool.Parse(query.FirstOrDefault());
                    if (!value) return;

                    //Retrieve connector dimensions
                    var cons = mp.GetConnectors(e);
                    dia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                }
                //Case: Pipe Fitting
                else if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                {
                    var mf = fi.MEPModel as MechanicalFitting;
                    //Case: Tee
                    if (mf.PartType.ToString() == "Tee")
                    {
                        //See if tee already has insulation and delete it
                        Parameter parInsType = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
                        if (parInsType.HasValue) doc.Delete(PipeInsulation.GetInsulationIds(doc, e.Id));

                        //Retrieve connector dimensions
                        var cons = mp.GetConnectors(e);
                        dia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                        insThickness = ReadThickness();
                        if (insThickness == 0) return;
                        Parameter par = e.LookupParameter("Insulation Projected");
                        if (par == null) return;
                        par.Set(insThickness);
                        return;
                    }
                    //Case: Reducer
                    else if (mf.PartType.ToString() == "Transition")
                    {
                        //Retrieve connector dimensions
                        var cons = mp.GetConnectors(e);
                        double primDia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                        double secDia = (cons.Secondary.Radius * 2).FtToMm().Round(0);

                        if (primDia > secDia) dia = secDia;
                        else dia = primDia;
                    }
                    //Case: Other fitting
                    else
                    {
                        //Retrieve connector dimensions
                        var cons = mp.GetConnectors(e);
                        dia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                    }
                }
                //Case: None of the above
                else return;

            }
            else return;

            insThickness = ReadThickness();

            double ReadThickness()
            {
                string insThicknessAsReadFromDataTable = dh.ReadParameterFromDataTable(sysAbbr, insPar, dia.ToString());
                if (insThicknessAsReadFromDataTable == null) return 0;
                return double.Parse(insThicknessAsReadFromDataTable).Round(0).MmToFt();
            }
                        
            PipeInsulation.Create(doc, e.Id, pipeInsulationType.Id, insThickness);
        }

        public static Result DeleteAllPipeInsulation(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;

            var allInsulation = fi.GetElements<PipeInsulation>(doc);
            if (allInsulation == null) return Result.Failed;
            else if (allInsulation.Count == 0) return Result.Failed;

            //var fittings = fi.GetElements(doc, BuiltInCategory.OST_PipeFitting).Cast<FamilyInstance>().ToHashSet();
            //var tees = from FamilyInstance e in fittings where mf (e.MEPModel as MechanicalFitting) 

            Transaction tx = new Transaction(doc);
            tx.Start("Delete all insulation!");
            foreach (Element el in allInsulation) doc.Delete(el.Id);
            tx.Commit();

            return Result.Succeeded;
        }
    }
}
