using System;
using System.Collections;
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

        public Result CalculateLoads(Document doc)
        {
            //Build a direct shape with TessellatedShapeBuilder

            try
            {
                //Get the transform
                Transform trf = Origo.GetTransform();
                trf = trf.Inverse;

                //Get the list of boundaries (to simplify the synthax)
                IList<CurveElement> Bd = BoundaryData.BoundaryLines;

                //The analysis proceeds in steps of 1mm
                double _1mm = 0.001.ToFeet();

                //Determine the largest X value
                double Xmax = Bd.Max(x => EndPoint(x, trf).X);
                //Divide the largest X value by the step value to determine the number iterations
                int nrOfIterations = (int)Math.Floor(Xmax / _1mm);

                //Current X-step


                //Iterate through the length of the building analyzing the load
                for (int i = 0; i < nrOfIterations; i++)
                {
                    //Decided to store the load in LoadArea comments.
                }

                #region BySplittingFaces (does not work)

                //TessellatedShapeBuilder builder = new TessellatedShapeBuilder();
                ////http://thebuildingcoder.typepad.com/blog/2014/05/directshape-performance-and-minimum-size.html
                //builder.OpenConnectedFaceSet(false);
                //builder.AddFace(new TessellatedFace(BoundaryData.Vertices, ElementId.InvalidElementId));
                //builder.CloseConnectedFaceSet();
                //builder.Build();
                //TessellatedShapeBuilderResult result = builder.GetBuildResult();
                //IList<GeometryObject> resultList = result.GetGeometricalObjects();
                //var solidShape = resultList[0] as Solid;
                //Face face = solidShape.Faces.get_Item(0);

                //face.


                //DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
                //ds.ApplicationId = "Application id";
                //ds.ApplicationDataId = "Geometry object id";
                //ds.Name = "Whole area of analysis";
                //DirectShapeOptions dso = ds.GetOptions();
                //dso.ReferencingOption = DirectShapeReferencingOption.Referenceable;
                //ds.SetOptions(dso);
                //ds.SetShape(resultList);
                //doc.Regenerate();

                #endregion

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw new Exception(e.Message);
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
                IList<FamilyInstance> along = GetWallSymbolsUnordered(name, doc);
                IList<FamilyInstance> wallsAlongSorted = OrderGeometrically(along, Origo);

                int idx = 0;
                foreach (FamilyInstance fi in wallsAlongSorted)
                {
                    idx++;
                    fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(idx.ToString());
                }

                name = "GS_Stabilizing_Wall: Stabilizing Wall - Cross";
                IList<FamilyInstance> cross = GetWallSymbolsUnordered(name, doc);
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
        private static IList<FamilyInstance> OrderGeometrically(IList<FamilyInstance> listToOrder, FamilyInstance Origo)
        {
            Transform trf = Origo.GetTransform();
            trf = trf.Inverse;
            return (from FamilyInstance fi in listToOrder
                    orderby StartPoint(fi, trf).Y.ToMeters(), StartPoint(fi, trf).X.ToMeters()
                    select fi).ToList();
        }

        /// <summary>
        /// Returns the start point of the LocationCurve transformed to the reference coordinates.
        /// </summary>
        /// <param name="fi">LocationCurve based FamilyInstance.</param>
        /// <param name="trf">Inverse Transform of the reference coordinates.</param>
        private static XYZ StartPoint(FamilyInstance fi, Transform trf)
        {
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

        private static IList<FamilyInstance> GetWallSymbolsUnordered(string familyName, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            return collector.WherePasses(fi.FamInstOfDetailComp())
                .WherePasses(fi.ParameterValueFilter(familyName,
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM))
                .Cast<FamilyInstance>()
                .ToList();
        }
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

    public class BoundaryData
    {
        public IList<CurveElement> BoundaryLines { get; }
        public IList<XYZ> Vertices { get; } = new List<XYZ>();

        public BoundaryData(string lineName, Document doc)
        {
            BoundaryLines = (from CurveElement cu in fi.GetElements<CurveElement>(doc)
                             where cu.LineStyle.Name == lineName
                             select cu).ToList();

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
            Vertices = Vertices.OrderByDescending(pt => Math.Atan2(pt.X - cp.X, pt.Y - cp.Y)).ToList();

            #endregion

        }
    }

    public class LoadData
    {
        public IList<FilledRegion> LoadAreas { get; }

        public LoadData(Document doc)
        {
            ViewPlan v = fi.GetViewByName<ViewPlan>("GeneralStability", doc); //<-- this is a "magic" string. TODO: Find a better way to specify the view, maybe by using the current view.
            
            LoadAreas = new FilteredElementCollector(doc, v.Id).OfClass(typeof(FilledRegion)).Cast<FilledRegion>().ToList();
        }

    }
}
