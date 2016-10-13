using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using MoreLinq;

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

            IList<ElementFilter> b = new List<ElementFilter>();

            b.Add(familySymbolFilter);

            LogicalOrFilter classFilter = new LogicalOrFilter(b);

            return classFilter;
        }

        /// <summary>
        /// Get the collection of elements of the specified type.
        /// <para>The specified type must derive from Element, or you can use Element but you get everything :)</para>
        /// </summary>
        /// <typeparam name="T">The type of element to get</typeparam>
        /// <returns>The list of elements of the specified type</returns>
        public static IEnumerable<T> GetElements<T>(Document document) where T : Element
        {
            return new FilteredElementCollector(document).OfClass(typeof(T)).Cast<T>();
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
            return (from v in GetElements<View>(doc) where v != null && !v.IsTemplate && v.Name == name select v as T).FirstOrDefault();
        }
    }

    public class Output
    {
        public static void WriteDebugFile(string filePath, StringBuilder whatToWrite)
        {
            //// Clear the output file
            //System.IO.File.WriteAllBytes(filePath, new byte[0]);

            //// Write to output file
            using (StreamWriter w = File.AppendText(filePath))
            {
                w.Write(whatToWrite);
                w.Close();
            }
        }

        public static void WriteDebugFile(string filePath, string whatToWrite)
        {
            //// Clear the output file
            //System.IO.File.WriteAllBytes(filePath, new byte[0]);

            //// Write to output file
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
        #endregion // Convex Hull
    }
}
