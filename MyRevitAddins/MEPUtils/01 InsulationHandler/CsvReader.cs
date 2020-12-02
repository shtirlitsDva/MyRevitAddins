using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Data;

namespace MEPUtils
{
    internal static class CsvReader
    {
        internal static DataTable ReadInsulationCsv(string path)
        {
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { ";" });
                csvParser.HasFieldsEnclosedInQuotes = false;

                string[] colNames = new string[0];
                string[] fields = new string[0];

                DataTable dt = new DataTable("InsulationParameters");

                int counter = 0;
                while (!csvParser.EndOfData)
                {
                    if (counter == 0)
                    {
                        colNames = csvParser.ReadFields();
                        counter++;
                        //Console.WriteLine($"{colNames.Length} columns detected!");
                        for (int i = 0; i < colNames.Length; i++)
                        {
                            if (i == 0 || i == 1)
                            {
                                //Two first columns are strings
                                DataColumn dc = new DataColumn(colNames[i]);
                                dc.DataType = typeof(string);
                                dt.Columns.Add(dc);
                            }
                            else
                            {
                                //The rest of columns are INT
                                DataColumn dc = new DataColumn(colNames[i]);
                                dc.DataType = typeof(int);
                                dt.Columns.Add(dc);
                            }
                        }
                    }
                    else
                    {// Read current line fields, pointer moves to the next line.
                        fields = csvParser.ReadFields();
                        DataRow dr = dt.NewRow();

                        for (int i = 0; i < colNames.Length; i++)
                        {
                            if (i == 0 || i == 1)
                            {
                                dr[i] = fields[i];
                            }
                            else
                            {
                                int intResult;
                                if (int.TryParse(fields[i], out intResult))
                                    dr[i] = intResult;
                                else dr[i] = 0;
                            }
                        }
                        dt.Rows.Add(dr);
                        counter++;
                    }
                }
                return dt;
            }
        }
    }
}
