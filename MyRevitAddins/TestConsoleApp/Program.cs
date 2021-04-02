using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Text.RegularExpressions;
using MoreLinq;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = @"X:\AC - Rohr2\VEKS\Mølleholmen 5 Veksler_0\R2DOC\Support loads.csv";

            //Process the file
            //Skip is to remove the first line
            string allLines = string.Join(Environment.NewLine, File.ReadAllLines(fileName).Skip(1).ToArray());
            string[] splitByEmptyLines = allLines.Split(new string[]
                { Environment.NewLine +" "+ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            //Whole block as string
            for (int i = 0; i < splitByEmptyLines.Length; i++)
            {
                string[] splitByLinebreak = splitByEmptyLines[i].Split(
                    new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                string[][] atomised = new string[splitByLinebreak.Length][];

                //Block as lines
                for (int j = 0; j < splitByLinebreak.Length; j++)
                {
                    string temp = Regex.Replace(splitByLinebreak[j], @"\s\s+", "");
                    temp = Regex.Replace(temp, @"\s;", ";");
                    temp = Regex.Replace(temp, @";\s", ";");
                    temp = Regex.Replace(temp, "\"", "");
                    atomised[j] = temp.Split(';');
                }

                Console.WriteLine("-------------------------------------------------------------------");
                Console.WriteLine("Type: " + atomised[3][2]);
                Console.WriteLine("Tag: " + PrettyPrintArrayOfArrays(new string[1][] { atomised[1][2].Split('_') }));
                Console.WriteLine("Load Case: " + atomised[7][3]);
                Console.WriteLine("Value: " + atomised[0][12]+" "+ (double.Parse(atomised[7][12]) +10.5));


                //Console.WriteLine(PrettyPrintArrayOfArrays(atomised));
            }
        }

        public static string PrettyPrintArrayOfArrays(string[][] arrayOfArrays)
        {
            if (arrayOfArrays == null)
                return "";

            var prettyArrays = new string[arrayOfArrays.Length];

            for (int i = 0; i < arrayOfArrays.Length; i++)
            {
                prettyArrays[i] = "[" + String.Join(";", arrayOfArrays[i]) + "]";
            }

            return "[" + String.Join(",\n", prettyArrays) + "]";
        }
    }
}
