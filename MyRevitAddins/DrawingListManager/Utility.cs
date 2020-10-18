using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManager
{
    public static class Extensions
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
    }
    public static class Output
    {
        public static void OutputWriter(StringBuilder _collect)
        {
            //Create filename
            string filename = @"C:\Users\Michail Golubjev\Documents\debug.txt";

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
