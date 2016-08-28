using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
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
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(T));
            return collector.Cast<T>();
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
}
