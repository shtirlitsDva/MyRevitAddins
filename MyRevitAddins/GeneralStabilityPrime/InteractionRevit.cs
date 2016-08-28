using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Shared;
using Autodesk.Revit.DB;
using fi = Shared.Filter;
using ut = Shared.Util;

namespace GeneralStability
{
    public class InteractionRevit
    {
        public FamilyInstance Origo { get; } //Holds the Origo family instance
        public WallData WallsAlong { get; }
        public WallData WallsCross { get; }

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
            WallsAlong = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Along", doc);
            WallsCross = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Cross", doc);



            
        }

        public class WallData
        {
            private IList<FamilyInstance> WallSymbols { get; }
            private IList<double> Length { get; }

            public WallData(string familyName, Document doc)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                WallSymbols = collector.WherePasses(fi.FamInstOfDetailComp())
                    .WherePasses(fi.ParameterValueFilter(familyName,
                        BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM))
                    .OrderBy(x => int.Parse(x.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString())) //Mark must be filled with integer numbers
                    .Cast<FamilyInstance>()
                    .ToList();

                foreach (FamilyInstance fi in WallSymbols)
                {
                    LocationCurve loc = fi.Location as LocationCurve;
                    double length = ut.FootToMeter(loc.Curve.Length);
                    Length.Add(length);
                }
                
            }
        }
    }
}
