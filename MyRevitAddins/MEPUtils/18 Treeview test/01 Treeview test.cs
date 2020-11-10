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
            Selection selection = uidoc.Selection;

            //LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("G:\\Github\\shtirlitsDva\\MyRevitAddins\\MyRevitAddins\\SetTagsModeless\\NLog.config");

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

            Treeview_testForm tvtest = new Treeview_testForm(els, PropsList, commandData);

            tvtest.ShowDialog();

            return Result.Succeeded;
        }
    }

    public class PropertiesInformation
    {
        public bool IsBuiltIn { get; private set; } = true;
        public string Name { get; private set; }
        public BuiltInParameter Bip { get; private set; }
        public string getBipValue(Element e) => e.get_Parameter(Bip).ToValueString2();
        public HashSet<string> distinctValues = new HashSet<string>();
        public PropertiesInformation(bool isBuiltIn, string name, BuiltInParameter bip, HashSet<Element> els)
        {
            IsBuiltIn = isBuiltIn; Name = name; Bip = bip;
            distinctValues = new HashSet<string>(els.GroupBy(x => getBipValue(x)).Select(x => x.Key));
        }
    }
}
