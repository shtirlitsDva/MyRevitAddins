using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using MoreLinq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Shared;
using Autodesk.Revit.DB;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using mySettings = GeneralStability.Properties.Settings;
using TxBox = System.Windows.Forms.TextBox;

namespace GeneralStability
{
    public class InteractionRevit
    {
        public FamilyInstance Origo { get; } //Holds the Origo family instance
        public WallData WallsAlong { get; }
        public WallData WallsCross { get; }
        public BoundaryData BoundaryData { get; }
        public LoadData LoadData { get; }


        public InteractionRevit(Document doc)
        {
            //Get the Origo component
            Origo = GetOrigo(doc);

            //Gather the detail components
            WallsAlong = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Along", Origo, doc);
            WallsCross = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Cross", Origo, doc);

            //Initialize boundary data
            BoundaryData = new BoundaryData("GS_Boundary", doc);

            //Initialize load data
            LoadData = new LoadData(doc);
        }

        #region LoadCalculation

        public Result CalculateLoads(Document doc, TxBox txBox)
        {
            try
            {
                //Log
                int nrI = 0, nrJ = 0, nrTotal = 1;
                StringBuilder sbLog = new StringBuilder();


                //Reset the load from last run
                foreach (FamilyInstance fi in WallsAlong.WallSymbols)
                {
                    fi.LookupParameter("GS_Load").Set(0);
                }

                //Get the transform
                Transform trfO = Origo.GetTransform();
                Transform trf = trfO.Inverse;

                //Get the list of boundaries (to simplify the synthax)
                HashSet<CurveElement> Bd = BoundaryData.BoundaryLines;

                //Get the faces of filled regions
                //Determine the intersection between the centre point of finite element and filled region symbolizing the load
                //TODO: Move this access of faces out of the for loop because it accesses them each time.
                IList<Face> faces = new List<Face>();

                Options options = new Options();
                options.ComputeReferences = true;
                options.View = LoadData.GS_View;

                foreach (FilledRegion fr in LoadData.LoadAreas)
                {
                    GeometryElement geometryElement = fr.get_Geometry(options);
                    foreach (GeometryObject go in geometryElement)
                    {
                        Solid solid = go as Solid;
                        Face face = solid.Faces.get_Item(0);
                        faces.Add(face);
                    }
                }

                //The analysis proceeds in steps of 1mm (hardcoded for now)
                double step = 20.0.MmToFeet(); //<-- a "magic" number. TODO: Implement definition of step size.

                //Determine the largest X value
                double Xmax = Bd.Max(x => EndPoint(x, trf).X);
                //Divide the largest X value by the step value to determine the number iterations in X direction
                int nrOfX = (int)Math.Floor(Xmax / step);
                //Log
                nrI = nrOfX;

                //Iterate through the length of the building analyzing the load
                for (int i = 0; i < nrOfX; i++)
                {
                    //Current x value
                    double x1 = i * step;
                    double x2 = (i + 1) * step;
                    double xC = x1 + step / 2;

                    //Select boundary lines in scope, but make sure Y boundaries are discarded
                    var boundaryX = (from CurveElement cu in Bd
                                     where StartPoint(cu, trf).X <= xC && EndPoint(cu, trf).X >= xC && !StartPoint(cu, trf).X.Equals(EndPoint(cu, trf).X)
                                     select cu).ToHashSet();
                    //Determine minimum and maximum Y values
                    double Ymin = boundaryX.Min(x => StartPoint(x, trf).Y);
                    double Ymax = boundaryX.Max(x => StartPoint(x, trf).Y);

                    //Determine relevant walls (that means the walls which are crossed by the current X value iteration)
                    var wallsX = (from FamilyInstance fi in WallsAlong.WallSymbols
                                  where StartPoint(fi, trf).X <= xC && EndPoint(fi, trf).X >= xC
                                  select fi).ToHashSet();

                    //Determine number of iterations in Y direction
                    int nrOfY = (int)Math.Floor((Ymax - Ymin) / step);

                    //Log
                    nrJ = nrOfY;

                    //Iterate through the width of the building
                    for (int j = 0; j < nrOfY; j++)
                    {
                        //Stopwatch 3
                        var watch3 = Stopwatch.StartNew();

                        #region watch3
                        //Current y value
                        double y1 = Ymin + j * step;
                        double y2 = Ymin + (j + 1) * step;
                        double yC = y1 + step / 2;
                        #endregion watch1

                        //watch1.Stop();
                        //TimeSpan time1 = watch1.Elapsed;
                        //sbLog.Append(", " + time1.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                        //#endregion

                        //#region Stopwatch2
                        //var watch2 = Stopwatch.StartNew();

                        #region watch2
                        //Determine nearest wall

                        FamilyInstance nearestWall = wallsX.MinBy(x => Math.Abs(StartPoint(x, trf).Y - yC));

                        #endregion watch2

                        //watch2.Stop();
                        //TimeSpan time2 = watch2.Elapsed;
                        //sbLog.Append(", " + time2.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                        //#endregion

                        //#region Stopwatch3
                        //var watch3 = Stopwatch.StartNew();

                        #region watch3
                        //Area of finite element
                        double areaSqrM = ((x2 - x1) * (y2 - y1)).SqrFeetToSqrMeters();
                        #endregion watch3

                        //watch3.Stop();
                        //TimeSpan time3 = watch3.Elapsed;
                        //sbLog.Append(", " + time3.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                        //#endregion

                        //#region Stopwatch4
                        //var watch4 = Stopwatch.StartNew();

                        #region watch4
                        //Determine the correct load intensity at the finite element centre point
                        XYZ cPointInOrigoCoords = new XYZ(xC, yC, 0);
                        XYZ cPointInGlobalCoords = trfO.OfPoint(cPointInOrigoCoords);

                        double loadIntensity = 0.0;
                        #endregion watch3

                        watch3.Stop();
                        TimeSpan time3 = watch3.Elapsed;
                        sbLog.Append(nrTotal + ", 3, " + time3.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + ", ");

                        //Stopwatch 4
                        var watch4 = Stopwatch.StartNew();

                        #region watch4
                        for (int f = 0; f < faces.Count; f++)
                        {
                            IntersectionResult result = faces[f].Project(cPointInGlobalCoords);
                            if (result == null) continue;
                            string rawLoadIntensity = LoadData.LoadAreas[f].get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
                            loadIntensity = double.Parse(rawLoadIntensity, CultureInfo.InvariantCulture);
                        }
                        #endregion watch4

                        watch4.Stop();
                        TimeSpan time4 = watch4.Elapsed;
                        sbLog.Append(nrTotal + ", 4, " + time4.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + ", ");


                        //Stopwatch 5
                        var watch5 = Stopwatch.StartNew();

                        #region watch5
                        double force = loadIntensity * areaSqrM;

                        #region watch7
                        double currentValue = nearestWall.LookupParameter("GS_Load").AsDouble();
                        #endregion watch7

                        //watch7.Stop();
                        //TimeSpan time7 = watch7.Elapsed;
                        //sbLog.Append(", " + time7.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                        //#endregion

                        //#region Stopwatch8
                        //var watch8 = Stopwatch.StartNew();

                        #region watch8

                        bool success = nearestWall.LookupParameter("GS_Load").Set(currentValue + force);
                        #endregion watch5

                        watch5.Stop();
                        TimeSpan time5 = watch5.Elapsed;
                        sbLog.Append(nrTotal + ", 5, " + time5.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + "\n");
                        
                        nrTotal++;

                    }
                }

                //Log
                txBox.Text = nrI + ", " + nrJ + ": " + nrTotal;
                op.WriteDebugFile(mySettings.Default.debugFilePath, sbLog);
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception(e.Message);
                return Result.Failed;
            }
        }

        #endregion


        /// <summary>
        /// Renumbers the wall symbols by following geometric pattern sorting by y and then x of start point.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static Result RenumberWallSymbols(Document doc)
        {
            try
            {
                //Get the origo family
                FamilyInstance Origo = GetOrigo(doc);

                //Get and assign number to walls along
                string name = "GS_Stabilizing_Wall: Stabilizing Wall - Along";
                HashSet<FamilyInstance> along = GetWallSymbolsUnordered(name, doc);
                IList<FamilyInstance> wallsAlongSorted = OrderGeometrically(along, Origo);

                int idx = 0;
                foreach (FamilyInstance fi in wallsAlongSorted)
                {
                    idx++;
                    fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(idx.ToString());
                }

                name = "GS_Stabilizing_Wall: Stabilizing Wall - Cross";
                HashSet<FamilyInstance> cross = GetWallSymbolsUnordered(name, doc);
                IList<FamilyInstance> wallsCrossSorted = OrderGeometrically(cross, Origo);

                idx = 0;
                foreach (FamilyInstance fi in wallsCrossSorted)
                {
                    idx++;
                    fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(idx.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Orders the LocationCurve based FamilyInstances according to their Y value then their X value.
        /// </summary>
        /// <param name="listToOrder">The list of FamilyInstances to order.</param>
        /// <param name="Origo">The reference FamilyInstance in whose coordinate system the sortin must be done.</param>
        /// <returns></returns>
        private static IList<FamilyInstance> OrderGeometrically(HashSet<FamilyInstance> listToOrder, FamilyInstance Origo)
        {
            Transform trf = Origo.GetTransform();
            trf = trf.Inverse;
            return (from FamilyInstance fi in listToOrder
                    orderby StartPoint(fi, trf).Y.FtToMeters(), StartPoint(fi, trf).X.FtToMeters()
                    select fi).ToList();
        }

        /// <summary>
        /// Returns the start point of the LocationCurve transformed to the reference coordinates.
        /// </summary>
        /// <param name="fi">LocationCurve based FamilyInstance.</param>
        /// <param name="trf">Inverse Transform of the reference coordinates.</param>
        private static XYZ StartPoint(FamilyInstance fi, Transform trf)
        {
            if (fi == null) return null;
            LocationCurve loc = fi.Location as LocationCurve;
            return trf.OfPoint(loc.Curve.GetEndPoint(0));
        }

        /// <summary>
        /// Returns the start point of the CurveElement transformed to the reference coordinates.
        /// </summary>
        /// <param name="fi">CurveElement such as detail line.</param>
        /// <param name="trf">Inverse Transform of the reference coordinates.</param>
        private static XYZ StartPoint(CurveElement fi, Transform trf)
        {
            return trf.OfPoint(fi.GeometryCurve.GetEndPoint(0));
        }

        /// <summary>
        /// Returns the end point of the LocationCurve transformed to the reference coordinates.
        /// </summary>
        /// <param name="fi">LocationCurve based FamilyInstance.</param>
        /// <param name="trf">Inverse Transform of the reference coordinates.</param>
        private static XYZ EndPoint(FamilyInstance fi, Transform trf)
        {
            LocationCurve loc = fi.Location as LocationCurve;
            return trf.OfPoint(loc.Curve.GetEndPoint(1));
        }

        /// <summary>
        /// Returns the end point of the CurveElement transformed to the reference coordinates.
        /// </summary>
        /// <param name="fi">CurveElement such as detail line.</param>
        /// <param name="trf">Inverse Transform of the reference coordinates.</param>
        private static XYZ EndPoint(CurveElement fi, Transform trf)
        {
            return trf.OfPoint(fi.GeometryCurve.GetEndPoint(1));
        }

        private static FamilyInstance GetOrigo(Document doc)
        {
            FilteredElementCollector colOrigo = new FilteredElementCollector(doc);
            return colOrigo
                .WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter("GS_Origo: Origin",
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM)).Cast<FamilyInstance>()
                .FirstOrDefault();
        }

        private static HashSet<FamilyInstance> GetWallSymbolsUnordered(string familyName, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            return collector.WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter(familyName,
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM))
                .Cast<FamilyInstance>()
                .ToHashSet();
        }
    }

    public class WallData
    {
        public HashSet<FamilyInstance> WallSymbols { get; }
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
                .ToHashSet();

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
                Length.Add(loc.Curve.Length.FtToMeters());

                //Get the end and start points
                Curve locCurve = loc.Curve;
                XYZ start = locCurve.GetEndPoint(0);
                XYZ end = locCurve.GetEndPoint(1);

                //Transform the points
                XYZ tStart = trf.OfPoint(start);
                XYZ tEnd = trf.OfPoint(end);

                double sX = tStart.X.FtToMeters(), sY = tStart.Y.FtToMeters(), eX = tEnd.X.FtToMeters(), eY = tEnd.Y.FtToMeters();

                //sb.Append("Wall ("+ sX +","+ sY + ") <> ");
                //sb.Append("(" + eX + "," + eY + ") <> ");
                //sb.Append(loc.Curve.Length.FtToMeters());
                //sb.AppendLine();

                //Take advantage of the fact that X or Y is equal
                if (sX.Equals(eX)) X.Add(sX);
                else if (sY.Equals(eY)) Y.Add(sY);
                else throw new Exception("No equal coordinates found!!!\n");

                //Collect the width of the wall from the wall symbol
                Thickness.Add(fi.LookupParameter("GS_Width").AsDouble().FtToMillimeters());
            }

            //op.WriteDebugFile(_debugFilePath, sb);
        }
    }

    public class BoundaryData
    {
        public HashSet<CurveElement> BoundaryLines { get; }
        public HashSet<XYZ> Vertices { get; } = new HashSet<XYZ>();

        public BoundaryData(string lineName, Document doc)
        {
            BoundaryLines = (from CurveElement cu in fi.GetElements<CurveElement>(doc)
                             where cu.LineStyle.Name == lineName
                             select cu).ToHashSet();

            #region Collect Vertices

            //Get the end points of boundary lines, but discard duplicates
            foreach (CurveElement cu in BoundaryLines)
            {
                Curve curve = cu.GeometryCurve;
                //Start point
                XYZ p1 = curve.GetEndPoint(0);
                if (!Vertices.Any(p => p.IsEqual(p1))) Vertices.Add(p1);
                //End point
                XYZ p2 = curve.GetEndPoint(1);
                if (!Vertices.Any(p => p.IsEqual(p2))) Vertices.Add(p2);
            }

            //I think this statement sorts the points CCW
            //http://stackoverflow.com/questions/22435397/sort-2d-points-in-a-list-clockwise
            //http://stackoverflow.com/questions/6996942/c-sharp-sort-list-of-x-y-coordinates-clockwise?rq=1

            double cX = 0, cY = 0;

            //Adds all x and y coords
            cX = Vertices.Aggregate(cX, (current, pt) => current + pt.X) / Vertices.Count;
            cY = Vertices.Aggregate(cY, (current, pt) => current + pt.Y) / Vertices.Count;
            //Define centre point
            XYZ cp = new XYZ(cX, cY, 0);
            //Sorts the points -> Works only for convex hulls!
            Vertices = Vertices.OrderByDescending(pt => Math.Atan2(pt.X - cp.X, pt.Y - cp.Y)).ToHashSet();

            #endregion

        }
    }

    public class LoadData
    {
        public IList<FilledRegion> LoadAreas { get; }
        public ViewPlan GS_View { get; }

        public LoadData(Document doc)
        {
            GS_View = fi.GetViewByName<ViewPlan>("GeneralStability", doc); //<-- this is a "magic" string. TODO: Find a better way to specify the view, maybe by using the current view.

            LoadAreas = fi.GetElements<FilledRegion>(doc, GS_View.Id).ToList();
        }

    }

}
