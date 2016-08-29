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
            WallsAlong = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Along", Origo, doc);
            WallsCross = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Cross", Origo, doc);



            
        }

        public class WallData
        {
            public IList<FamilyInstance> WallSymbols { get; }
            public IList<double> Length { get; } = new List<double>();
            public IList<double> X { get; } = new List<double>();
            public IList<double> Y { get; } = new List<double>();
            public IList<double> Thickness { get; } = new List<double>();

            public WallData(string familyName, FamilyInstance Origo, Document doc)
            {
                //Get the relevant wall symbols
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                WallSymbols = collector.WherePasses(fi.FamInstOfDetailComp())
                    .WherePasses(fi.ParameterValueFilter(familyName,
                        BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM))
                    .OrderBy(x => int.Parse(x.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString())) //Mark must be filled with integer numbers
                    .Cast<FamilyInstance>()
                    .ToList();

                //Determine the length of each wall symbol
                foreach (FamilyInstance fi in WallSymbols)
                {
                    LocationCurve loc = fi.Location as LocationCurve;
                    double length = ut.FootToMeter(loc.Curve.Length);
                    Length.Add(length);
                }

                //Analyze the geometry to get x and y values
                Transform trf = Origo.GetTransform();

                foreach (FamilyInstance fi in WallSymbols)
                {
                    //Get the location points of the wall symbols
                    LocationCurve loc = fi.Location as LocationCurve;
                    Curve locCurve = loc.Curve;
                    XYZ start = locCurve.GetEndPoint(0);
                    XYZ end = locCurve.GetEndPoint(1);

                    //Transform the points
                    XYZ tStart = trf.OfPoint(start);
                    XYZ tEnd = trf.OfPoint(end);

                    //Take advantage of the fact that X or Y is equal
                    if (tStart.X.Equals(tEnd.X)) X.Add(tStart.X);
                    else if (tStart.Y.Equals(tEnd.Y)) Y.Add(tStart.Y);
                    else throw new Exception("No equal coordinates found!!!"); 
                }
            }
        }
    }
}
