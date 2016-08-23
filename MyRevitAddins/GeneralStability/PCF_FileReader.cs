using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit_PCF_Importer
{
    class FileReader
    {
        string[] readLines;

        public string[] ReadFile(string path)
        {

            readLines = System.IO.File.ReadAllLines(path);
            return readLines;
        }
    }
}
