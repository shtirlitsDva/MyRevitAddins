using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Shared;
using Autodesk.Revit.DB;
using fi = Shared.Filter;

namespace GeneralStability
{
    class RevitInteraction
    {
        public static void InteractWithRevit(Document doc)
        {
            //Get the Origo component
            FilteredElementCollector colOrigo = new FilteredElementCollector(doc);
            var origo = colOrigo
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Origo: Origin",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>()
                .FirstOrDefault();

            //Gather the detail components
            FilteredElementCollector colAlong = new FilteredElementCollector(doc);
            var wallsAlong = colAlong
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Stabilizing_Wall: Stabilizing Wall - Along",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>();

            FilteredElementCollector colCross = new FilteredElementCollector(doc);
            var wallsCross = colCross
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Stabilizing_Wall: Stabilizing Wall - Cross",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>();
        }
    }
}
