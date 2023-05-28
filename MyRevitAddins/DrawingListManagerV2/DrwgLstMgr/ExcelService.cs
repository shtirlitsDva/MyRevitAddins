using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Range = Microsoft.Office.Interop.Excel.Range;

namespace MEPUtils.DrawingListManagerV2
{
    internal static class ExcelService
    {
        internal static IEnumerable<DrawingInfo> GetDrawingInfosFromExcel(string pathToExcel)
        {
            DrawingInfoTypeEnum drawingType = DrawingInfoTypeEnum.DrawingList;

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
                wb = oXL.Workbooks.Open(pathToExcel, 0, false, 5, "", "", false,
                        Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true, false, 0, false, false,
                        Microsoft.Office.Interop.Excel.XlCorruptLoad.xlNormalLoad);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            wss = wb.Worksheets;
            ws = (Microsoft.Office.Interop.Excel.Worksheet)wss.Item[1];

            Microsoft.Office.Interop.Excel.Range usedRange = ws.UsedRange;
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
                throw new Exception($"Excel file did not find a cell in the first column\n" +
                            $"containing the first column keyword: {firstColumnValue}");
            }

            #region BuildDataTableFromExcel
            //Main loop creating DataTables for DataSet
            var fields = new Field.Fields().GetAllFields().ToDictionary(x => x.ExcelColumnIdx, x => x);
            for (int i = rowStartIdx; i < usedRows + 1; i++)
            {
                //Detect start of the table
                var cellValue = (string)(ws.Cells[i, 1] as Range).Value;
                if (cellValue == firstColumnValue) //Header row detected
                {
                    continue;
                }
                else
                {
                    Dictionary<DrawingInfoPropsEnum, string> dict = 
                        new Dictionary<DrawingInfoPropsEnum, string>();

                    for (int j = 1; j < usedCols + 1; j++)
                    {
                        string value;
                        var cellValueRaw = (ws.Cells[i, j] as Range).Value;
                        if (cellValueRaw == null) value = "";
                        else if (cellValueRaw is string) value = (string)cellValueRaw;
                        else { value = cellValueRaw.ToString(); }

                        dict.Add(fields[j].PropertyName, value);
                    }

                    yield return new DrawingInfo(dict, drawingType);
                }
            }
            #endregion

            wb.Close(true, misVal, misVal);
            oXL.Quit();
        }
    }
}
