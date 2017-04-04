using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using MoreLinq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;
using pd = PED.ParameterData;
using pdef = PED.ParameterDefinition;
using pl = PED.ParameterList;

namespace PED
{
    public class InitPED
    {
        public Result CreateElementBindings(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application app = doc.Application;
            Autodesk.Revit.Creation.Application ca = doc.Application.Create;

            Category pipeCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeCurves);
            //Category fittingCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeFitting);
            //Category accessoryCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeAccessory);

            CategorySet pipeSet = ca.NewCategorySet();
            pipeSet.Insert(pipeCat);

            //CategorySet elemSet = ca.NewCategorySet();
            //elemSet.Insert(fittingCat);
            //elemSet.Insert(accessoryCat);

            string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string oriFile = app.SharedParametersFilename;
            string tempFile = ExecutingAssemblyPath + "Temp.txt";

            StringBuilder sbFeedback = new StringBuilder();

            string domain = "PIPE";
            CreateBinding(domain, tempFile, app, doc, pipeSet, sbFeedback);

            //domain = "ELEM";
            //CreateBinding(domain, tempFile, app, doc, elemSet, sbFeedback);

            //ut.InfoMsg(sbFeedback.ToString());

            app.SharedParametersFilename = oriFile;

            return Result.Succeeded;
        }

        public Result PopulateParameters(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            HashSet<Element> elements = fi.GetElements<Pipe>(doc).Cast<Element>().ToHashSet();
            SetWallThicknessPipes(elements);
            return Result.Succeeded;
        }

        public static void SetWallThicknessPipes(HashSet<Element> elements)
        {
            //Wallthicknes for pipes are hardcoded until further notice
            //Values are from 10216-2 - Seamless steel tubes for pressure purposes
            //TODO: Implement a way to read values from excel
            Dictionary<int, double> pipeWallThk = new Dictionary<int, double>
            {
                [10] = 1.8,
                [15] = 2.0,
                [20] = 2.3,
                [25] = 2.6,
                [32] = 2.6,
                [40] = 2.6,
                [50] = 2.9,
                [65] = 2.9,
                [80] = 3.2,
                [100] = 3.6,
                [125] = 4.0,
                [150] = 4.5,
                [200] = 6.3,
                [250] = 6.3,
                [300] = 7.1,
                [350] = 8.0,
                [400] = 8.8,
                [450] = 10.0,
                [500] = 11.0,
                [600] = 12.5
            };

            Dictionary<int, double> outerD = new Dictionary<int, double>
            {
                [10] = 17.2,
                [15] = 21.3,
                [20] = 26.9,
                [25] = 33.7,
                [32] = 42.4,
                [40] = 48.3,
                [50] = 60.3,
                [65] = 76.1,
                [80] = 88.9,
                [100] = 114.3,
                [125] = 139.7,
                [150] = 168.3,
                [200] = 219.1,
                [250] = 273.0,
                [300] = 323.9,
                [350] = 355.6,
                [400] = 406.4,
                [450] = 457.0,
                [500] = 508.0,
                [600] = 610.0
            };

            pdef wallThkDef = pl.PED_PIPE_WALLTHK;

            foreach (Element element in elements)
            {
                //See if the parameter already has value and skip element if it has
                if (!element.get_Parameter(wallThkDef.Guid).HasValue) continue;

                //Retrieve the correct wallthickness from dictionary and set it on the element
                Parameter wallThkParameter = element.get_Parameter(wallThkDef.Guid);

                //Get connector set for the pipes
                ConnectorSet connectorSet = mp.GetConnectorSet(element);

                Connector c1 = null;
                
                //Filter out non-end types of connectors
                c1 = (from Connector connector in connectorSet
                      where connector.ConnectorType.ToString().Equals("End")
                      select connector).FirstOrDefault();

                string source = Conversion.PipeSizeToMm(c1.Radius);
                int dia = Convert.ToInt32(source);
                pipeWallThk.TryGetValue(dia, out double data);
                wallThkParameter.Set(data.MmToFeet());
            }
        }

        internal void CreateBinding(string domain, string tempFile, Application app, Document doc, CategorySet catSet, StringBuilder sbFeedback)
        {
            //Parameter query
            
            var query = from pdef p in new pl().PL where string.Equals(p.Domain, domain) select p;
            //Create parameter bindings
            try
            {
                foreach (pdef parameter in query.ToList())
                {
                    using (File.Create(tempFile)) { }
                    app.SharedParametersFilename = tempFile;
                    ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(parameter.Name, parameter.Type)
                    {
                        GUID = parameter.Guid
                    };
                    ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("TemporaryDefinitionGroup").Definitions.Create(options) as ExternalDefinition;

                    BindingMap map = doc.ParameterBindings;
                    Binding binding = app.Create.NewInstanceBinding(catSet);

                    if (map.Contains(def)) sbFeedback.Append("Parameter " + parameter.Name + " already exists.\n");
                    else
                    {
                        map.Insert(def, binding, pd.PCF_BUILTIN_GROUP_NAME);
                        if (map.Contains(def)) sbFeedback.Append("Parameter " + parameter.Name + " added to project.\n");
                        else sbFeedback.Append("Creation of parameter " + parameter.Name + " failed for some reason.\n");
                    }
                    File.Delete(tempFile);
                }

            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    public static class ParameterData
    {
        //The group to place the parameters
        public const BuiltInParameterGroup PCF_BUILTIN_GROUP_NAME = BuiltInParameterGroup.PG_ANALYTICAL_MODEL;

        //Parametertypes
        public const ParameterType Text = ParameterType.Text;
        public const ParameterType Integer = ParameterType.Integer;
        public const ParameterType Length = ParameterType.Length;
    }

    public class ParameterDefinition
    {
        public ParameterDefinition(string pName, string pDomain, string pUsage, ParameterType pType, Guid pGuid, string pKeyword = "", string pExportingTo = "")
        {
            Name = pName;
            Domain = pDomain;
            Usage = pUsage; //U = user, P = programmatic
            Type = pType;
            Guid = pGuid;
            Keyword = pKeyword;
            ExportingTo = pExportingTo;
        }

        public string Name { get; }
        public string Domain { get; } //PIPL = Pipeline, PIPE = Pipe, ELEM = accessory, fitting, SUPP = Support.
        public string Usage { get; } //U = user defined values, P = programatically defined values.
        public ParameterType Type { get; }
        public Guid Guid { get; }
        public string Keyword { get; } //The keyword as defined in the PCF reference guide.
        public string ExportingTo { get; } //Currently used with CII export to distinguish CII parameters from other PIPL parameters.
    }

    public class ParameterList
    {
        #region Parameter Definition
        //Element parameters user defined
        public static readonly pdef PED_PIPE_WALLTHK = new pdef("PED_PIPE_WALLTHK", "PIPE", "P", pd.Length, new Guid("B87FD9E8-D2B6-4560-B481-9586EF65FCFE"));
        //public static readonly pdef PED_ELEM_ = new pdef("PED_ELEM_WALLTHK1", "ELEM", "P", pd.Length, new Guid("8290FACA-6A0A-4F33-8C46-3F5639CA4A12"));
        //public static readonly pdef PED_ELEM_WALLTHK2 = new pdef("PED_ELEM_WALLTHK2", "ELEM", "P", pd.Length, new Guid("E4325364-28FC-448F-9CE0-3CA2AF5AF416"));
        //public static readonly pdef PED_ELEM_TYPE = new pdef("PED_ELEM_TYPE", "ELEM", "P", pd.Text, new Guid("CE11C016-965D-44C9-B6FC-041F9F65C286"));
        //public static readonly pdef PED_ELEM_MODEL = new pdef("PED_ELEM_MODEL", "ELEM", "P", pd.Text, new Guid("4A40907D-E4BE-43D6-BD88-BF961AF8D6A3"));
        #endregion

        #region Parameter List
        //Populate the list with element parameters
        public readonly HashSet<pdef> PL = new HashSet<pdef>()
        {
            PED_PIPE_WALLTHK,
            //PED_ELEM_WALLTHK1,
            //PED_ELEM_WALLTHK2,
            //PED_ELEM_TYPE,
            //PED_ELEM_MODEL
        };
        #endregion

    }
}
