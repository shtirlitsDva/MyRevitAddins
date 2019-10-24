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
            var logfile = new pyRevitLabs.NLog.Targets.FileTarget("logfile") { FileName = "G:\\GitHub\\log.txt", DeleteOldFileOnStartup = true };
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

            string pathToDataFile = @"INPUD PATH TO EXCEL FILE HERE!";
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

            string allColumns = "";

            foreach (DataColumn dc in dataTable.Columns)
            {
                allColumns += dc.ColumnName + "|";
            }
            log.Info(allColumns);

            FilteredElementCollector donorFec = new FilteredElementCollector(doc);
            Element paDonor = donorFec.OfCategory(BuiltInCategory.OST_PipeAccessory).OfClass(typeof(FamilyInstance)).FirstElement();
            if (paDonor == null) { log.Info("Failed to get donor element! -> NULL"); }
            log.Info($"Donor element collected {paDonor.Id.ToString()}, {paDonor.Name}");
            Parameter filterTAG1 = paDonor.LookupParameter("TAG 1");
            Parameter filterTAG2 = paDonor.LookupParameter("TAG 2");

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("New tags!");
                foreach (DataRow dr in dataTable.Rows)
                {
                    FilteredElementCollector col = new FilteredElementCollector(doc);
                    col = col.OfCategory(BuiltInCategory.OST_PipeAccessory).OfClass(typeof(FamilyInstance));

                    string GmlTag1 = dr.Field<string>("Gml Tag 1");
                    string GmlTag2 = Convert.ToString(dr["Gml Tag 2"]).PadLeft(2, '0');

                    string NewTag1 = dr.Field<string>("TAG 1");
                    string NewTag2 = dr.Field<string>("TAG 2");
                    string NewTag3 = Convert.ToString(dr["TAG 3"]).PadLeft(2, '0');

                    ElementParameterFilter epf1 = ParameterValueGenericFilter(doc, GmlTag1, filterTAG1.GUID);
                    ElementParameterFilter epf2 = ParameterValueGenericFilter(doc, GmlTag2, filterTAG2.GUID);

                    col.WherePasses(epf1).WherePasses(epf2);
                    string ids = "";
                    foreach (var id in col.ToElementIds()) ids += id.IntegerValue + " | ";

                    log.Info($"Gml Tag: {GmlTag1}_{GmlTag2} ->" +
                        $"New tag: +{NewTag1}-{NewTag2}{NewTag3}: " +
                        $"Matched existing elements: {ids}");

                    foreach (var el in col.ToElements())
                    {
                        Parameter TAG1 = el.LookupParameter("TAG 1");
                        Parameter TAG2 = el.LookupParameter("TAG 2");

                        TAG1.Set(NewTag1);
                        TAG2.Set(NewTag2 + NewTag3);
                        log.Info($"Updated element {el.Id.IntegerValue}.");
                    }
                    
                }
                tx.Commit();
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

        /// <summary>
        /// Generic Parameter value filter. An attempt to write a generic method,
        /// that returns an element filter consumed by FilteredElementCollector.
        /// </summary>
        /// <typeparam name="T1">Type of the parameter VALUE to filter by.</typeparam>
        /// <typeparam name="T2">Type of the PARAMETER to filter.</typeparam>
        /// <param name="value">Currently: string, bool.</param>
        /// <param name="parameterId">Currently: Guid, BuiltInCategory.</param>
        /// <returns>ElementParameterFilter consumed by FilteredElementCollector.</returns>
        public static ElementParameterFilter ParameterValueGenericFilter<T1, T2>(Document doc, T1 value, T2 parameterId)
        {
            //Initialize ParameterValueProvider
            ParameterValueProvider pvp = null;
            switch (parameterId)
            {
                case BuiltInParameter bip:
                    pvp = new ParameterValueProvider(new ElementId((int)bip));
                    break;
                case Guid guid:
                    SharedParameterElement spe = SharedParameterElement.Lookup(doc, guid);
                    pvp = new ParameterValueProvider(spe.Id);
                    break;
                default:
                    throw new NotImplementedException("ParameterValueGenericFilter: T2 (parameter) type not implemented!");
            }

            //Branch off to value types
            switch (value)
            {
                case string str:
                    FilterStringRuleEvaluator fsrE = new FilterStringEquals();
                    FilterStringRule fsr = new FilterStringRule(pvp, fsrE, str, false);
                    return new ElementParameterFilter(fsr);
                case bool bol:
                    int _value;

                    if (bol == true) _value = 1;
                    else _value = 0;

                    FilterNumericRuleEvaluator fnrE = new FilterNumericEquals();
                    FilterIntegerRule fir = new FilterIntegerRule(pvp, fnrE, _value);
                    return new ElementParameterFilter(fir);
                default:
                    throw new NotImplementedException("ParameterValueGenericFilter: T1 (value) type not implemented!");
            }
        }

    }
}
