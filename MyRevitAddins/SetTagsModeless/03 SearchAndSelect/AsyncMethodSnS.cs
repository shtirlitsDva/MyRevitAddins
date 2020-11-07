using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static Shared.Filter;
using WinForms = System.Windows.Forms;

namespace MEPUtils.ModelessForms.SearchAndSelect
{
    class AsyncSelectByFilters : IAsyncCommand
    {
        SelectionPredicateContainer Payload;
        private AsyncSelectByFilters() { }
        public AsyncSelectByFilters(SelectionPredicateContainer payload)
        {
            Payload = payload;
        }

        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public void Execute(UIApplication uiApp)
        {
            #region LoggerSetup
            //Nlog configuration
            var nlogConfig = new NLog.Config.LoggingConfiguration();
            //Targets
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "g:\\GitHub\\log.txt", DeleteOldFileOnStartup = true };
            //Rules
            nlogConfig.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            //Apply config
            NLog.LogManager.Configuration = nlogConfig;
            //DISABLE LOGGING
            NLog.LogManager.DisableLogging();
            #endregion

            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            Selection selection = uidoc.Selection;

            FilteredElementCollector col = new FilteredElementCollector(doc);

            //Test to see if catfilter is populated
            if (Payload.CategoriesToSearch.Count < 1) return;

            List<ElementFilter> catFilter = new List<ElementFilter>();
            if (Payload.CategoriesToSearch.Contains("Pipe Fittings"))
                catFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting));
            if (Payload.CategoriesToSearch.Contains("Pipe Accessories"))
                catFilter.Add(new ElementCategoryFilter(BuiltInCategory.OST_PipeAccessory));
            if (Payload.CategoriesToSearch.Contains("Pipes"))
                catFilter.Add(new ElementClassFilter(typeof(Pipe)));

            col.WherePasses(new LogicalAndFilter(new List<ElementFilter>
                                                    {new LogicalOrFilter(catFilter),
                                                        new LogicalOrFilter(new List<ElementFilter>
                                                        {
                                                            new ElementClassFilter(typeof(Pipe)),
                                                            new ElementClassFilter(typeof(FamilyInstance))
                                                        })}));

            selection.SetElementIds(col.ToElementIds());
        }
    }
}
