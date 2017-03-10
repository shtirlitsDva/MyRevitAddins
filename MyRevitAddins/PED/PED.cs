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
            Category fittingCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeFitting);
            Category accessoryCat = doc.Settings.Categories.get_Item(BuiltInCategory.OST_PipeAccessory);

            CategorySet pipeSet = ca.NewCategorySet();
            pipeSet.Insert(pipeCat);

            CategorySet elemSet = ca.NewCategorySet();
            elemSet.Insert(fittingCat);
            elemSet.Insert(accessoryCat);

            string ExecutingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string oriFile = app.SharedParametersFilename;
            string tempFile = ExecutingAssemblyPath + "Temp.txt";

            StringBuilder sbFeedback = new StringBuilder();

            string domain = "PIPE";
            CreateBinding(domain, tempFile, app, doc, pipeSet, sbFeedback);

            domain = "ELEM";
            CreateBinding(domain, tempFile, app, doc, elemSet, sbFeedback);
            
            ut.InfoMsg(sbFeedback.ToString());

            app.SharedParametersFilename = oriFile;

            return Result.Succeeded;
        }

        public Result PupulateParameters()
        {
            return Result.Succeeded;
        }

        internal void CreateBinding(string domain, string tempFile, Application app, Document doc, CategorySet catSet, StringBuilder sbFeedback)
        {
            //Parameter query
            var query = from p in pl.PL where p.Domain == domain select p;
            //Create parameter bindings
            try
            {
                foreach (pdef parameter in query.ToList())
                {
                    using (File.Create(tempFile)) { }
                    app.SharedParametersFilename = tempFile;
                    ExternalDefinitionCreationOptions options = new ExternalDefinitionCreationOptions(parameter.Name, parameter.Type);
                    options.GUID = parameter.Guid;
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

    public static class ParameterList
    {
        public static readonly HashSet<pdef> PL = new HashSet<pdef>()
        {
            PED_PIPE_WALLTHK,
            PED_ELEM_WALLTHK1,
            PED_ELEM_WALLTHK2,
            PED_ELEM_TYPE,
            PED_ELEM_MODEL
        };

        #region Parameter Definition
        //Element parameters user defined
        public static readonly pdef PED_PIPE_WALLTHK = new pdef("PED_PIPE_WALLTHK", "PIPE", "P", pd.Length, new Guid("B87FD9E8-D2B6-4560-B481-9586EF65FCFE"));
        public static readonly pdef PED_ELEM_WALLTHK1 = new pdef("PED_ELEM_WALLTHK1", "ELEM", "P", pd.Length, new Guid("8290FACA-6A0A-4F33-8C46-3F5639CA4A12"));
        public static readonly pdef PED_ELEM_WALLTHK2 = new pdef("PED_ELEM_WALLTHK2", "ELEM", "P", pd.Length, new Guid("E4325364-28FC-448F-9CE0-3CA2AF5AF416"));
        public static readonly pdef PED_ELEM_TYPE = new pdef("PED_ELEM_TYPE", "ELEM", "P", pd.Text, new Guid("CE11C016-965D-44C9-B6FC-041F9F65C286"));
        public static readonly pdef PED_ELEM_MODEL = new pdef("PED_ELEM_MODEL", "ELEM", "P", pd.Text, new Guid("4A40907D-E4BE-43D6-BD88-BF961AF8D6A3"));
        #endregion

        #region Parameter List
        //Populate the list with element parameters

        #endregion

    }
}
