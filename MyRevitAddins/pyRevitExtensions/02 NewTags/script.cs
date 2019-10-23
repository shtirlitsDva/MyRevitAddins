using System;
using System.Data;
using System.Data.OleDb;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using pyRevitLabs.NLog;

namespace pyRevitExtensions
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class NewTags : IExternalCommand
    {
        //public ExecParams execParams;
        private Logger log = LogManager.GetCurrentClassLogger();

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region LoggerSetup
            //Nlog configuration
            var nlogConfig = new pyRevitLabs.NLog.Config.LoggingConfiguration();
            //Targets
            var logfile = new pyRevitLabs.NLog.Targets.FileTarget("logfile") { FileName = "E:\\GitHub\\log.txt", DeleteOldFileOnStartup = true };
            //Rules
            nlogConfig.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            //Apply config
            pyRevitLabs.NLog.LogManager.Configuration = nlogConfig;
            //DISABLE LOGGING
            //pyRevitLabs.NLog.LogManager.DisableLogging();
            #endregion

            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            log.Info("Script starting!");

            string pathToDataFile = @"E:\Damgaard Rådgivende Ingeniører ApS\058-1046 - Damvarmelager - Dokumenter\02 Vekslercentral\02 Komponenter\2.1 Komponentliste\Tags_Nye_Gamle_2019.10.23.xlsx";
            log.Info("Loading file: " + pathToDataFile);
            DataSet dataSet = ImportExcelToDataSet(pathToDataFile, "Yes");
            List<string> dataTableNames = new List<string>();
            foreach (DataTable item in dataSet.Tables)
            {
                dataTableNames.Add(item.TableName);
                log.Info("Sheets present in file: " + item.TableName);
            }

            string sheetName = "Sheet1";
            log.Info("Reading sheet: " + sheetName);
            DataTable dataTable = ReadDataTable(dataSet.Tables, sheetName);

            log.Info("Columns present in sheet:");

            foreach (DataColumn dc in dataTable.Columns)
            {
                log.Info(dc.ColumnName);
            }


            foreach (DataRow dr in dataTable.Rows)
            {
                dr.
            }

            return Result.Succeeded;
        }

        public DataSet ImportExcelToDataSet(string fileName, string dataHasHeaders)
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

            if (data == null) log.Info("Data set is null");
            if (data.Tables.Count < 1) log.Info("Table count in DataSet is 0");

            return data;
        }

        static string[] GetExcelSheetNames(string connectionString)
        {
            //OleDbConnection con = null;
            DataTable dt = null;
            using (OleDbConnection con = new OleDbConnection(connectionString))
            {
                con.Open();
                dt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            }

            if (dt == null) return null;

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
            return (from DataTable dtbl in dataTableCollection where dtbl.TableName == tableName select dtbl).FirstOrDefault();
        }

    }
}
