using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils.SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Input;
using dbg = Shared.Dbg;
using fi = Shared.Filter;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using mp = Shared.MepUtils;
using tr = Shared.Transformation;
using Autodesk.Revit.Attributes;
using NLog;

namespace MEPUtils.Treeview_test
{
    [Transaction(TransactionMode.Manual)]
    public class TreeviewTest : IExternalCommand
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("G:\\Github\\shtirlitsDva\\MyRevitAddins\\MyRevitAddins\\SetTagsModeless\\NLog.config");

            FilteredElementCollector col = new FilteredElementCollector(doc);

            List<ElementFilter> catFilter = new List<ElementFilter>();
            catFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting));
            catFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory));
            catFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves));

            col.WherePasses(new LogicalAndFilter(new List<ElementFilter>
                                                    {new LogicalOrFilter(catFilter),
                                                        new LogicalOrFilter(new List<ElementFilter>
                                                        {
                                                            new ElementClassFilter(typeof(Pipe)),
                                                            new ElementClassFilter(typeof(FamilyInstance))
                                                        })}));

            HashSet<Element> els = new HashSet<Element>(col.ToElements());

            PropertiesInformation[] PropsList = new PropertiesInformation[]
                            { new PropertiesInformation(true, "System Abbreviation", BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM, els),
                              new PropertiesInformation(true, "Category Name", BuiltInParameter.ELEM_CATEGORY_PARAM, els),
                              new PropertiesInformation(true, "Family and Type Name", BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM, els) };

            var dt = new DataTable();

            dt.Columns.Add("Name", typeof(string));
            //for (int i = 0; i < PropsList.Length; i++)
            //{
            //    dt.Columns.Add(PropsList[i].Name, typeof(string));
            //}
            DataColumn dc = new DataColumn("ParentNode", typeof(string));
            dc.AllowDBNull = true;
            dt.Columns.Add(dc);

            //Root node
            DataRow row = dt.NewRow();
            row["Name"] = "All";
            dt.Rows.Add(row);

            //Hierarchy nodes
            for (int i = 0; i < PropsList.Length; i++)
            {
                if (i == 0) //Special case for first iteration
                {
                    foreach (string nodeName in PropsList[i].distinctValues)
                    {
                        row = dt.NewRow(); row["Name"] = nodeName; row["ParentNode"] = "All";
                    }
                }
                else
                {
                    foreach (string nodeName in PropsList[i].distinctValues)
                    {
                        PropertiesInformation piNode = PropsList[i];

                        foreach (string parentName in PropsList[i - 1].distinctValues)
                        {
                            PropertiesInformation piParent = PropsList[i - 1];
                            //Test to see if the combination exists
                            if (els.Any(x => piNode.getBipValue(x) == nodeName && piParent.getBipValue(x) == parentName))
                            {
                                row = dt.NewRow(); row["Name"] = nodeName; row["ParentNode"] = parentName;

                                //If on last iteration, then write the single element information
                                HashSet<Element> 
                            }
                        }
                    }
                }
            }

            return Result.Succeeded;
        }
    }

    public class PropertiesInformation
    {
        public bool IsBuiltIn { get; private set; } = true;
        public string Name { get; private set; }
        public BuiltInParameter Bip { get; private set; }
        public string getBipValue(Element e) => e.get_Parameter(Bip).ToValueString();
        public HashSet<string> distinctValues = new HashSet<string>();
        public PropertiesInformation(bool isBuiltIn, string name, BuiltInParameter bip, HashSet<Element> els)
        {
            IsBuiltIn = isBuiltIn; Name = name; Bip = bip;
            distinctValues = new HashSet<string>(els.GroupBy(x => getBipValue(x)).Select(x => x.Key));
        }
    }
}
