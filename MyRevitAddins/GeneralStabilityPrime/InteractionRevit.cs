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
using op = Shared.Output;
using mySettings = GeneralStability.Properties.Settings;

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

            //private readonly string _debugFilePath = mySettings.Default.debugFilePath;

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

                //Analyze the geometry to get x and y values
                //Obtain the transform from the Origo family
                Transform trf = Origo.GetTransform();
                trf = trf.Inverse;

                //StringBuilder sb = new StringBuilder();

                foreach (FamilyInstance fi in WallSymbols)
                {
                    //Get the location points of the wall symbols
                    LocationCurve loc = fi.Location as LocationCurve;
                    
                    //Collect the length of the walls
                    Length.Add(loc.Curve.Length.ToMeters());

                    //Get the end and start points
                    Curve locCurve = loc.Curve;
                    XYZ start = locCurve.GetEndPoint(0);
                    XYZ end = locCurve.GetEndPoint(1);

                    //Transform the points
                    XYZ tStart = trf.OfPoint(start);
                    XYZ tEnd = trf.OfPoint(end);

                    double sX = tStart.X.ToMeters(), sY = tStart.Y.ToMeters(), eX = tEnd.X.ToMeters(), eY = tEnd.Y.ToMeters();

                    //sb.Append("Wall ("+ sX +","+ sY + ") <> ");
                    //sb.Append("(" + eX + "," + eY + ") <> ");
                    //sb.Append(loc.Curve.Length.ToMeters());
                    //sb.AppendLine();

                    //Take advantage of the fact that X or Y is equal
                    if (sX.Equals(eX)) X.Add(sX);
                    else if (sY.Equals(eY)) Y.Add(sY);
                    else throw new Exception("No equal coordinates found!!!\n");

                    //Collect the width of the wall from the wall symbol
                    Thickness.Add(fi.LookupParameter("GS_Width").AsDouble().ToMillimeters());
                }

                //op.WriteDebugFile(_debugFilePath, sb);
            }
        }
    }
}
