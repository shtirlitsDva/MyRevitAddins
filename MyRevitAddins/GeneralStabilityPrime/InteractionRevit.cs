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
using ClipperLib;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mySettings = GeneralStability.Properties.Settings;
using TxBox = System.Windows.Forms.TextBox;
using ir = GeneralStability.InteractionRevit;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace GeneralStability
{
    public class InteractionRevit
    {
        public FamilyInstance Origo { get; } //Holds the Origo family instance
        public WallData WallsAlong { get; }
        public WallData WallsCross { get; }
        public WallData BearingBeams { get; }
        public BoundaryData BoundaryData { get; }
        public LoadData LoadData { get; }


        public InteractionRevit(Document doc)
        {
            //Get the Origo component
            Origo = GetOrigo(doc);

            //Gather the detail components
            WallsAlong = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Along", Origo, doc);
            WallsCross = new WallData("GS_Stabilizing_Wall: Stabilizing Wall - Across", Origo, doc);
            BearingBeams = new WallData("GS_Stabilizing_Wall: Bearing Beam - Along", Origo, doc);

            //Initialize boundary data
            BoundaryData = new BoundaryData("GS_Boundary", doc);

            //Initialize load data
            LoadData = new LoadData(doc);
        }

        #region LoadCalculation

        public Result CalculateLoads(Document doc, ref int totalLoops, ref StringBuilder debug)
        {
            try
            {
                //Log
                int nrTotal = 0;

                //Get the transform
                Transform trfO = Origo.GetTransform();
                Transform trf = trfO.Inverse;

                //Get the list of boundaries and walls (to simplify the synthax)
                HashSet<CurveElement> Bd = BoundaryData.BoundaryLines;
                HashSet<FamilyInstance> Walls = WallsAlong.WallSymbols;
                HashSet<FamilyInstance> Beams = BearingBeams.WallSymbols;
                HashSet<Element> BdAndWalls = new HashSet<Element>();
                BdAndWalls.UnionWith(Bd);
                BdAndWalls.UnionWith(Walls);
                BdAndWalls.UnionWith(Beams);

                //I know it's messy, but beams need to be in walls too, apparently
                Walls.UnionWith(Beams);

                HashSet<LoadArea> LoadAreas = LoadData.LoadAreas;

                //Roof load intensity
                double roofLoadIntensity = double.Parse(mySettings.Default.roofLoadIntensity, CultureInfo.InvariantCulture);

                //Create a list of ALL X Points of Interest ie. Start and End points
                IList<XYZ> allPoiX = new List<XYZ>();
                foreach (Element el in BdAndWalls)
                {
                    allPoiX.Add(StartPoint(el, trf));
                    allPoiX.Add(EndPoint(el, trf));
                }
                //Clean list of duplicates and sort by value of X
                allPoiX = allPoiX.DistinctBy(pt => pt.X.FtToMillimeters()).OrderBy(pt => pt.X.FtToMillimeters()).ToList();

                foreach (FamilyInstance fi in Walls)
                {
                    //Debug
                    debug.Append("\n" + fi.Id + "\n");

                    //Initialize variables
                    double load = 0;
                    double totalArea = 0;

                    //Determine the start X
                    double Xmin = StartPoint(fi, trf).X;

                    //Determine the end X
                    double Xmax = EndPoint(fi, trf).X;

                    //The y of the wall
                    double Ycur = StartPoint(fi, trf).Y;

                    //Længde af væggen
                    double length = (Xmax - Xmin).FtToMeters();

                    //TODO: New implementation -- start here

                    //Determine the POIX in scope of the wall (POIX = Point Of Interest on X axis)
                    var poixInScope = (from XYZ pt in allPoiX
                                       where pt.X.FtToMillimeters() >= Xmin.FtToMillimeters() &&
                                             pt.X.FtToMillimeters() <= Xmax.FtToMillimeters()
                                       select pt).ToList();

                    //Iterate through the load areas
                    for (int i = 0; i < poixInScope.Count - 1; i++) //<- -1 because theres 1 less areas than points
                    {
                        //Determine the X value in the middle of the span to be able to get relevant walls
                        double x1 = poixInScope[i].X;
                        double x2 = poixInScope[i + 1].X;
                        double xC = (x1 + (x2 - x1) / 2).FtToMillimeters();

                        //Determine relevant walls (that means the walls which are crossed by the current Xcentre value iteration)
                        var wallsX = (from FamilyInstance fin in Walls
                                      where StartPoint(fin, trf).X.FtToMillimeters() <= xC &&
                                            EndPoint(fin, trf).X.FtToMillimeters() >= xC
                                      select fin).OrderBy(x => StartPoint(x, trf).Y); //<- Not using Descending because the list is defined from up to down

                        //Determine relevant boundaries (that means the boundaries which are crossed by the current Xcentre value iteration)
                        var boundaryX = (from CurveElement cue in Bd
                                         where StartPoint(cue, trf).X.FtToMillimeters() <= xC &&
                                               EndPoint(cue, trf).X.FtToMillimeters() >= xC
                                         select cue).ToHashSet();

                        //First handle the walls
                        //Create a linked list to be able select previous and next elements in sequence
                        var wallsXlinked = new LinkedList<FamilyInstance>(wallsX);
                        var node = wallsXlinked.Find(fi);
                        var wallPositive = node?.Next?.Value;
                        var wallNegative = node?.Previous?.Value;

                        //Select boundaries if no walls found at location
                        CurveElement bdPositive = null, bdNegative = null;
                        if (wallPositive == null) bdPositive = boundaryX.MaxBy(x => StartPoint(x, trf).Y);
                        if (wallNegative == null) bdNegative = boundaryX.MinBy(x => StartPoint(x, trf).Y);

                        //Flow control
                        bool isEdgePositive = false, isEdgeNegative = false; //<-- Indicates if the wall is on the boundary

                        //Detect edge cases
                        if (wallPositive == null && StartPoint(bdPositive, trf).Y.FtToMillimeters().Equals(Ycur.FtToMillimeters())) isEdgePositive = true;
                        if (wallNegative == null && StartPoint(bdNegative, trf).Y.FtToMillimeters().Equals(Ycur.FtToMillimeters())) isEdgeNegative = true;

                        //Prepare for roof load: if edge case detected select both boundaries
                        if (isEdgePositive || isEdgeNegative)
                        {
                            if (bdPositive == null) bdPositive = boundaryX.MaxBy(x => StartPoint(x, trf).Y);
                            if (bdNegative == null) bdNegative = boundaryX.MinBy(x => StartPoint(x, trf).Y);

                            double widthRoofLoad = (StartPoint(bdPositive, trf).Y - StartPoint(bdNegative, trf).Y) / 2;
                            double roofLoadArea = (widthRoofLoad * (x2 - x1)).SqrFeetToSqrMeters();
                            double roofLoad = roofLoadIntensity * roofLoadArea;
                            load = load + roofLoad; //Write to the overall load variable
                        }

                        //Process the positive and negative side
                        //Declare Y values
                        double yP, yN;

                        //Declare combining list for vertices
                        List<XYZ> vertices = new List<XYZ>();
                        //Add points along the wall if one is edge case
                        if (isEdgePositive || isEdgeNegative)
                        {
                            vertices.Add(NormPoint(x1.Round4(), Ycur.Round4(), trfO, LoadData.GS_View));
                            vertices.Add(NormPoint(x2.Round4(), Ycur.Round4(), trfO, LoadData.GS_View));
                        }


                        #region Positive side

                        if (!isEdgePositive)
                        {
                            //Calculate Y values
                            if (wallPositive != null) yP = Ycur + (StartPoint(wallPositive, trf).Y - Ycur) / 2;
                            else yP = Ycur + (StartPoint(bdPositive, trf).Y - Ycur) / 2;

                            //Create points from the X and Y values
                            XYZ PxP1 = NormPoint(x1.Round4(), yP.Round4(), trfO, LoadData.GS_View);
                            XYZ PxP2 = NormPoint(x2.Round4(), yP.Round4(), trfO, LoadData.GS_View);

                            //Create a list of vertices to feed the solid builder
                            vertices.Add(PxP1);
                            vertices.Add(PxP2);

                            nrTotal++;
                        }

                        #endregion


                        #region Negative side

                        if (!isEdgeNegative)
                        {
                            if (wallNegative != null)
                                yN = StartPoint(wallNegative, trf).Y + (Ycur - StartPoint(wallNegative, trf).Y) / 2;
                            else yN = StartPoint(bdNegative, trf).Y + (Ycur - StartPoint(bdNegative, trf).Y) / 2;

                            //Create points from the X and Y values
                            XYZ PxN1 = NormPoint(x1.Round4(), yN.Round4(), trfO, LoadData.GS_View);
                            XYZ PxN2 = NormPoint(x2.Round4(), yN.Round4(), trfO, LoadData.GS_View);

                            //Create a list of vertices to feed the solid builder
                            vertices.Add(PxN1);
                            vertices.Add(PxN2);

                            nrTotal++;
                        }
                        #endregion

                        //Create a list of vertices
                        vertices = vertices.DistinctBy(xyz => new { X = xyz.X.Round4(), Y = xyz.Y.Round4() }).ToList();
                        vertices = tr.ConvexHull(vertices);

                        //Create a path from the Clipper framework
                        Path wallLoadPath = CreatePath(vertices);
                        //The defined precision of the Clipper objects
                        long precision = 10000;

                        //Debug
                        //debug.Append((Clipper.Area(wallLoadPath) / (precision * precision)).SqrFeetToSqrMeters() + "+\n");

                        //Iterate through the load areas and intersect them with wall load areas
                        foreach (LoadArea la in LoadAreas)
                        {
                            Paths solution = new Paths();
                            Clipper c = new Clipper();
                            c.AddPath(wallLoadPath, PolyType.ptClip, true);
                            c.AddPath(la.Path, PolyType.ptSubject, true);
                            c.Execute(ClipType.ctIntersection, solution);
                            foreach (Path path in solution)
                            {
                                double intArea = (Clipper.Area(path) / (precision * precision)).SqrFeetToSqrMeters();
                                //debug.Append(intArea + " " + la.Load + " " + la.Load * intArea + "\n");
                                //debug.Append((Clipper.Area(la.Path)/(precision*precision)).SqrFeetToSqrMeters()+"\n");
                                load += intArea * la.Load;
                                totalArea += intArea;
                            }
                        }
                    }
                    fi.LookupParameter("GS_Load").Set(load/length); //Change meee!!
                    debug.Append(length + " "+totalArea+" "+load+"\n" + load/length + "\n");
                }

                totalLoops = nrTotal;
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception(e.Message);
                //return Result.Succeeded;
            }
        }

        public Result DrawLoadAreas(Document doc)
        {
            try
            {
                //Get the transform
                Transform trfO = Origo.GetTransform();
                Transform trf = trfO.Inverse;

                //Get the list of boundaries and walls (to simplify the synthax)
                HashSet<CurveElement> Bd = BoundaryData.BoundaryLines;
                HashSet<FamilyInstance> Walls = WallsAlong.WallSymbols;

                //The analysis proceeds in steps
                double step = ((double)mySettings.Default.integerStepSize).MmToFeet();

                foreach (FamilyInstance fi in Walls)
                {
                    //Determine the start X
                    double Xmin = StartPoint(fi, trf).X;

                    //Determine the end X
                    double Xmax = EndPoint(fi, trf).X;

                    //The y of the wall
                    double Ycur = StartPoint(fi, trf).Y;

                    ////Divide the largest X value by the step value to determine the number iterations in X direction
                    int nrOfX = (int)Math.Floor((Xmax - Xmin) / step);

                    //Debug
                    double[] X, Y; X = new double[4]; Y = new double[4];

                    //Iterate through the length of the current wall analyzing the load
                    for (int i = 0; i < nrOfX; i++)
                    {
                        //Current x value
                        double x1 = Xmin + i * step;
                        double x2 = Xmin + (i + 1) * step;
                        double xC = x1 + step / 2;

                        //Debug
                        if (i == 0)
                        {
                            X[0] = x1;
                            X[2] = x1;
                        }
                        X[1] = x2;
                        X[3] = x2;

                        //Determine relevant walls (that means the walls which are crossed by the current X value iteration)
                        var wallsX = (from FamilyInstance fin in Walls
                                      where StartPoint(fin, trf).X <= xC && EndPoint(fin, trf).X >= xC
                                      select fin).OrderBy(x => StartPoint(x, trf).Y); //<- Not using Descending because the list is defined from up to down

                        //Determine relevant walls (that means the walls which are crossed by the current X value iteration)
                        var boundaryX = (from CurveElement cue in Bd
                                         where StartPoint(cue, trf).X <= xC && EndPoint(cue, trf).X >= xC
                                         select cue).ToHashSet();

                        //First handle the walls
                        var wallsXlinked = new LinkedList<FamilyInstance>(wallsX);
                        var listNode1 = wallsXlinked.Find(fi);
                        var wallPositive = listNode1?.Next?.Value;
                        var wallNegative = listNode1?.Previous?.Value;

                        //Select boundaries if no walls found at location
                        CurveElement bdPositive = null, bdNegative = null;
                        if (wallPositive == null) bdPositive = boundaryX.MaxBy(x => StartPoint(x, trf).Y);
                        if (wallNegative == null) bdNegative = boundaryX.MinBy(x => StartPoint(x, trf).Y);

                        //Flow control
                        bool isEdgePositive = false, isEdgeNegative = false; //<-- Indicates if the wall is on the boundary

                        //Detect edge cases
                        if (wallPositive == null && StartPoint(bdPositive, trf).Y.FtToMillimeters().Equals(Ycur.FtToMillimeters())) isEdgePositive = true;
                        if (wallNegative == null && StartPoint(bdNegative, trf).Y.FtToMillimeters().Equals(Ycur.FtToMillimeters())) isEdgeNegative = true;

                        //Init loop counters
                        int nrOfYPos, nrOfYNeg;

                        //Determine number of iterations in Y direction POSITIVE handling all cases
                        //The 2* multiplier on step makes sure that iteration only happens on the half of the span
                        if (wallPositive != null) nrOfYPos = (int)Math.Floor((StartPoint(wallPositive, trf).Y - Ycur) / (2 * step));
                        else if (isEdgePositive) nrOfYPos = 0;
                        else nrOfYPos = (int)Math.Floor((StartPoint(bdPositive, trf).Y - Ycur) / (2 * step));

                        //Determine number of iterations in Y direction NEGATIVE handling all cases
                        //The 2* multiplier on step makes sure that iteration only happens on the half of the span
                        if (wallNegative != null) nrOfYNeg = (int)Math.Floor((-StartPoint(wallNegative, trf).Y + Ycur) / (2 * step));
                        else if (isEdgeNegative) nrOfYNeg = 0;
                        else nrOfYNeg = (int)Math.Floor((-StartPoint(bdNegative, trf).Y + Ycur) / (2 * step));

                        //Iterate through the POSITIVE side
                        for (int j = 0; j < nrOfYPos; j++)
                        {
                            //Current y value
                            double y1 = Ycur + j * step;
                            double y2 = Ycur + (j + 1) * step;

                            //Debug
                            if (i == 0 && j == nrOfYPos - 1) Y[0] = y2;
                            if (j == nrOfYPos - 1)
                            {
                                Y[1] = y2;
                                if (!Y[0].FtToMillimeters().Equals(Y[1].FtToMillimeters()))
                                {
                                    CreateLoadAreaBoundaries(doc, X[0], X[1], Y[0], trfO);
                                    X[0] = x1; Y[0] = y2;
                                }
                                if (i == nrOfX - 1) CreateLoadAreaBoundaries(doc, X[0], X[1], Y[0], trfO);
                            }
                        }

                        //Iterate through the NEGATIVE side
                        for (int k = 0; k < nrOfYNeg; k++)
                        {
                            //Current y value
                            double y1 = Ycur - k * step;
                            double y2 = Ycur - (k + 1) * step;

                            //Debug
                            if (i == 0 && k == nrOfYNeg - 1) Y[2] = y2;
                            if (k == nrOfYNeg - 1)
                            {
                                Y[3] = y2;
                                if (!Y[2].FtToMillimeters().Equals(Y[3].FtToMillimeters()))
                                {
                                    CreateLoadAreaBoundaries(doc, X[2], X[3], Y[2], trfO);
                                    X[2] = x1; Y[2] = y2;
                                }
                                if (i == nrOfX - 1) CreateLoadAreaBoundaries(doc, X[2], X[3], Y[2], trfO);
                            }
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception(e.Message);
            }
        }

        private void CreateLoadAreaBoundaries(Document doc, double x1, double x2, double y, Transform trfO)
        {
            XYZ p1 = new XYZ(x1, y, 0);
            XYZ p1O = trfO.OfPoint(p1);
            XYZ p2 = new XYZ(x2, y, 0);
            XYZ p2O = trfO.OfPoint(p2);
            Curve line = Line.CreateBound(p1O, p2O) as Curve;
            var detailCurve = doc.Create.NewDetailCurve(LoadData.GS_View, line);
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

                name = "GS_Stabilizing_Wall: Stabilizing Wall - Across";
                HashSet<FamilyInstance> cross = GetWallSymbolsUnordered(name, doc);
                IList<FamilyInstance> wallsCrossSorted = OrderGeometrically(cross, Origo);

                idx = 0;
                foreach (FamilyInstance fi in wallsCrossSorted)
                {
                    idx++;
                    fi.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(idx.ToString());
                }

                name = "GS_Stabilizing_Wall: Bearing Beam - Along";
                HashSet<FamilyInstance> beams = GetWallSymbolsUnordered(name, doc);
                IList<FamilyInstance> beamsAlongSorted = OrderGeometrically(beams, Origo);

                idx = 0;
                foreach (FamilyInstance fi in beamsAlongSorted)
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
        /// Returns the start point of a Curve based Element.
        /// </summary>
        /// <typeparam name="T">The type of the Element in question.</typeparam>
        /// <param name="obj">The Element where to extract the start point.</param>
        /// <param name="trf">The transform of the Origo.</param>
        /// <returns>The start point of the Element's Curve as XYZ.</returns>
        private static XYZ StartPoint<T>(T obj, Transform trf) where T : Element
        {
            if (obj == null) return null;
            if (obj is FamilyInstance)
            {
                FamilyInstance fi = obj as FamilyInstance;
                LocationCurve loc = fi.Location as LocationCurve;
                return trf.OfPoint(loc.Curve.GetEndPoint(0));
            }
            if (obj is CurveElement)
            {
                CurveElement cu = obj as CurveElement;
                return trf.OfPoint(cu.GeometryCurve.GetEndPoint(0));
            }
            throw new Exception("Type not handled!");
        }

        /// <summary>
        /// Returns the end point of a Curve based Element.
        /// </summary>
        /// <typeparam name="T">The type of the Element in question.</typeparam>
        /// <param name="obj">The Element where to extract the end point.</param>
        /// <param name="trf">The transform of the Origo.</param>
        /// <returns>The end point of the Element's Curve as XYZ.</returns>
        private static XYZ EndPoint<T>(T obj, Transform trf) where T : Element
        {
            if (obj == null) return null;
            if (obj is FamilyInstance)
            {
                FamilyInstance fi = obj as FamilyInstance;
                LocationCurve loc = fi.Location as LocationCurve;
                return trf.OfPoint(loc.Curve.GetEndPoint(1));
            }
            if (obj is CurveElement)
            {
                CurveElement cu = obj as CurveElement;
                return trf.OfPoint(cu.GeometryCurve.GetEndPoint(1));
            }
            throw new Exception("Type not handled!");
        }

        /// <summary>
        /// A method to create points on the same elevation as the specified view's associated level.
        /// </summary>
        /// <param name="x">X in Origo.</param>
        /// <param name="y">Y in Origo.</param>
        /// <param name="trfO">Transform back to global coords.</param>
        /// <param name="view">The view where to create the points.</param>
        /// <returns>Returns the point at the level's elevation.</returns>
        private static XYZ NormPoint(double x, double y, Transform trfO, ViewPlan view)
        {
            XYZ temp1 = new XYZ(x, y, 0);
            XYZ temp2 = trfO.OfPoint(temp1);
            double realZ = view.GenLevel.ProjectElevation;
            return new XYZ(temp2.X.Round4(), temp2.Y.Round4(), realZ.Round4());
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

        public static Face GetFace(FilledRegion fr, Options options)
        {
            GeometryElement geometryElement = fr.get_Geometry(options);
            foreach (GeometryObject go in geometryElement)
            {
                Solid solid = go as Solid;
                return solid.Faces.get_Item(0);
            }
            return null;
        }

        private static Solid GetSolid(FilledRegion fr, Options options)
        {
            GeometryElement geometryElement = fr.get_Geometry(options);
            return geometryElement.Select(go => go as Solid).FirstOrDefault();
        }

        /// <summary>
        /// Creates a solid from a list of points. Intented to create single face solids for solid operations.
        /// </summary>
        /// <param name="vertices">A list of XYZ vertices of the face.</param>
        /// <returns>A solid consisting of one face.</returns>
        public static Solid CreateSolid(IList<XYZ> vertices)
        {
            TessellatedShapeBuilder builder = new TessellatedShapeBuilder();
            //http://thebuildingcoder.typepad.com/blog/2014/05/directshape-performance-and-minimum-size.html
            builder.OpenConnectedFaceSet(false);
            builder.AddFace(new TessellatedFace(vertices, ElementId.InvalidElementId));
            builder.CloseConnectedFaceSet();
            builder.Build();
            TessellatedShapeBuilderResult result = builder.GetBuildResult();
            return result.GetGeometricalObjects()[0] as Solid;
        }

        public static DirectShape CreateDirectShape(Document doc, IList<GeometryObject> resultList)
        {
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.ApplicationId = "Application id";
            ds.ApplicationDataId = "Geometry object id";
            ds.Name = "Load area";
            DirectShapeOptions dso = ds.GetOptions();
            dso.ReferencingOption = DirectShapeReferencingOption.Referenceable;
            ds.SetOptions(dso);
            ds.SetShape(resultList);
            doc.Regenerate();
            return ds;
        }

        public static DirectShape CreateDirectShape(Document doc, Solid solid)
        {
            IList<GeometryObject> list = new List<GeometryObject>(1);
            list.Add(solid);
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.ApplicationId = "Application id";
            ds.ApplicationDataId = "Geometry object id";
            ds.Name = "Load area";
            DirectShapeOptions dso = ds.GetOptions();
            dso.ReferencingOption = DirectShapeReferencingOption.Referenceable;
            ds.SetOptions(dso);
            ds.SetShape(list);
            doc.Regenerate();
            return ds;
        }

        public static Path CreatePath(IList<XYZ> source)
        {
            long precision = 10000;
            Path path = new Path(source.Count);
            path.AddRange(source.Select(p => new IntPoint(p.X * precision, p.Y * precision)));
            return path;
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
        public HashSet<LoadArea> LoadAreas { get; }
        public ViewPlan GS_View { get; }

        public LoadData(Document doc)
        {
            GS_View = fi.GetViewByName<ViewPlan>("GeneralStability", doc); //<-- this is a "magic" string. TODO: Find a better way to specify the view, maybe by using the current view.

            HashSet<FilledRegion> filledRegions = fi.GetElements<FilledRegion>(doc, GS_View.Id).ToHashSet();
            LoadAreas = new HashSet<LoadArea>();
            foreach (FilledRegion fr in filledRegions)
            {
                Options options = new Options();
                options.ComputeReferences = true;
                options.View = GS_View;
                LoadAreas.Add(new LoadArea(fr, options));
            }
        }
    }

    public class LoadArea
    {
        public FilledRegion FilledRegion { get; }
        public ElementId ElementId { get; }
        //public Solid Solid { get; }
        public Path Path { get; }
        public double Load { get; }

        public LoadArea(FilledRegion filledRegion, Options options)
        {
            try
            {
                FilledRegion = filledRegion;
                ElementId = filledRegion.Id;
                Load = double.Parse(filledRegion.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString(), CultureInfo.InvariantCulture);

                //Create a new solid from the element solid
                Face face = ir.GetFace(filledRegion, options);
                IList<XYZ> source = new List<XYZ>();
                foreach (CurveLoop curveLoop in face.GetEdgesAsCurveLoops())
                {
                    foreach (Curve curve in curveLoop)
                    {
                        source.Add(curve.GetEndPoint(0));
                        source.Add(curve.GetEndPoint(1));
                    }
                }
                source = source.DistinctBy(xyz => new { X = xyz.X.Round4(), Y = xyz.Y.Round4() }).ToList();
                source = tr.ConvexHull(source.ToList());
                Path = ir.CreatePath(source);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception(e.Message);
            }
        }
    }
}
