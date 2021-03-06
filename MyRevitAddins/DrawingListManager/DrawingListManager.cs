﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using MsgBox = System.Windows.Forms.MessageBox;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using NLog;
using System.Drawing;

namespace MEPUtils.DrawingListManager
{
    static class DrawingListManager
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new DrawingListManagerForm());
        }
    }

    public class DrwgLstMan
    {
        //Form caching
        public DrawingListManagerForm dlmF;
        //Fields for filename analysis
        internal List<string> drwgFileNameList;
        internal List<string> stagingFileNameList;
        internal List<Drwg> drwgListFiles = new List<Drwg>();
        internal List<Drwg> drwgListExcel = new List<Drwg>();
        internal List<Drwg> drwgListMeta = new List<Drwg>();
        internal List<Drwg> drwgListAggregated = new List<Drwg>();
        internal List<Drwg> drwgListStaging = new List<Drwg>();
        internal Field.Fields fs = new Field.Fields();
        //Fields for Excel data analysis
        private DataSet ExcelDataSet;
        //Fields for Metadata data
        private DataTable MetadataDataTable;
        //internal DataTable FileNameDataTable;
        internal DataTable AggregateDataTable;
        //DataGridViewStyles
        internal DgvStyles dgvStyles = new DgvStyles();

        //Logger
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        internal void ScanRescanFilesAndList(string pathToDrwgFolder, string pathToStagingFolder)
        {
            if (drwgFileNameList != null) drwgFileNameList.Clear();
            if (drwgFileNameList == null || drwgFileNameList.Count < 1)
            {
                drwgFileNameList = Directory.EnumerateFiles(
                    pathToDrwgFolder, "*.pdf", SearchOption.TopDirectoryOnly).ToList();
                if (drwgFileNameList == null || drwgFileNameList.Count < 1)
                {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("No valid files found at specified location!", "Error!", buttons);
                }
            }

            if (stagingFileNameList != null) stagingFileNameList.Clear();
            if (stagingFileNameList == null || stagingFileNameList.Count < 1)
            {
                stagingFileNameList = Directory.EnumerateFiles(
                    pathToStagingFolder, "*.pdf", SearchOption.TopDirectoryOnly).ToList();
            }
        }
        /// <summary>
        /// Builds the DataTable to store the data about the pdf files
        /// </summary>
        internal void ResetDgv()
        {
            drwgListFiles = new List<Drwg>();
            drwgListExcel = new List<Drwg>();
            drwgListMeta = new List<Drwg>();
            drwgListAggregated = new List<Drwg>();
            drwgListStaging = new List<Drwg>();
        }
        internal void PopulateDrwgDataFromFileName()
        {
            foreach (string fileNameWithPath in drwgFileNameList)
            {
                Drwg drwg = new Drwg(fileNameWithPath);
                drwgListFiles.Add(drwg);
                drwg.ReadDrwgDataFromFileName();
            }
        }
        //internal void BuildFileNameDataTable()
        //{
        //    FileNameDataTable = new DataTable("DrwgFileNameData");

        //    #region DataTable Definition
        //    DataColumn column;

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._Number.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._Title.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._FileNameFormat.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._Scale.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._Date.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._Revision.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._RevisionDate.ColumnName;
        //    FileNameDataTable.Columns.Add(column);

        //    column = new DataColumn();
        //    column.DataType = typeof(string);
        //    column.ColumnName = fs._Extension.ColumnName;
        //    FileNameDataTable.Columns.Add(column);
        //    #endregion
        //}
        //internal void PopulateFileNameDataTable()
        //{
        //    foreach (Drwg drwg in drwgListFiles)
        //    {
        //        DataRow row = FileNameDataTable.NewRow();

        //        row[fs._Number.ColumnName] = drwg.GetValue(Source.FileName, FieldName.Number);
        //        row[fs._Title.ColumnName] = drwg.GetValue(Source.FileName, FieldName.Title);
        //        row[fs._FileNameFormat.ColumnName] = drwg.GetValue(Source.FileName, FieldName.FileNameFormat);
        //        row[fs._Revision.ColumnName] = drwg.GetValue(Source.FileName, FieldName.Revision);
        //        row[fs._Extension.ColumnName] = drwg.GetValue(Source.FileName, FieldName.Extension);

        //        FileNameDataTable.Rows.Add(row);
        //    }
        //    FileNameDataTable.AcceptChanges();
        //}
        internal void ScanExcelFile(string pathToDwgList)
        {
            //Fields for Excel Interop
            Microsoft.Office.Interop.Excel.Workbook wb;
            Microsoft.Office.Interop.Excel.Sheets wss;
            Microsoft.Office.Interop.Excel.Worksheet ws;
            Microsoft.Office.Interop.Excel.Application oXL;
            object misVal = System.Reflection.Missing.Value;
            oXL = new Microsoft.Office.Interop.Excel.Application();
            oXL.Visible = false;
            oXL.DisplayAlerts = false;
            try
            {
                wb = oXL.Workbooks.Open(pathToDwgList, 0, false, 5, "", "", false,
                        Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true, false, 0, false, false,
                        Microsoft.Office.Interop.Excel.XlCorruptLoad.xlNormalLoad);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            wss = wb.Worksheets;
            ws = (Microsoft.Office.Interop.Excel.Worksheet)wss.Item[1];

            Range usedRange = ws.UsedRange;
            int usedRows = usedRange.Rows.Count;
            int usedCols = usedRange.Columns.Count;
            int rowStartIdx = 0;
            //Detect first row of the drawingslist
            //Assumes that Drawing data starts with a field containing string "Tegningsnr." -- subject to change
            string firstColumnValue = "Tegningsnr."; //<-- Here be the string that triggers the start of data.
            for (int row = 1; row < usedRows + 1; row++)
            {
                var cellValue = (string)(ws.Cells[row, 1] as Range).Value;
                if (cellValue == firstColumnValue)
                { rowStartIdx = row; break; }
            }

            if (rowStartIdx == 0)
            {
                MsgBox.Show($"Excel file did not find a cell in the first column\n" +
                            $"containing the first column keyword: {firstColumnValue}");
                return;
            }

            //int lastColIdx = fs.GetAllFields().MaxBy(x => x.ExcelColumnIdx).Select(x => x.ExcelColumnIdx).FirstOrDefault();
            //Range drwgUsedRange = oXL.Range[ws.Cells[rowStartIdx, 1], ws.Cells[usedRows, lastColIdx]];

            //myRange.Select();
            //MsgBox.Show(myRange.Address);

            #region BuildDataTableFromExcel
            ExcelDataSet = new DataSet("ExcelDrwgData");
            DataTable table = null;

            //Main loop creating DataTables for DataSet
            for (int i = rowStartIdx; i < usedRows + 1; i++)
            {
                //Detect start of the table
                var cellValue = (string)(ws.Cells[i, 1] as Range).Value;
                if (cellValue == firstColumnValue) //Header row detected
                {
                    //Add previously made dataTable to dataSet except at the first iteration
                    if (i != rowStartIdx) ExcelDataSet.Tables.Add(table);

                    //Get the name of DataTable
                    string nameOfDt = (string)(ws.Cells[i, 2] as Range).Value;
                    table = new DataTable(nameOfDt);

                    //Add the header values to the column names
                    DataColumn column;
                    for (int j = 1; j < usedCols + 1; j++)
                    {
                        cellValue = (string)(ws.Cells[i, j] as Range).Value;
                        column = new DataColumn();
                        column.DataType = typeof(string);
                        column.ColumnName = fs.GetExcelColumnField(j).ColumnName;
                        table.Columns.Add(column);
                        //MsgBox.Show(cellValue);
                    }
                }
                else
                {
                    DataRow row = table.NewRow();

                    for (int j = 1; j < usedCols + 1; j++)
                    {
                        string value;
                        var cellValueRaw = (ws.Cells[i, j] as Range).Value;
                        if (cellValueRaw == null) value = "";
                        else if (cellValueRaw is string) value = (string)cellValueRaw;
                        else { value = cellValueRaw.ToString(); }
                        row[j - 1] = value;
                    }
                    table.Rows.Add(row);
                }
            }
            //Add last made data table to data set.
            ExcelDataSet.Tables.Add(table);
            ExcelDataSet.AcceptChanges();
            #endregion

            wb.Close(true, misVal, misVal);
            oXL.Quit();
        }
        internal void PopulateDrwgDataFromExcel()
        {
            foreach (DataTable table in ExcelDataSet.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    Drwg drwg = new Drwg();
                    drwg.DataFromExcel = new DrwgProps.Source_Excel(
                    row.Field<string>(fs.Number.ColumnName),
                    row.Field<string>(fs.Title.ColumnName),
                    row.Field<string>(fs.Revision.ColumnName),
                    row.Field<string>(fs.Scale.ColumnName),
                    row.Field<string>(fs.Date.ColumnName),
                    row.Field<string>(fs.RevisionDate.ColumnName)
                    );
                    drwgListExcel.Add(drwg);
                }
            }
        }
        /// <summary>
        /// Retrieves datarows from supplied datatable based on values stored in props. Props are used to get the fields.
        /// </summary>
        /// <typeparam name="T">DataTable or DataSet.</typeparam>
        /// <param name="props">Data values -- must be populated!</param>
        /// <param name="Data">Data to search.</param>
        /// <param name="includeTitleFieldInSelectExpression">If searchin from file name data, exclude title, as the filenmae titles are not corresponding</param>
        /// <returns></returns>
        private HashSet<DataRow> GetRowsBySelectQuery<T>(DrwgProps props, T Data, bool includeTitleFieldInSelectExpression)
        {
            HashSet<DataRow> foundRows = new HashSet<DataRow>();

            //Build expression to use Table.Select
            string number = props.Number.Value;
            string title = props.Title.Value.Replace("'", "''");
            string rev = props.Revision.Value;
            List<string> exprlist = new List<string>();
            exprlist.Add($"[{fs.Number.ColumnName}] = '{number}'");
            if (includeTitleFieldInSelectExpression)
            {
                exprlist.Add($"[{fs.Title.ColumnName}] = '{title}'");
            }
            if (!rev.IsNoE()) exprlist.Add($"[{fs.Revision.ColumnName}] = '{rev}'");
            string expr = string.Join(" AND ", exprlist);

            if (Data is DataSet dataSet)
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    DataRow[] result = table.Select(expr);
                    HashSet<DataRow> newSet = new HashSet<DataRow>(result);
                    foundRows.UnionWith(newSet);
                }
            }
            else if (Data is DataTable table)
            {
                DataRow[] result = table.Select(expr);
                HashSet<DataRow> newSet = new HashSet<DataRow>(result);
                foundRows.UnionWith(newSet);
            }
            return foundRows;
        }
        internal void ReadMetadataData(string path)
        {
            if (drwgFileNameList.Count < 1 || drwgFileNameList == null)
            {
                drwgFileNameList = Directory.EnumerateFiles(path, "*.pdf", SearchOption.TopDirectoryOnly).ToList();
                if (drwgFileNameList.Count < 1 || drwgFileNameList == null)
                {
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show("No valid files found at specified location!", "Error!", buttons);
                }
            }

            MetadataDataTable = new DataTable("MetadataData");

            #region DataTable Definition
            DataColumn column;

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Number.ColumnName;
            MetadataDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Title.ColumnName;
            MetadataDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Scale.ColumnName;
            MetadataDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Date.ColumnName;
            MetadataDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Revision.ColumnName;
            MetadataDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.RevisionDate.ColumnName;
            MetadataDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.DrawingListCategory.ColumnName;
            MetadataDataTable.Columns.Add(column);
            #endregion

            HashSet<Field> fields = new HashSet<Field>(fs.GetAllFields().Where(x => x.IsMetaDataField));

            foreach (string filePath in drwgFileNameList)
            {
                PdfDocument document = null;
                try
                {
                    if (0 != PdfReader.TestPdfFile(filePath)) document = PdfReader.Open(filePath);
                    else continue;
                }
                catch (Exception) { continue; }

                var props = document.Info.Elements;

                if (fields.Any(x => props.ContainsKey("/" + x.MetadataName)))
                {
                    DataRow row = MetadataDataTable.NewRow();
                    foreach (Field field in fields)
                    {
                        if (props.ContainsKey("/" + field.MetadataName))
                        {
                            string s = props["/" + field.MetadataName].ToString();
                            //Substring removes leading and closing parantheses
                            row[field.ColumnName] = s.Substring(1, s.Length - 2);
                        }
                    }
                    MetadataDataTable.Rows.Add(row);
                }
                document.Close();
            }
            MetadataDataTable.AcceptChanges();
        }
        internal void PopulateDrwgDataFromMetadata()
        {
            foreach (DataRow row in MetadataDataTable.Rows)
            {
                Drwg drwg = new Drwg();
                drwg.DataFromMetadata = new DrwgProps.Source_Meta(
                row.Field<string>(fs.Number.ColumnName),
                row.Field<string>(fs.Title.ColumnName),
                row.Field<string>(fs.Revision.ColumnName),
                row.Field<string>(fs.Scale.ColumnName),
                row.Field<string>(fs.Date.ColumnName),
                row.Field<string>(fs.RevisionDate.ColumnName),
                row.Field<string>(fs.DrawingListCategory.ColumnName)
                );
                drwgListMeta.Add(drwg);
            }
        }
        internal void CreateAggregateDataTable()
        {
            //This method defines the DataGridView
            //Add columns here
            AggregateDataTable = new DataTable("AggregateData");

            #region DataTable Definition
            DataColumn column;

            column = new DataColumn();
            column.DataType = typeof(bool);
            column.ColumnName = fs.Select.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Number.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Title.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.FileNameFormat.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Scale.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Date.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Revision.ColumnName;
            AggregateDataTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.RevisionDate.ColumnName;
            AggregateDataTable.Columns.Add(column);

            //Debug column showing the extension of file
            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs.Extension.ColumnName;
            AggregateDataTable.Columns.Add(column);
            #endregion
        }
        internal void AggregateData()
        {
            List<Drwg> tempDrwgListFiles = new List<Drwg>(drwgListFiles);
            List<Drwg> tempDrwgListExcel = new List<Drwg>(drwgListExcel);
            List<Drwg> tempDrwgListMeta = new List<Drwg>(drwgListMeta);
            Log.Info("Test message");
            //Compare meta and file data with one in excel
            //And match data to excel data
            foreach (Drwg drwgExcel in tempDrwgListExcel)
            {
                var query1 = tempDrwgListMeta.Where(x =>
                                   x?.DataFromMetadata?.Number == drwgExcel.DataFromExcel.Number &&
                                   x?.DataFromMetadata?.Revision == drwgExcel.DataFromExcel.Revision);

                if (query1.Count() > 1) Log.Warn("var query1 encountered count > 1.");

                Drwg drwgMeta = query1.FirstOrDefault();

                var query2 = tempDrwgListFiles.Where(x =>
                                    x?.DataFromFileName?.Number == drwgExcel.DataFromExcel.Number &&
                                    x?.DataFromFileName?.Revision == drwgExcel.DataFromExcel.Revision);

                if (query2.Count() > 1) Log.Warn("var query2 encountered count > 1.");

                Drwg drwgFile = query2.FirstOrDefault();

                Drwg drwgAggregated = new Drwg();
                drwgListAggregated.Add(drwgAggregated);

                drwgAggregated.DataFromFileName = drwgFile?.DataFromFileName;
                drwgAggregated.DataFromExcel = drwgExcel?.DataFromExcel;
                drwgAggregated.DataFromMetadata = drwgMeta?.DataFromMetadata;

                if (!(drwgMeta is null)) tempDrwgListMeta.RemoveAll(x => x.Id == drwgMeta.Id);
                if (!(drwgFile is null)) tempDrwgListFiles.RemoveAll(x => x.Id == drwgFile.Id);
            }

            //Now to check if the temp collections have anything left
            //If they have that means that there's a descrepancy between data which needs located
            //Excel list should have been run to it's completion, so no need to check there.

            if (tempDrwgListFiles.Count > 0)
            {
                Log.Info("tempDrwgListFiles count {0} -> drwg(s) not in excel", tempDrwgListFiles.Count);
                foreach (Drwg drwgFile in tempDrwgListFiles)
                {
                    var query3 = tempDrwgListMeta.Where(x =>
                                   x?.DataFromMetadata?.Number == drwgFile.DataFromFileName.Number &&
                                   x?.DataFromMetadata?.Revision == drwgFile.DataFromFileName.Revision);

                    if (query3.Count() > 1) Log.Warn("var query3 encountered count > 1.");

                    Drwg drwgMeta = query3.FirstOrDefault();

                    Drwg drwgAggregated = new Drwg();
                    drwgListAggregated.Add(drwgAggregated);

                    drwgAggregated.DataFromFileName = drwgFile?.DataFromFileName;
                    drwgAggregated.DataFromMetadata = drwgMeta?.DataFromMetadata;

                    if (!(drwgMeta is null)) tempDrwgListMeta.RemoveAll(x => x.Id == drwgMeta.Id);
                }
            }

            //Now to check if Meta temp collection has anything left
            if (tempDrwgListMeta.Count > 0)
            {
                Log.Warn("tempDrwgListMeta count {0} -> drwg(s) not in excel nor filedata", tempDrwgListMeta.Count);
                foreach (Drwg drwgMeta in tempDrwgListMeta)
                {
                    Drwg drwgAggregated = new Drwg();
                    drwgListAggregated.Add(drwgAggregated);

                    drwgAggregated.DataFromMetadata = drwgMeta?.DataFromMetadata;
                }
                tempDrwgListMeta.Clear();
            }
        }
        internal void PopulateAggregateDataTable()
        {
            foreach (Drwg drwg in drwgListAggregated)
            {
                //Populate the aggregate datatable
                DataRow row = AggregateDataTable.NewRow();

                row[fs.Number.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.Number);
                row[fs.Title.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.Title);
                row[fs.FileNameFormat.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.FileNameFormat);
                row[fs.Scale.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.Scale);
                row[fs.Date.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.Date);
                row[fs.Revision.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.Revision);
                row[fs.RevisionDate.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.RevisionDate);
                row[fs.Extension.ColumnName] = drwg.TryGetValueOfSpecificPropsField(FieldName.Extension);

                //Store reference to the data row which holds the data
                drwg.dataRowGV = row;
                //Add the data row to the table
                AggregateDataTable.Rows.Add(row);
            }
            AggregateDataTable.AcceptChanges();
        }
        internal void ReadStagingData()
        {
            foreach (string fileNameWithPath in stagingFileNameList)
            {
                Drwg drwg = new Drwg(fileNameWithPath);
                drwgListStaging.Add(drwg);
                drwg.ReadDrwgDataFromStaging();
            }
        }
        internal DataGridViewRow GetDgvRow(DataGridView dgv, DataRow drow)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if ((DataRowView)row.DataBoundItem == null) continue;
                //Using reference equality
                if (((DataRowView)row.DataBoundItem).Row == drow) return row;
            }
            return null;
        }
        internal void AnalyzeDataAndUpdateGridView(DataGridView dGV)
        {
            //Analyze existing data
            foreach (Drwg drwg in drwgListAggregated)
            {
                DataGridViewRow dGVRow = GetDgvRow(dGV, drwg.dataRowGV);
                if (dGVRow != null)
                {
                    var fields = fs.GetAllFields().Where(x => x.IsExcelField);
                    foreach (Field field in fields)
                    {
                        AnalyzeFields(drwg, dGVRow, field);
                    }
                }
            }

            void AnalyzeFields(Drwg drwg, DataGridViewRow dGVRow, Field field)
            {
                //Retrieve cell for the data
                DataGridViewCell cell;
                try
                {
                    cell = dGVRow.Cells[field.ColumnName];
                }
                catch (Exception) { return; }
                if (cell == null) return;
                cell.ToolTipText = drwg.BuildToolTip(field.FieldName);

                //1. Does Excel and File data exist?
                if (field.IsFileAndExcelField)
                {
                    if ((drwg.GetValue(Source.Excel, field.FieldName).IsNoE() ||
                        drwg.GetValue(Source.FileName, field.FieldName).IsNoE()) &&
                        field.FieldName != FieldName.Revision)
                    {
                        //1.1 Does Excel and File exist? -> No
                        cell.Style = dgvStyles.Error;
                        return;
                    }
                }
                //1.2 Does Excel and File exist? -> Yes -> Fall through

                //2. Does meta exist?
                if (field.IsMetaDataField && !drwg.GetValue(Source.MetaData, field.FieldName).IsNoE())
                {
                    //2.1 Does meta exist? -> Yes
                    //3. Does data match?
                    //3.1 Does data match? -> Yes
                    if (drwg.CompareFieldValues(field.FieldName))
                    {
                        cell.Style = dgvStyles.AllOkay;
                        return;
                    }
                    else
                    {
                        cell.Style = dgvStyles.Warning;
                        return;
                    }
                }
                else
                {
                    //2.2 Does meta exist? -> No
                    //2.2.1 Does data match?
                    //2.2.1 Does data match? -> Yes
                    if (drwg.CompareFieldValues(field.FieldName))
                    {
                        cell.Style = dgvStyles.OkayMetaMissing;
                        return;
                    }
                    else
                    {
                        //Does data match -> No
                        cell.Style = dgvStyles.Warning;
                        return;
                    }
                }
            }
        }
        internal class DgvStyles
        {
            internal DataGridViewCellStyle AllOkay = new DataGridViewCellStyle()
            { ForeColor = Color.Green, BackColor = Color.GreenYellow };
            internal DataGridViewCellStyle Warning = new DataGridViewCellStyle()
            { ForeColor = Color.Yellow, BackColor = Color.DeepSkyBlue };
            internal DataGridViewCellStyle OkayMetaMissing = new DataGridViewCellStyle()
            { ForeColor = Color.Green, BackColor = Color.LemonChiffon };
            internal DataGridViewCellStyle Error = new DataGridViewCellStyle()
            { ForeColor = Color.DarkRed, BackColor = Color.Thistle };
        }
        internal bool AnalyzeStagingDataAndUpdateGridView(DataGridView dGV)
        {
            bool stagingFound = false;

            //Analyze staging data
            foreach (Drwg drwg in drwgListAggregated)
            {
                Drwg stagingDrwg = drwgListStaging.Where(x => x.DataFromStaging.Number.Value ==
                drwg.TryGetValueOfSpecificPropsField(FieldName.Number)).FirstOrDefault();
                if (stagingDrwg == null) continue;

                DataGridViewRow dGVRow = GetDgvRow(dGV, drwg.dataRowGV);
                if (dGVRow == null) continue;

                Field revField = new Field.Revision();

                string curRev = drwg.TryGetValueOfSpecificPropsField(FieldName.Revision);
                string newRev = stagingDrwg.GetValue(Source.Staging, FieldName.Revision);

                if (curRev != newRev) stagingFound = true;

                //Retrieve cell for the data
                DataGridViewCell cell;
                try
                {
                    cell = dGVRow.Cells[revField.ColumnName];
                }
                catch (Exception) { continue; }
                if (cell == null) continue;
                cell.ToolTipText = $"{curRev} -> {newRev}";
                cell.Style = dgvStyles.Error;
            }

            return stagingFound;
        }
    }
}