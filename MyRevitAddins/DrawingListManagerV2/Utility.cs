using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    public static class Extensions
    {
        private static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        public static bool IsNoE(this string str) => str.IsNullOrEmpty();
        public static bool IsNotNoE(this string str) => !str.IsNullOrEmpty();

        public static object GetPropertyValue(this object T, string PropName)
        {
            return T.GetType().GetProperty(PropName) == null ? null : T.GetType().GetProperty(PropName).GetValue(T, null);
        }

        public static Dictionary<string, object> ToPropertyDictionary(this object obj)
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var propertyInfo in obj.GetType().GetProperties())
                if (propertyInfo.CanRead && propertyInfo.CanWrite &&
                    propertyInfo.GetIndexParameters().Length == 0)
                    dictionary[propertyInfo.Name] = propertyInfo.GetValue(obj, null);
            return dictionary;
        }
    }
    public static class Output
    {
        public static void OutputWriter(StringBuilder _collect)
        {
            //Create filename
            string filename = @"C:\Temp\debug.txt";

            ////Clear the output file
            System.IO.File.WriteAllBytes(filename, new byte[0]);

            // Write to output file
            using (StreamWriter w = new StreamWriter(filename))
            {
                w.Write(_collect);
                w.Close();
            }
        }
        public static void OutputWriter(string _collect)
        {
            //Create filename
            string filename = @"C:\Temp\debug.txt";

            ////Clear the output file
            System.IO.File.WriteAllBytes(filename, new byte[0]);

            // Write to output file
            using (StreamWriter w = new StreamWriter(filename))
            {
                w.Write(_collect);
                w.Close();
            }
        }
    }
}
