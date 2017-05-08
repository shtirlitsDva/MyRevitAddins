using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Linq;
using MoreLinq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Plumbing;

namespace Shared
{
    public class Filter
    {
        public static ElementParameterFilter ParameterValueFilter(string valueQualifier, BuiltInParameter parameterName)
        {
            BuiltInParameter testParam = parameterName;
            ParameterValueProvider pvp = new ParameterValueProvider(new ElementId((int)testParam));
            FilterStringRuleEvaluator str = new FilterStringContains();
            FilterStringRule paramFr = new FilterStringRule(pvp, str, valueQualifier, false);
            ElementParameterFilter epf = new ElementParameterFilter(paramFr);
            return epf;
        }

        public static LogicalOrFilter FamSymbolsAndPipeTypes()
        {
            BuiltInCategory[] bics = new BuiltInCategory[]
            {
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeFitting,
            };

            IList<ElementFilter> a = new List<ElementFilter>(bics.Count());

            foreach (BuiltInCategory bic in bics) a.Add(new ElementCategoryFilter(bic));

            LogicalOrFilter categoryFilter = new LogicalOrFilter(a);

            LogicalAndFilter familySymbolFilter = new LogicalAndFilter(categoryFilter,
                new ElementClassFilter(typeof(FamilySymbol)));

            IList<ElementFilter> b = new List<ElementFilter>
            {
                new ElementClassFilter(typeof(PipeType)),

                familySymbolFilter
            };
            LogicalOrFilter classFilter = new LogicalOrFilter(b);

            return classFilter;
        }

        public static LogicalOrFilter FamInstOfDetailComp()
        {
            BuiltInCategory[] bics = new BuiltInCategory[]
            {
                BuiltInCategory.OST_DetailComponents,
            };

            IList<ElementFilter> a = new List<ElementFilter>(bics.Count());

            foreach (BuiltInCategory bic in bics) a.Add(new ElementCategoryFilter(bic));

            LogicalOrFilter categoryFilter = new LogicalOrFilter(a);

            LogicalAndFilter familySymbolFilter = new LogicalAndFilter(categoryFilter,
                new ElementClassFilter(typeof(FamilyInstance)));

            IList<ElementFilter> b = new List<ElementFilter>
            {
                familySymbolFilter
            };
            LogicalOrFilter classFilter = new LogicalOrFilter(b);

            return classFilter;
        }

        /// <summary>
        /// Get the collection of elements of the specified type.
        /// <para>The specified type must derive from Element, or you can use Element but you get everything :)</para>
        /// </summary>
        /// <typeparam name="T">The type of element to get</typeparam>
        /// <returns>The list of elements of the specified type</returns>
        public static HashSet<T> GetElements<T>(Document document) where T : Element
        {
            return new FilteredElementCollector(document).OfClass(typeof(T)).Cast<T>().ToHashSet();
        }

        /// <summary>
        /// Get the collection of elements of the specified type in a specified view.
        /// <para>The specified type must derive from Element, or you can use Element but you get everything :)</para>
        /// </summary>
        /// <typeparam name="T">The type of element to get</typeparam>
        /// <param name="document">Standard Document</param>
        /// <param name="id">The Element Id of the view to query</param>
        /// <returns>The list of elements of the specified type</returns>
        public static IEnumerable<T> GetElements<T>(Document document, ElementId id) where T : Element
        {
            return new FilteredElementCollector(document, id).OfClass(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Return a view, specify the type of view and name.
        /// </summary>
        /// <typeparam name="T">Type of view needed.</typeparam>
        /// <param name="name">The name of view needed.</param>
        /// <param name="doc">Standard Document.</param>
        /// <returns></returns>
        public static T GetViewByName<T>(string name, Document doc) where T : Element
        {
            return
                (from v in GetElements<View>(doc) where v != null && !v.IsTemplate && v.Name == name select v as T)
                    .FirstOrDefault();
        }
    }

    public static class Output
    {
        public static void WriteDebugFile<T>(string filePath, T whatToWrite)
        {
            // Clear the output file
            System.IO.File.WriteAllBytes(filePath, new byte[0]);

            using (StreamWriter w = File.AppendText(filePath))
            {
                w.Write(whatToWrite);
                w.Close();
            }
        }
    }

    public class Conversion
    {
        const double _inch_to_mm = 25.4;
        const double _foot_to_mm = 12 * _inch_to_mm;
        const double _foot_to_inch = 12;

        /// <summary>
        /// Return a string for a real number formatted to two decimal places.
        /// </summary>
        public static string RealString(double a)
        {
            //return a.ToString("0.##");
            return (Math.Truncate(a * 100) / 100).ToString("0.00", CultureInfo.GetCultureInfo("en-GB"));
        }

        /// <summary>
        /// Return a string for an XYZ point or vector with its coordinates converted from feet to millimetres and formatted to two decimal places.
        /// </summary>
        public static string PointStringMm(XYZ p)
        {
            return string.Format("{0:0.00} {1:0.00} {2:0.00}",
                RealString(p.X * _foot_to_mm),
                RealString(p.Y * _foot_to_mm),
                RealString(p.Z * _foot_to_mm));
        }

        public static string PointStringInch(XYZ p)
        {
            return string.Format("{0:0.00} {1:0.00} {2:0.00}",
                RealString(p.X * _foot_to_inch),
                RealString(p.Y * _foot_to_inch),
                RealString(p.Z * _foot_to_inch));
        }

        public static string PipeSizeToMm(double l)
        {
            return string.Format("{0}", Math.Round(l * 2 * _foot_to_mm));
        }

        public static string PipeSizeToInch(double l)
        {
            return string.Format("{0}", RealString(l * 2 * _foot_to_inch));
        }

        public static string AngleToPCF(double l)
        {
            return string.Format("{0}", l);
        }

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }

    public static class MyExtensions
    {
        /// <summary>
        /// Returns the value converted to meters.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double FtToMeters(this Double number)
        {
            return Util.FootToMeter(number);
        }

        public static double FtToMillimeters(this Double number)
        {
            return Util.FootToMm(number);
        }

        public static double MmToFeet(this Double number)
        {
            return Util.MmToFoot(number);
        }

        public static double SqrFeetToSqrMeters(this Double number)
        {
            return Util.SqrFootToSqrMeter(number);
        }

        public static double Round4(this Double number)
        {
            return Math.Round(number, 4, MidpointRounding.AwayFromZero);
        }

        public static double Round3(this Double number)
        {
            return Math.Round(number, 3, MidpointRounding.AwayFromZero);
        }

        public static bool IsEqual(this XYZ p, XYZ q)
        {
            return 0 == Util.Compare(p, q);
        }
    }

    public static class Transformation
    {
        #region Convex Hull

        /// <summary>
        /// Return the convex hull of a list of points 
        /// using the Jarvis march or Gift wrapping:
        /// https://en.wikipedia.org/wiki/Gift_wrapping_algorithm
        /// Written by Maxence.
        /// </summary>
        public static List<XYZ> ConvexHull(List<XYZ> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            XYZ startPoint = points.MinBy(p => p.X);
            var convexHullPoints = new List<XYZ>();
            XYZ walkingPoint = startPoint;
            XYZ refVector = XYZ.BasisY.Negate();
            do
            {
                convexHullPoints.Add(walkingPoint);
                XYZ wp = walkingPoint;
                XYZ rv = refVector;
                walkingPoint = points.MinBy(p =>
                {
                    double angle = (p - wp).AngleOnPlaneTo(rv, XYZ.BasisZ);
                    if (angle < 1e-10) angle = 2 * Math.PI;
                    return angle;
                });
                refVector = wp - walkingPoint;
            } while (walkingPoint != startPoint);
            convexHullPoints.Reverse();
            return convexHullPoints;
        }

        #endregion //Convex Hull

        public static void RotateElementInPosition(XYZ placementPoint, Connector conOnFamilyToConnect, Connector start, Element element)
        {
            #region Geometric manipulation

            //http://thebuildingcoder.typepad.com/blog/2012/05/create-a-pipe-cap.html

            //Select the OTHER connector
            MEPCurve hostPipe = start.Owner as MEPCurve;

            Connector end = (from Connector c in hostPipe.ConnectorManager.Connectors //End of the host/dummy pipe
                             where c.Id != start.Id && (int)c.ConnectorType == 1
                             select c).FirstOrDefault();

            XYZ dir = (start.Origin - end.Origin);

            // rotate the cap if necessary
            // rotate about Z first

            XYZ pipeHorizontalDirection = new XYZ(dir.X, dir.Y, 0.0).Normalize();
            //XYZ pipeHorizontalDirection = new XYZ(dir.X, dir.Y, 0.0);

            XYZ connectorDirection = -conOnFamilyToConnect.CoordinateSystem.BasisZ;

            double zRotationAngle = pipeHorizontalDirection.AngleTo(connectorDirection);

            Transform trf = Transform.CreateRotationAtPoint(XYZ.BasisZ, zRotationAngle, placementPoint);

            XYZ testRotation = trf.OfVector(connectorDirection).Normalize();

            if (Math.Abs(testRotation.DotProduct(pipeHorizontalDirection) - 1) > 0.00001)
                zRotationAngle = -zRotationAngle;

            Line axis = Line.CreateBound(placementPoint, placementPoint + XYZ.BasisZ);

            ElementTransformUtils.RotateElement(element.Document, element.Id, axis, zRotationAngle);

            //Parameter comments = element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            //comments.Set("Horizontal only");

            // Need to rotate vertically?

            if (Math.Abs(dir.DotProduct(XYZ.BasisZ)) > 0.000001)
            {
                // if pipe is straight up and down, 
                // kludge it my way else

                if (dir.X.Round3() == 0 && dir.Y.Round3() == 0 && dir.Z.Round3() != 0)
                {
                    XYZ yaxis = new XYZ(0.0, 1.0, 0.0);
                    //XYZ yaxis = dir.CrossProduct(connectorDirection);

                    double rotationAngle = dir.AngleTo(yaxis);
                    //double rotationAngle = 90;

                    if (dir.Z.Equals(1)) rotationAngle = -rotationAngle;

                    axis = Line.CreateBound(placementPoint,
                        new XYZ(placementPoint.X, placementPoint.Y + 5, placementPoint.Z));

                    ElementTransformUtils.RotateElement(element.Document, element.Id, axis, rotationAngle);

                    //comments.Set("Vertical!");
                }
                else
                {
                    #region sloped pipes

                    double rotationAngle = dir.AngleTo(pipeHorizontalDirection);

                    XYZ normal = pipeHorizontalDirection.CrossProduct(XYZ.BasisZ);

                    trf = Transform.CreateRotationAtPoint(normal, rotationAngle, placementPoint);

                    testRotation = trf.OfVector(dir).Normalize();

                    if (Math.Abs(testRotation.DotProduct(pipeHorizontalDirection) - 1) < 0.00001)
                        rotationAngle = -rotationAngle;

                    axis = Line.CreateBound(placementPoint, placementPoint + normal);

                    ElementTransformUtils.RotateElement(element.Document, element.Id, axis, rotationAngle);

                    //comments.Set("Sloped");

                    #endregion
                }
            }

            #endregion
        }

    }



    public static class MyMepUtils
    {
        public static FilteredElementCollector GetElementsWithConnectors(Document doc)
        {
            // what categories of family instances
            // are we interested in?
            // From here: http://thebuildingcoder.typepad.com/blog/2010/06/retrieve-mep-elements-and-connectors.html

            BuiltInCategory[] bics = new BuiltInCategory[]
            {
                //BuiltInCategory.OST_CableTray,
                //BuiltInCategory.OST_CableTrayFitting,
                //BuiltInCategory.OST_Conduit,
                //BuiltInCategory.OST_ConduitFitting,
                //BuiltInCategory.OST_DuctCurves,
                //BuiltInCategory.OST_DuctFitting,
                //BuiltInCategory.OST_DuctTerminal,
                //BuiltInCategory.OST_ElectricalEquipment,
                //BuiltInCategory.OST_ElectricalFixtures,
                //BuiltInCategory.OST_LightingDevices,
                //BuiltInCategory.OST_LightingFixtures,
                //BuiltInCategory.OST_MechanicalEquipment,
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeFitting,
                //BuiltInCategory.OST_PlumbingFixtures,
                //BuiltInCategory.OST_SpecialityEquipment,
                //BuiltInCategory.OST_Sprinklers,
                //BuiltInCategory.OST_Wire
            };

            IList<ElementFilter> a = new List<ElementFilter>(bics.Count());

            foreach (BuiltInCategory bic in bics) a.Add(new ElementCategoryFilter(bic));

            LogicalOrFilter categoryFilter = new LogicalOrFilter(a);

            LogicalAndFilter familyInstanceFilter = new LogicalAndFilter(categoryFilter, new ElementClassFilter(typeof(FamilyInstance)));

            //IList<ElementFilter> b = new List<ElementFilter>(6);
            IList<ElementFilter> b = new List<ElementFilter>
            {

                //b.Add(new ElementClassFilter(typeof(CableTray)));
                //b.Add(new ElementClassFilter(typeof(Conduit)));
                //b.Add(new ElementClassFilter(typeof(Duct)));
                new ElementClassFilter(typeof(Pipe)),

                familyInstanceFilter
            };
            LogicalOrFilter classFilter = new LogicalOrFilter(b);

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            collector.WherePasses(classFilter);

            return collector;
        }

        /// <summary>
        /// Returns the collection of all instances of the specified BuiltInCategory. Warning: does not work with Pipes!
        /// </summary>
        /// <param name="doc">The active document.</param>
        /// <param name="cat">The specified category. Ex: BuiltInCategory.OST_PipeFitting</param>
        /// <returns>A collection of all instances of the specified category.</returns>
        public static FilteredElementCollector GetElementsOfBuiltInCategory(Document doc, BuiltInCategory cat)
        {
            // what categories of family instances
            // are we interested in?
            // From here: http://thebuildingcoder.typepad.com/blog/2010/06/retrieve-mep-elements-and-connectors.html

            ElementFilter categoryFilter = new ElementCategoryFilter(cat);
            LogicalAndFilter familyInstanceFilter = new LogicalAndFilter(categoryFilter, new ElementClassFilter(typeof(FamilyInstance)));
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.WherePasses(familyInstanceFilter);
            return collector;
        }

        public static ConnectorSet GetConnectorSet(Element e)
        {
            ConnectorSet connectors = null;

            if (e is FamilyInstance)
            {
                MEPModel m = ((FamilyInstance)e).MEPModel;
                if (null != m && null != m.ConnectorManager) connectors = m.ConnectorManager.Connectors;
            }

            else if (e is Wire) connectors = ((Wire)e).ConnectorManager.Connectors;

            else
            {
                Debug.Assert(e.GetType().IsSubclassOf(typeof(MEPCurve)),
                    "expected all candidate connector provider "
                    + "elements to be either family instances or "
                    + "derived from MEPCurve");

                if (e is MEPCurve) connectors = ((MEPCurve)e).ConnectorManager.Connectors;
            }
            return connectors;
        }

        public static HashSet<Connector> GetALLConnectorsFromElements(HashSet<Element> elements)
        {
            return (from e in elements from Connector c in GetConnectorSet(e) select c).ToHashSet();
        }

        public static HashSet<Connector> GetALLConnectorsFromElements(Element element)
        {
            return (from Connector c in GetConnectorSet(element) select c).ToHashSet();
        }

        public static HashSet<Connector> GetALLConnectorsInDocument(Document doc)
        {
            return (from e in GetElementsWithConnectors(doc) from Connector c in GetConnectorSet(e) select c).ToHashSet();
        }
    }

    public class DataHandler
    {
        //DataSet import is from here:
        //http://stackoverflow.com/a/18006593/6073998
        public static DataSet ImportExcelToDataSet(string fileName, string dataHasHeaders)
        {
            //On connection strings http://www.connectionstrings.com/excel/#p84
            string connectionString =
                string.Format(
                    "provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR={1};IMEX=1\"",
                    fileName, dataHasHeaders);

            DataSet data = new DataSet();

            foreach (string sheetName in GetExcelSheetNames(connectionString))
            {
                using (OleDbConnection con = new OleDbConnection(connectionString))
                {
                    var dataTable = new DataTable();
                    string query = string.Format("SELECT * FROM [{0}]", sheetName);
                    con.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, con);
                    adapter.Fill(dataTable);

                    //Remove ' and $ from sheetName
                    Regex rgx = new Regex("[^a-zA-Z0-9 _-]");
                    string tableName = rgx.Replace(sheetName, "");

                    dataTable.TableName = tableName;
                    data.Tables.Add(dataTable);
                }
            }

            if (data == null) Util.ErrorMsg("Data set is null");
            if (data.Tables.Count < 1) Util.ErrorMsg("Table count in DataSet is 0");

            return data;
        }

        static string[] GetExcelSheetNames(string connectionString)
        {
            OleDbConnection con = null;
            DataTable dt = null;
            con = new OleDbConnection(connectionString);
            con.Open();
            dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

            if (dt == null)
            {
                return null;
            }

            string[] excelSheetNames = new string[dt.Rows.Count];
            int i = 0;

            foreach (DataRow row in dt.Rows)
            {
                excelSheetNames[i] = row["TABLE_NAME"].ToString();
                i++;
            }

            return excelSheetNames;
        }
    }

}
