using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace MEPUtils.DrawingListManager
{
    public class MainSequence
    {
        internal void ExecuteMainSequence(DrwgLstMan dlm, DataGridView dGV,
            string pathToDwgFolder, string pathToDwgList, string pathToStagingFolder)
        {
            //Load file name data
            dlm.ScanRescanFilesAndList(pathToDwgFolder, pathToStagingFolder);
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

            #region Read staging files
            if (dlm.stagingFileNameList != null && dlm.stagingFileNameList.Count > 0)
            {
                dlm.ReadStagingData();
            }
            #endregion

            //Bind data
            dGV.DataSource = dlm.AggregateDataTable;

            //Formatting to see better
            foreach (DataGridViewColumn dc in dGV.Columns)
            {
                dc.DefaultCellStyle.Font = new Font("Arial", 14F, GraphicsUnit.Pixel);
                dc.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }

            dGV.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dlm.AnalyzeDataAndUpdateGridView(dGV);
            if (dlm.AnalyzeStagingDataAndUpdateGridView(dGV)) dlm.dlmF.flipConsolidateButton();

            #region Debug
            //StringBuilder sb = new StringBuilder();
            //Output.OutputWriter(sb);
            #endregion

            #region Code Bin

            #endregion
        }
    }
    public class ConsolidateSequence
    {
        internal void ExecuteConsolidate(DrwgLstMan dlm, string pathToDrwgFolder,
            string pathToStagingFolder)
        {
            string pathToStagingDWFFolder = pathToStagingFolder + "DWF\\";
            string pathToMainDWFFolder = pathToDrwgFolder + "DWF\\";
            string pathToArchiveFolder = pathToDrwgFolder + "Arkiv\\";
            Directory.CreateDirectory(pathToArchiveFolder);
            string pathToArchiveDWFFolder = pathToArchiveFolder + "DWF\\";
            Directory.CreateDirectory(pathToArchiveDWFFolder);

            foreach (Drwg sDrwg in dlm.drwgListStaging)
            {
                try
                {
                    //Detect if staging file has a version in main folder
                    Drwg drwg = dlm.drwgListFiles.Where(x => x
                    .DataFromFileName.Number.Value == sDrwg.DataFromStaging.Number.Value)
                    .FirstOrDefault();

                    //Move pdf from staging to main
                    string originalName = pathToStagingFolder + sDrwg.FileName;
                    string destinationName = pathToDrwgFolder + sDrwg.FileName;
                    File.Move(originalName, destinationName);

                    //Construct dwf file name
                    string dwfFileName = sDrwg.FileName.Remove(sDrwg.FileName.Length - 3) + "dwf";
                    string originalDwfName = pathToStagingDWFFolder + dwfFileName;
                    string destinationDwfName = pathToMainDWFFolder + dwfFileName;

                    //Move (if) dwf from staging to main
                    if (File.Exists(originalDwfName))
                    {
                        new System.IO.FileInfo(destinationDwfName).Directory.Create();
                        File.Move(originalDwfName, destinationDwfName);
                    }

                    if (drwg != null)
                    {
                        //Move pdf from main to archive
                        string originalDrwgName = pathToDrwgFolder + drwg.FileName;
                        string destinationDrwgName = pathToArchiveFolder + drwg.FileName;
                        File.Move(originalDrwgName, destinationDrwgName);
                        //Move (if) dwf from main to archive
                        string originalDwfFileName = drwg.FileName.Remove(drwg.FileName.Length - 3) + "dwf";
                        string originalDwfDrwgName = pathToMainDWFFolder + originalDwfFileName;
                        string destinationDwfDrwgName = pathToArchiveDWFFolder + originalDwfFileName;
                        if (File.Exists(originalDwfDrwgName))
                            File.Move(originalDwfDrwgName, destinationDwfDrwgName);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }

            #region Debug
            //Output.OutputWriter(pathToStagingDWFFolder);
            #endregion
        }
    }
}
