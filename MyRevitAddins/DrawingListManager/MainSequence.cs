using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace MEPUtils.DrawingListManager
{
    public class MainSequence
    {
        internal void ExecuteMainSequence(DrwgLstMan dlm, DataGridView dGV, string pathToDwgFolder, string pathToDwgList)
        {
            //Load file name data
            dlm.ScanRescanFilesAndList(pathToDwgFolder);
            dlm.PopulateDrwgDataFromFileName();
            //dlm.BuildFileNameDataTable();
            //dlm.PopulateFileNameDataTable();

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

            #region Aggregation
            dlm.CreateAggregateDataTable();
            dlm.AggregateData();
            dlm.PopulateAggregateDataTable();
            #endregion

            //Bind data
            dGV.DataSource = dlm.AggregateDataTable;

            //Formatting to see better
            foreach (DataGridViewColumn dc in dGV.Columns)
            {
                dc.DefaultCellStyle.Font = new Font("Arial", 26F, GraphicsUnit.Pixel);
                dc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            dGV.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dlm.AnalyzeDataAndUpdateGridView(dGV);

            #region Debug
            //StringBuilder sb = new StringBuilder();
            //Output.OutputWriter(sb);
            #endregion

            //TODO: Add DrawingListCategory also as a last column to Excel data and add it to props.
            //It could be compared to data from metadata and acted upon.

            #region Code Bin

            #endregion


        }
    }
}
