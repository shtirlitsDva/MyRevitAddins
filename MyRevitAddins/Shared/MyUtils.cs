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
using Autodesk.Revit.DB.Mechanical;

namespace Shared
{
    public static class DataHandler
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
                    con.Close();

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

        public static DataTable ReadDataTable(DataTableCollection dataTableCollection, string tableName)
        {
            var table = (from DataTable dtbl in dataTableCollection where dtbl.TableName == tableName select dtbl)
                .FirstOrDefault();
            return table;
        }

        public static string ReadParameterFromDataTable(string key, DataTable table, string parameter)
        {
            //Test if value exists
            if (table.AsEnumerable().Any(row => row.Field<string>(0) == key))
            {
                var query = from row in table.AsEnumerable()
                            where row.Field<string>(0) == key
                            select row.Field<string>(parameter);

                string value = query.FirstOrDefault();

                //if (value.IsNullOrEmpty()) return null;
                return value;
            }
            else return null;
        }
    }

    public class Dbg
    {
        /// <summary>
        /// This method is used to place an adaptive family which helps in debugging
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static FamilyInstance PlaceAdaptiveFamilyInstance(Document doc, string famAndTypeName, XYZ p1, XYZ p2)
        {
            //Get the symbol
            ElementParameterFilter filter = Filter.ParameterValueFilter(famAndTypeName,
                BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM); //Hardcoded until implements

            FamilySymbol markerSymbol =
                new FilteredElementCollector(doc).WherePasses(filter)
                    .Cast<FamilySymbol>()
                    .FirstOrDefault();

            // Create a new instance of an adaptive component family
            FamilyInstance instance = AdaptiveComponentInstanceUtils.CreateAdaptiveComponentInstance(doc,
                markerSymbol);

            // Get the placement points of this instance
            IList<ElementId> placePointIds = new List<ElementId>();
            placePointIds = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(instance);
            // Set the position of each placement point
            ReferencePoint point1 = doc.GetElement(placePointIds[0]) as ReferencePoint;
            point1.Position = p1;
            ReferencePoint point2 = doc.GetElement(placePointIds[1]) as ReferencePoint;
            point2.Position = p2;

            return instance;
        }
    }
}
