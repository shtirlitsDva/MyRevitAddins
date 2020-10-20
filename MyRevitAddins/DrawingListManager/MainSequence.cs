using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManager
{
    public class MainSequence
    {
        internal void ExecuteMainSequence(DrwgLstMan dlm, string pathToDwgFolder, string pathToDwgList)
        {
            //Load file name data
            dlm.ScanRescanFilesAndList(pathToDwgFolder);
            dlm.PopulateDrwgDataFromFileName();
            dlm.BuildFileNameDataTable();
            dlm.PopulateFileNameDataTable();

            #region LoadExcelData-GC-Populate
            //Load excel data
            dlm.ScanExcelFile(pathToDwgList);
            //Dispose of Excel objects
            //https://stackoverflow.com/questions/25134024/clean-up-excel-interop-objects-with-idisposable/25135685#25135685
            GC.Collect();
            GC.WaitForPendingFinalizers();
            //Populate drwg data by data from Excel
            dlm.PopulateDrwgDataFromExcel();
            #endregion

            #region LoadMetaData-GC-Populate
            //Load data from metadata
            dlm.ReadMetadataData(pathToDwgFolder);
            //Run GC just to be sure that the pdf pointers are gone
            GC.Collect();
            GC.WaitForPendingFinalizers();
            dlm.PopulateDrwgDataFromMetadata();
            #endregion

            //Analyze data
            foreach (Drwg drwg in dlm.drwgListFiles) drwg.CalculateState();

            #region Debug
            //StringBuilder sb = new StringBuilder();
            //List<string> list = new List<string>();
            ////Group all states and list them
            //var query = dlm.drwgList.GroupBy(x => x.State);
            //foreach (var gr in query) list.Add($"{(int)gr.Key} - {gr.Key}");
            //list.Sort();
            //sb.Append(string.Join("\n", list));

            //Just for fun list all states
            //for (int val = 0; val <= 16385; val++)
            //    sb.AppendLine($"{val} - {(Drwg.StateFlags)val}");
            //Output.OutputWriter(sb); 
            #endregion

            foreach (Drwg drwg in dlm.drwgListFiles)
            {
                //drwg.ActOnState();
            }

            #region Code Bin
            //Essential formatting!
            //foreach (DataGridViewColumn dc in dGV1.Columns)
            //{
            //    dc.DefaultCellStyle.Font = new Font("Arial", 26F, GraphicsUnit.Pixel);
            //    dc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            //}
            //dGV1.DataSource = dlm.FileNameDataTable;

            //column = new DataColumn();
            //column.DataType = typeof(bool);
            //column.ColumnName = fs._Select.ColumnName;
            //FileNameDataTable.Columns.Add(column);

            ////Debug column showing the state of drwg
            //column = new DataColumn("State");
            //column.DataType = typeof(int);
            //FileNameDataTable.Columns.Add(column);

            //dGV1.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            //foreach (Drwg drwg in dlm.drwgList) dlm.AddStateToGridView(drwg, drwg.DataFromFileName, dlm.FileNameDataTable);
            //dlm.FileNameDataTable.AcceptChanges();
            #endregion
        }
    }
}
