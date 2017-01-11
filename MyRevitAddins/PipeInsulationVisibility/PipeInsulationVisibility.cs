using System;
using System.Collections.Generic;
using System.Linq;
//using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using mp = Shared.MyMepUtils;
//using mySettings = GeneralStability.Properties.Settings;

namespace PipeInsulationVisibility
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
            if (curView.GetCategoryHidden(pipeInsCat.Id))
            {
                curView.SetCategoryHidden(pipeInsCat.Id, false);
            }
            else
            {
                curView.SetCategoryHidden(pipeInsCat.Id, true);
            }
        }
    }
}

