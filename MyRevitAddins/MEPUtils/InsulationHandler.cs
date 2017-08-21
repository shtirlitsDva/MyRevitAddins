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

                //TODO: Split the InsulateElement into three methods for each kind -- I think it would make it more simple
                foreach (Element element in pipes) InsulatePipe(doc, element); //Works
                foreach (Element element in fittings) InsulateFitting(doc, element);
                foreach (Element element in accessories) InsulateAccessory(doc, element);

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

        private static void InsulatePipe(Document doc, Element e)
        {
            #region Initialization
            //Read configuration data
            var insPar = GetInsulationParameters();

            //Read common configuration values
            string sysAbbr = e.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

            //Declare insulation thickness vars
            var dia = ((Pipe)e).Diameter.FtToMm().Round(0);
            double specifiedInsulationThickness = ReadThickness(sysAbbr, insPar, dia); //In feet already
            #endregion

            //Retrieve insulation type parameter and see if the pipe is already insulated
            Parameter parInsTypeCheck = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
            if (parInsTypeCheck.HasValue)
            {
                //Case: If the pipe is already insulated, check to see if insulation is correct
                Parameter parInsThickness = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
                double existingInsulationThickness = parInsThickness.AsDouble(); //In feet

                //Test if existing thickness is as specified
                //If ok -> do nothing, if not -> fix it
                if (!specifiedInsulationThickness.Equalz(existingInsulationThickness, 1.0e-9))
                {
                    ElementId id = InsulationLiningBase.GetInsulationIds(doc, e.Id).FirstOrDefault();
                    if (id == null) return;
                    PipeInsulation insulation = doc.GetElement(id) as PipeInsulation;
                    if (insulation == null) return;
                    insulation.Thickness = specifiedInsulationThickness;
                }
            }
            else
            {
                //Case: If no insulation -> add insulation
                //Read pipeinsulation type and get the type
                string pipeInsulationName = dh.ReadParameterFromDataTable(sysAbbr, insPar, "Type");
                if (pipeInsulationName == null) return;
                PipeInsulationType pipeInsulationType =
                    fi.GetElements<PipeInsulationType>(doc, pipeInsulationName, BuiltInParameter.ALL_MODEL_TYPE_NAME).FirstOrDefault();
                if (pipeInsulationType == null) throw new Exception($"No pipe insulation type named {pipeInsulationName}!");

                //Create insulation
                PipeInsulation.Create(doc, e.Id, pipeInsulationType.Id, specifiedInsulationThickness);
            }

        }

        private static void InsulateFitting(Document doc, Element e)
        {
            #region Initialization
            //Read configuration data
            var insPar = GetInsulationParameters();

            //Read common configuration values
            string sysAbbr = e.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

            //Declare insulation thickness vars
            #region Retrieve fitting diameter
            double dia;
            //Retrieve specified insulation thickness
            var mf = ((FamilyInstance)e).MEPModel as MechanicalFitting;
            //Case: Reducer
            if (mf.PartType.ToString() == "Transition")
            {
                //Retrieve connector dimensions
                var cons = mp.GetConnectors(e);

                //Insulate after the larger diameter
                double primDia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                double secDia = (cons.Secondary.Radius * 2).FtToMm().Round(0);

                dia = primDia > secDia ? primDia : secDia;
            }
            //Case: Other fitting
            else
            {
                //Retrieve connector dimensions
                var cons = mp.GetConnectors(e);
                dia = (cons.Primary.Radius * 2).FtToMm().Round(0);
            }
            #endregion

            #region Read specified Insulation Thickness
            double specifiedInsulationThickness = ReadThickness(sysAbbr, insPar, dia); //In feet
            #endregion
            #endregion

            //Retrieve insulation type parameter and see if the fitting is already insulated
            Parameter parInsTypeCheck = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
            if (parInsTypeCheck.HasValue)
            {
                //Case Tee: If the element is a tee, delete any existing insulation
                if (mf.PartType.ToString() == "Tee")
                {
                    doc.Delete(InsulationLiningBase.GetInsulationIds(doc, e.Id));
                    InsulateTee();
                    return;
                }

                //Case: If the fitting is already insulated, check to see if insulation is correct
                Parameter parInsThickness = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
                double existingInsulationThickness = parInsThickness.AsDouble(); //In feet

                //Test if existing thickness is as specified
                //If ok -> do nothing, if not -> fix it
                if (!specifiedInsulationThickness.Equalz(existingInsulationThickness, 1.0e-9))
                {
                    ElementId id = InsulationLiningBase.GetInsulationIds(doc, e.Id).FirstOrDefault();
                    if (id == null) return;
                    PipeInsulation insulation = doc.GetElement(id) as PipeInsulation;
                    if (insulation == null) return;
                    insulation.Thickness = specifiedInsulationThickness;
                }
            }
            else
            {
                if (mf.PartType.ToString() == "Tee")
                {
                    InsulateTee();
                    return;
                }

                //Case: If no insulation -> add insulation
                //Read pipeinsulation type and get the type
                string pipeInsulationName = dh.ReadParameterFromDataTable(sysAbbr, insPar, "Type");
                if (pipeInsulationName == null) return;
                PipeInsulationType pipeInsulationType =
                    fi.GetElements<PipeInsulationType>(doc, pipeInsulationName, BuiltInParameter.ALL_MODEL_TYPE_NAME).FirstOrDefault();
                if (pipeInsulationType == null) throw new Exception($"No pipe insulation type named {pipeInsulationName}!");

                //Create insulation
                PipeInsulation.Create(doc, e.Id, pipeInsulationType.Id, specifiedInsulationThickness);
            }

            //Local method to insulate Tees
            void InsulateTee()
            {
                if (specifiedInsulationThickness == 0) return;

                //Set insulation
                Parameter par1 = e.LookupParameter("Insulation Projected");
                if (par1 == null) return;
                par1.Set(specifiedInsulationThickness);

                //Make visible if not
                Parameter par2 = e.LookupParameter("Dummy Insulation Visible");
                if (par2.AsInteger() == 0) par2.Set(1);
            }
        }

        private static void InsulateAccessory(Document doc, Element e)
        {
            #region Initialization

            //Read configuration data
            var insPar = GetInsulationParameters();
            var insSet = GetInsulationSettings(doc);

            //Read common configuration values
            string sysAbbr = e.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

            //Declare insulation thickness vars
            #region Retrieve accessory diameter
            var cons = mp.GetConnectors(e);
            double dia = (cons.Primary.Radius * 2).FtToMm().Round(0);
            #endregion

            #region Read specified Insulation Thickness
            double specifiedInsulationThickness = ReadThickness(sysAbbr, insPar, dia); //In feet
            #endregion
            #endregion

            //See if the accessory is in the settings list, else return no action done
            if (insSet.AsEnumerable().Any(row => row.Field<string>("FamilyAndType")
                == e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString()))
            {
                //See if element is not allowed to be insulated
                var query = insSet.AsEnumerable()
                    .Where(row => row.Field<string>("FamilyAndType") == e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString())
                    .Select(row => row.Field<string>("AddInsulation"));
                bool value = bool.Parse(query.FirstOrDefault());

                //Retrieve insulation type parameter and see if the accessory is already insulated
                Parameter parInsTypeCheck = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
                if (parInsTypeCheck.HasValue)
                {
                    //If not allowed (false is read) negate the false to true to trigger the following if
                    //Delete any existing insulation and return
                    if (!value)
                    {
                        doc.Delete(InsulationLiningBase.GetInsulationIds(doc, e.Id));
                        return;
                    }

                    //Case: If the accessory is already insulated, check to see if insulation is correct
                    Parameter parInsThickness = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
                    double existingInsulationThickness = parInsThickness.AsDouble(); //In feet

                    //Test if existing thickness is as specified
                    //If ok -> do nothing, if not -> fix it
                    if (!specifiedInsulationThickness.Equalz(existingInsulationThickness, 1.0e-9))
                    {
                        ElementId id = InsulationLiningBase.GetInsulationIds(doc, e.Id).FirstOrDefault();
                        if (id == null) return;
                        PipeInsulation insulation = doc.GetElement(id) as PipeInsulation;
                        if (insulation == null) return;
                        insulation.Thickness = specifiedInsulationThickness;
                    }
                }
                else
                {
                    //Case: If no insulation -> add insulation if allowed
                    if (!value) return;

                    //Read pipeinsulation type and get the type
                    string pipeInsulationName = dh.ReadParameterFromDataTable(sysAbbr, insPar, "Type");
                    if (pipeInsulationName == null) return;
                    PipeInsulationType pipeInsulationType =
                        fi.GetElements<PipeInsulationType>(doc, pipeInsulationName, BuiltInParameter.ALL_MODEL_TYPE_NAME).FirstOrDefault();
                    if (pipeInsulationType == null) throw new Exception($"No pipe insulation type named {pipeInsulationName}!");

                    //Create insulation
                    PipeInsulation.Create(doc, e.Id, pipeInsulationType.Id, specifiedInsulationThickness);
                }
            }
        }

        private static void InsulateElement(Document doc, Element e)
        {
            #region Initialization

            //Read configuration data
            var insPar = GetInsulationParameters();
            var insSet = GetInsulationSettings(doc);

            //Read common configuration values
            string sysAbbr = e.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();

            //Declare insulation thickness vars
            double insThickness = 0;
            double dia;

            #endregion

            #region Tees

            //Case: Tees get their own treatment because of the custom parameters
            if (e is FamilyInstance famInst && e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                var mf = famInst.MEPModel as MechanicalFitting;
                if (mf.PartType.ToString() == "Tee")
                {
                    //See if tee already has insulation and delete it
                    Parameter parInsType = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
                    if (parInsType.HasValue) doc.Delete(PipeInsulation.GetInsulationIds(doc, e.Id));

                    //Retrieve connector dimensions
                    var cons = mp.GetConnectors(e);
                    dia = (cons.Primary.Radius * 2).FtToMm().Round(0);

                    //Read insulation setting
                    insThickness = ReadThickness(sysAbbr, insPar, dia);
                    if (insThickness == 0) return;

                    //Set insulation
                    Parameter par1 = e.LookupParameter("Insulation Projected");
                    if (par1 == null) return;
                    par1.Set(insThickness);

                    //Make visible if not
                    Parameter par2 = e.LookupParameter("Dummy Insulation Visible");
                    if (par2.AsInteger() == 0) par2.Set(1);

                    return;
                }
            }

            #endregion

            //If element already is insulated -- continue to next element
            Parameter parInsTypeCheck = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
            if (parInsTypeCheck.HasValue) return;

            //Read pipeinsulation type and get the type
            string pipeInsulationName = dh.ReadParameterFromDataTable(sysAbbr, insPar, "Type");
            if (pipeInsulationName == null) return;
            PipeInsulationType pipeInsulationType =
                fi.GetElements<PipeInsulationType>(doc, pipeInsulationName, BuiltInParameter.ALL_MODEL_TYPE_NAME).FirstOrDefault();
            if (pipeInsulationType == null) throw new Exception($"No pipe insulation type named {pipeInsulationName}!");

            //Process the elements
            if (e is FamilyInstance finst)
            {
                //Case: Pipe Accessory with defined setting
                //Read element insulation settings
                if (insSet.AsEnumerable().Any(row => row.Field<string>("FamilyAndType")
                == e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString()))
                {
                    //See if element is not allowed to be insulated
                    var query = insSet.AsEnumerable()
                        .Where(row => row.Field<string>("FamilyAndType") == e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString())
                        .Select(row => row.Field<string>("AddInsulation"));
                    bool value = bool.Parse(query.FirstOrDefault());

                    //If not allowed (false is read) negate the false to true to trigger the following if
                    //Delete any existing insulation and return
                    if (!value)
                    {
                        Parameter parInsType = e.get_Parameter(BuiltInParameter.RBS_REFERENCE_INSULATION_TYPE);
                        if (parInsType.HasValue) doc.Delete(PipeInsulation.GetInsulationIds(doc, e.Id));
                        return;
                    }

                    //Retrieve connector dimensions
                    var cons = mp.GetConnectors(e);
                    dia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                }
                //Case: Pipe Fitting
                else if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                {
                    var mf = finst.MEPModel as MechanicalFitting;
                    //Case: Reducer
                    if (mf.PartType.ToString() == "Transition")
                    {
                        //Retrieve connector dimensions
                        var cons = mp.GetConnectors(e);

                        //Insulate after the larger diameter
                        double primDia = (cons.Primary.Radius * 2).FtToMm().Round(0);
                        double secDia = (cons.Secondary.Radius * 2).FtToMm().Round(0);

                        dia = primDia > secDia ? primDia : secDia;
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

            insThickness = ReadThickness(sysAbbr, insPar, dia);

            PipeInsulation.Create(doc, e.Id, pipeInsulationType.Id, insThickness);
        }

        private static double ReadThickness(string sysAbbr, DataTable insPar, double dia)
        {
            string insThicknessAsReadFromDataTable = dh.ReadParameterFromDataTable(sysAbbr, insPar, dia.ToString());
            if (insThicknessAsReadFromDataTable == null) return 0;
            return double.Parse(insThicknessAsReadFromDataTable).Round(0).MmToFt();
        }

        public static Result DeleteAllPipeInsulation(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;

            var allInsulation = fi.GetElements<PipeInsulation>(doc);
            if (allInsulation == null) return Result.Failed;
            else if (allInsulation.Count == 0) return Result.Failed;

            var fittings = fi.GetElements(doc, BuiltInCategory.OST_PipeFitting).Cast<FamilyInstance>().ToHashSet();



            Transaction tx = new Transaction(doc);
            tx.Start("Delete all insulation!");
            foreach (Element el in allInsulation) doc.Delete(el.Id);

            foreach (FamilyInstance fi in fittings)
            {
                var mf = fi.MEPModel as MechanicalFitting;
                if (mf.PartType.ToString() == "Tee")
                {
                    //Set insulation to 0
                    Parameter par1 = fi.LookupParameter("Insulation Projected");
                    par1?.Set(0);

                    //Make invisible also
                    Parameter par2 = fi.LookupParameter("Dummy Insulation Visible");
                    if (par2.AsInteger() == 1) par2.Set(0);
                }
            }

            tx.Commit();

            return Result.Succeeded;
        }
    }
}
