using System;
using System.Collections.Generic;
using System.Linq;
//using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Text;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using mp = Shared.MyMepUtils;
//using mySettings = GeneralStability.Properties.Settings;

namespace MEPUtils
{
    public class PipeInsulationVisibility
    {
        public static void TogglePipeInsulationVisibility(ExternalCommandData commandData)
        {
            var app = commandData.Application;
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;

            Category pipeInsCat = Category.GetCategory(doc, BuiltInCategory.OST_PipeInsulations);

            View curView = doc.ActiveView;

            //Handle built in category Pipe Insulations
            if (curView.GetCategoryHidden(pipeInsCat.Id))
            {
                curView.SetCategoryHidden(pipeInsCat.Id, false);
            }
            else
            {
                curView.SetCategoryHidden(pipeInsCat.Id, true);
            }

            //Handle custom Insulation category I use in Tees
            Category pipeFitCat = Category.GetCategory(doc, BuiltInCategory.OST_PipeFitting);

            var pipeFitCatSubs = pipeFitCat.SubCategories;
            var insulCat = (from Category cat in pipeFitCatSubs where cat.Name == "Insulation" select cat).FirstOrDefault();

            if (insulCat != null)
            {
                if (!curView.GetCategoryHidden(pipeInsCat.Id))
                {
                    curView.SetCategoryHidden(insulCat.Id, false);
                }
                else
                {
                    curView.SetCategoryHidden(insulCat.Id, true);
                }
            }

        }
    }
}

