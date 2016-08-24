using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Shared
{
    public class Utils
    {
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
            System.IO.File.WriteAllBytes(filePath, new byte[0]);

            //// Write to output file
            using (StreamWriter w = File.AppendText(filePath))
            {
                w.Write(whatToWrite);
                w.Close();
            }
        }
    }
}
