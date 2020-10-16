using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using MsgBox = System.Windows.Forms.MessageBox;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

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
        //Fields for filename analysis
        public List<string> drwgFileNameList;
        public List<Drwg> drwgList;
        public DataTable FileNameData;
        public Field.Fields fs = new Field.Fields();

        //Fields for Excel Interop
        private static Microsoft.Office.Interop.Excel.Workbook wb;
        private static Microsoft.Office.Interop.Excel.Sheets wss;
        private static Microsoft.Office.Interop.Excel.Worksheet ws;
        private static Microsoft.Office.Interop.Excel.Application oXL;
        object misVal = System.Reflection.Missing.Value;

        //Fields for Excel data analysis
        DataSet ExcelDataSet;

        //Fields for Metadata data
        public DataTable MetadataData;

        public void EnumeratePdfFiles(string path)
        {
            drwgFileNameList = Directory.EnumerateFiles(path, "*.pdf", SearchOption.TopDirectoryOnly).ToList();
        }
        public void ScanRescanFilesAndList(string path)
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

            BuildDataTable();

            PopulateDataTable();
        }
        /// <summary>
        /// Builds the DataTable to store the data about the pdf files
        /// </summary>
        private void BuildDataTable()
        {
            FileNameData = new DataTable("DrwgFileData");

            #region DataTable Definition
            DataColumn column;

            column = new DataColumn();
            column.DataType = typeof(bool);
            column.ColumnName = fs._Select.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Number.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Title.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._FileNameFormat.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Scale.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Date.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Revision.ColumnName;
            FileNameData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._RevisionDate.ColumnName;
            FileNameData.Columns.Add(column);
            #endregion
        }
        internal bool isExcelRunning() => oXL != null;
        private void PopulateDataTable()
        {
            foreach (string fileNameWithPath in drwgFileNameList)
            {
                Drwg drwg = new Drwg(fileNameWithPath);
                DataRow row = FileNameData.NewRow();

                row[fs._Number.ColumnName] = drwg.DrwgNumberFromFileName;
                row[fs._Title.ColumnName] = drwg.DrwgTitleFromFileName;
                row[fs._FileNameFormat.ColumnName] = drwg.DrwgFileNameFormat;
                row[fs._Revision.ColumnName] = drwg.DrwgRevFromFileName;

                FileNameData.Rows.Add(row);
            }

            FileNameData.AcceptChanges();
        }
        internal void ScanExcelFile(string pathToDwgList)
        {
            oXL = new Microsoft.Office.Interop.Excel.Application();
            oXL.Visible = true;
            oXL.DisplayAlerts = false;
            wb = oXL.Workbooks.Open(pathToDwgList, 0, false, 5, "", "", false,
                Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "", true, false, 0, false, false,
                Microsoft.Office.Interop.Excel.XlCorruptLoad.xlNormalLoad);

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
                        column.ColumnName = cellValue;
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

            //Range myRange = oXL.Range[ws.Cells[1, 1], ws.Cells[4, 5]];

            //myRange.Select();
            //MsgBox.Show(myRange.Address);

            wb.Close(true, misVal, misVal);
            oXL.Quit();
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

            MetadataData = new DataTable("MetadataData");

            #region DataTable Definition
            DataColumn column;

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Number.MetadataName;
            MetadataData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Title.MetadataName;
            MetadataData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Scale.MetadataName;
            MetadataData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Date.MetadataName;
            MetadataData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Revision.MetadataName;
            MetadataData.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._RevisionDate.MetadataName;
            MetadataData.Columns.Add(column);
            #endregion

            List<Field> fields = new List<Field>()
                    { fs._Number, fs._Title, fs._Scale, fs._Date, fs._Revision, fs._RevisionDate };

            foreach (string filePath in drwgFileNameList)
            {
                PdfDocument document = null;
                try { document = PdfReader.Open(filePath); }
                catch (Exception) { continue; }
                
                var props = document.Info.Elements;

                if (fields.Any(x => props.ContainsKey("/" + x.MetadataName)))
                {
                    DataRow row = MetadataData.NewRow();
                    foreach (Field field in fields)
                    {
                        if (props.ContainsKey("/" + field.MetadataName))
                        {
                            string s = props["/" + field.MetadataName].ToString();
                            row[field.MetadataName] = s.Substring(1, s.Length - 2);
                        }
                    }
                    MetadataData.Rows.Add(row);
                }
                document.Close();
            }
            MetadataData.AcceptChanges();
        }
    }

    public class Drwg
    {
        #region Fields
        string FileNameWithPath;
        string FileName;

        FileNameFormat Format;
        DrwgNamingFormat Dnf;
        Field.Fields Fields;

        public string DrwgNumberFromFileName;
        public string DrwgNumberFromMeta;

        public string DrwgTitleFromFileName;
        public string DrwgTitleFromMeta;

        public string DrwgRevFromFileName;
        public string DrwgRevFromMeta;

        public string DrwgScaleFromMeta;
        public string DrwgDateFromMeta;
        public string DrwgRevDateFromMeta;

        public string DrwgFileNameFormat;
        #endregion

        public List<DrwgNamingFormat> NamingFormats;
        //   new List<DrwgNamingFormat> //Except Other format -- it must not be included
        //{  new DrwgNamingFormat.VeksNoRevision(), new DrwgNamingFormat.VeksWithRevision(),
        //   new DrwgNamingFormat.DRI_BygNoRevision(), new DrwgNamingFormat.DRI_BygWithRevision(),
        //   new DrwgNamingFormat.STD_NoRevision(), new DrwgNamingFormat.STD_WithRevision() };

        public Drwg(string fileNameWithPath)
        {
            Fields = new Field.Fields();
            NamingFormats = new DrwgNamingFormat().GetDrwgNamingFormatListExceptOther();

            FileNameWithPath = fileNameWithPath;
            FileName = Path.GetFileName(FileNameWithPath);

            //Test to see if there are multiple matches for filenames -> meaning multiple regex matches -> should be only one match
            if (TestFormatsForMultipleMatches(FileName) > 1) throw
                      new Exception($"Filename {FileName} matched multiple Regex patterns! Must only match one!");

            //Analyze file name and find the format
            Format = DetermineFormat(FileName);

            //Find the correct analyzing format
            if (Format == FileNameFormat.Other) Dnf = new DrwgNamingFormat.Other();
            else Dnf = NamingFormats.Where(x => x.Format == Format).FirstOrDefault();

            //Analyze the file name
            PopulateDrwgData(Dnf, FileName);
        }

        private int TestFormatsForMultipleMatches(string fileName)
        {
            int count = 0;
            foreach (DrwgNamingFormat dnf in NamingFormats)
            {
                if (dnf.TestFormat(fileName)) count++;
            }
            return count;
        }

        private FileNameFormat DetermineFormat(string fileName)
            => NamingFormats.Where(x => x.TestFormat(fileName)).Select(x => x.Format).FirstOrDefault();

        private void PopulateDrwgData(DrwgNamingFormat Dnf, string FileName)
        {
            DrwgFileNameFormat = Dnf.Message;

            if (Dnf.Format == FileNameFormat.Other)
            {
                DrwgTitleFromFileName = Path.GetFileNameWithoutExtension(FileNameWithPath); //this one is not passed as argument
            }
            else
            {
                Match match = Dnf.Regex.Match(FileName);

                if (Dnf.DrwgNumberFromFileName) DrwgNumberFromFileName = match.Groups[Fields._Number.RegexName].Value;
                //if (Dnf.DrwgNumberFromMeta) DrwgNumberFromMeta = ;
                if (Dnf.DrwgTitleFromFileName) DrwgTitleFromFileName = match.Groups[Fields._Title.RegexName].Value;
                //if (Dnf.DrwgTitleFromMeta) DrwgTitleFromMeta = ;
                if (Dnf.DrwgRevFromFileName) DrwgRevFromFileName = match.Groups[Fields._Revision.RegexName].Value;
                //DrwgRevFromMeta;

                //DrwgScaleFromMeta;
                //DrwgDateFromMeta;
                //DrwgRevDateFromMeta;
            }
        }
    }

    public class DrwgNamingFormat
    {
        public FileNameFormat Format { get; private set; }
        public Regex Regex { get; private set; }
        public string Message { get; private set; }
        public bool TestFormat(string fileName) => this.Regex.IsMatch(fileName);

        public bool DrwgNumberFromFileName { get; private set; }
        public bool DrwgNumberFromMeta { get; private set; }

        public bool DrwgTitleFromFileName { get; private set; }
        public bool DrwgTitleFromMeta { get; private set; }

        public bool DrwgRevFromFileName { get; private set; }
        public bool DrwgRevFromMeta { get; private set; }

        public bool DrwgScaleFromMeta { get; private set; }
        public bool DrwgDateFromMeta { get; private set; }
        public bool DrwgRevDateFromMeta { get; private set; }

        /// <summary>
        /// "Other" is an exception and should be handled separately.
        /// </summary>
        public class Other : DrwgNamingFormat
        {
            public Other()
            {
                Format = FileNameFormat.Other;
                Regex = null;
                Message = "Andet";

                DrwgNumberFromFileName = false;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = false;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public class VeksNoRevision : DrwgNamingFormat
        {
            public VeksNoRevision()
            {
                Format = FileNameFormat.VeksNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "VEKS U. REV";

                DrwgNumberFromFileName = true;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = false;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public class VeksWithRevision : DrwgNamingFormat
        {
            public VeksWithRevision()
            {
                Format = FileNameFormat.VeksWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "VEKS M. REV";

                DrwgNumberFromFileName = true;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = true;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public class DRI_BygNoRevision : DrwgNamingFormat
        {
            public DRI_BygNoRevision()
            {
                Format = FileNameFormat.DRI_BygNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI BYG U. REV";

                DrwgNumberFromFileName = true;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = false;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public class DRI_BygWithRevision : DrwgNamingFormat
        {
            public DRI_BygWithRevision()
            {
                Format = FileNameFormat.DRI_BygWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI BYG M. REV";

                DrwgNumberFromFileName = true;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = true;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public class STD_NoRevision : DrwgNamingFormat
        {
            public STD_NoRevision()
            {
                Format = FileNameFormat.STD_NoRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})\s-\s(?<title>[\p{L}0-9 ,'-]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI STD U. REV";

                DrwgNumberFromFileName = true;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = false;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public class STD_WithRevision : DrwgNamingFormat
        {
            public STD_WithRevision()
            {
                Format = FileNameFormat.STD_WithRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>[\p{L}0-9 ,'-]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI STD M. REV";

                DrwgNumberFromFileName = true;
                DrwgNumberFromMeta = false;

                DrwgTitleFromFileName = true;
                DrwgTitleFromMeta = false;

                DrwgRevFromFileName = true;
                DrwgRevFromMeta = false;

                DrwgScaleFromMeta = false;
                DrwgDateFromMeta = false;
                DrwgRevDateFromMeta = false;
            }
        }

        public List<DrwgNamingFormat> GetDrwgNamingFormatListExceptOther()
        {
            List<DrwgNamingFormat> list = new List<DrwgNamingFormat>();

            //Field is the base class from which are subclasses are derived
            var dnfType = typeof(DrwgNamingFormat);
            //We also need the "Fields" type because it is also a subclas of Field, but should not be in the list
            var otherType = typeof(DrwgNamingFormat.Other);

            var subFieldTypes = dnfType.Assembly.DefinedTypes
                .Where(x => dnfType.IsAssignableFrom(x) && x != dnfType && x != otherType)
                .ToList();

            foreach (var field in subFieldTypes)
                list.Add((DrwgNamingFormat)Activator.CreateInstance(field));

            return list;
        }
    }
    public enum FileNameFormat
    {
        Other,
        VeksNoRevision,
        VeksWithRevision,
        DRI_BygNoRevision,
        DRI_BygWithRevision,
        STD_NoRevision,
        STD_WithRevision
    }

    public enum FieldCat
    {
        None,
        DrawingProperty,
        DataGridViewColumnName,
        FileProperty
    }

    public class Field
    {
        public FieldCat FieldCat { get; private set; }
        public string RegexName { get; private set; } = "";
        public string MetadataName { get; private set; } = "";
        public string ColumnName { get; private set; }
        public string ExcelColumnName { get; set; }
        public int ExcelColumnIdx { get; private set; } = 0;
        public class Number : Field
        {
            public Number()
            {
                FieldCat = FieldCat.DrawingProperty; RegexName = "number"; MetadataName = "DWGNUMBER";
                ColumnName = "Drwg Nr."; ExcelColumnIdx = 1;
            }
        }
        public class Title : Field
        {
            public Title()
            {
                FieldCat = FieldCat.DrawingProperty; RegexName = "title"; MetadataName = "DWGTITLE";
                ColumnName = "Drwg Title"; ExcelColumnIdx = 2;
            }
        }
        public class Revision : Field
        {
            public Revision()
            {
                FieldCat = FieldCat.DrawingProperty; RegexName = "revision"; MetadataName = "DWGREVINDEX";
                ColumnName = "Rev. idx"; ExcelColumnIdx = 5;
            }
        }
        public class Extension : Field
        {
            public Extension() { FieldCat = FieldCat.FileProperty; RegexName = "extension"; }
        }
        public class Scale : Field
        {
            public Scale()
            {
                FieldCat = FieldCat.DrawingProperty; MetadataName = "DWGSCALE";
                ColumnName = "Scale"; ExcelColumnIdx = 3;
            }
        }
        public class Date : Field
        {
            public Date()
            {
                FieldCat = FieldCat.DrawingProperty; MetadataName = "DWGDATE";
                ColumnName = "Date"; ExcelColumnIdx = 4;
            }
        }
        public class RevisionDate : Field
        {
            public RevisionDate()
            {
                FieldCat = FieldCat.DrawingProperty; MetadataName = "DWGREVDATE";
                ColumnName = "Rev. date"; ExcelColumnIdx = 6;
            }
        }
        public class Selected : Field
        {
            public Selected()
            {
                FieldCat = FieldCat.DataGridViewColumnName;
                ColumnName = "Select";
            }
        }
        public class FileNameFormat : Field
        {
            public FileNameFormat()
            {
                FieldCat = FieldCat.DataGridViewColumnName;
                ColumnName = "File name format";
            }
        }
        public class Fields
        {
            //Remember to add new "Field"s here!
            public Field _Number = new Number();
            public Field _Title = new Title();
            public Field _Revision = new Revision();
            public Field _Extension = new Extension();
            public Field _Scale = new Scale();
            public Field _Date = new Date();
            public Field _RevisionDate = new RevisionDate();
            public Field _FileNameFormat = new FileNameFormat();
            public Field _Select = new Selected();
            /// <summary>
            /// Returns the correct Field for the specified columnindex in the excel.
            /// </summary>
            /// <param name="colIdx"></param>
            /// <returns></returns>
            public Field GetExcelColumnField(int colIdx)
            {
                List<Field> FieldsCollection = new List<Field>();

                //Field is the base class from which are subclasses are derived
                var fieldType = typeof(Field);
                //We also need the "Fields" type because it is also a subclas of Field, but should not be in the list
                var fieldsType = typeof(Fields);

                var subFieldTypes = fieldType.Assembly.DefinedTypes
                    .Where(x => fieldType.IsAssignableFrom(x) && x != fieldType && x != fieldsType)
                    .ToList();

                foreach (var field in subFieldTypes)
                    FieldsCollection.Add((Field)Activator.CreateInstance(field));

                return FieldsCollection.Where(x => x.ExcelColumnIdx == colIdx).FirstOrDefault();
            }
        }
    }


}