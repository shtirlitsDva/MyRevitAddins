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
    public class InteractionRevit
    {
        public FamilyInstance Origo { get; } //Holds the Origo family instance
        public IList<FamilyInstance> WallsAlong { get; }
        public IList<FamilyInstance> WallsCross { get; }

        public InteractionRevit(Document doc)
        {
            //Get the Origo component
            FilteredElementCollector colOrigo = new FilteredElementCollector(doc);
            Origo = colOrigo
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Origo: Origin",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>()
                .FirstOrDefault();

            //Gather the detail components
            FilteredElementCollector colAlong = new FilteredElementCollector(doc);
            WallsAlong = colAlong
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Stabilizing_Wall: Stabilizing Wall - Along",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>().ToList();

            FilteredElementCollector colCross = new FilteredElementCollector(doc);
            WallsCross = colCross
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Stabilizing_Wall: Stabilizing Wall - Cross",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>().ToList();
        }
    }
}
