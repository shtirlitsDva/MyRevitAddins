using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DrawingListManagerForm());
        }
    }

    public class DrwgLstMan
    {
        public List<string> drwgFileNameList;
        public List<Drwg> drwgList;
        public DataTable Data;
        private Field.Fields fs = new Field.Fields();

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
            Data = new DataTable("DrwgFileData");

            #region DataTable Definition
            DataColumn column;
            
            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Number.ColumnName;
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Title.ColumnName;
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._FileNameFormat.ColumnName;
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Scale.ColumnName;
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Date.ColumnName;
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._Revision.ColumnName;
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = typeof(string);
            column.ColumnName = fs._RevisionDate.ColumnName;
            Data.Columns.Add(column);
            #endregion
        }

        private void PopulateDataTable()
        {
            foreach (string fileNameWithPath in drwgFileNameList)
            {
                Drwg drwg = new Drwg(fileNameWithPath);
                DataRow row = Data.NewRow();

                row[fs._Number.ColumnName] = drwg.DrwgNumberFromFileName;
                row[fs._Title.ColumnName] = drwg.DrwgTitleFromFileName;
                row[fs._FileNameFormat.ColumnName] = drwg.DrwgFileNameFormat;
                row[fs._Revision.ColumnName] = drwg.DrwgRevFromFileName;

                Data.Rows.Add(row);
            }

            Data.AcceptChanges();
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

        private List<DrwgNamingFormat> NamingFormats = new List<DrwgNamingFormat> //Except Other format -- it must not be included
                                    {  new DrwgNamingFormat.VeksNoRevision(), new DrwgNamingFormat.VeksWithRevision(),
                                       new DrwgNamingFormat.DRI_BygNoRevision(), new DrwgNamingFormat.DRI_BygWithRevision(),
                                       new DrwgNamingFormat.STD_NoRevision(), new DrwgNamingFormat.STD_WithRevision() };

        public Drwg(string fileNameWithPath)
        {
            Fields = new Field.Fields();

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

        private FileNameFormat DetermineFormat(string fileName) =>
            NamingFormats.Where(x => x.TestFormat(fileName)).Select(x => x.Format).FirstOrDefault();

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
        Number,
        Title,
        Revision,
        Scale,
        Date,
        RevisionDate,
        Extension,
        FileNameFormat
    }

    public class Field
    {
        public FieldCat FieldCat { get; private set; }
        public string RegexName { get; private set; }
        public string MetadataName { get; private set; }
        public string ColumnName { get; private set; }
        public class Number : Field
        {
            public Number()
            {
                FieldCat = FieldCat.Number; RegexName = "number"; MetadataName = "DWGNUMBER";
                ColumnName = "Drwg Nr.";
            }
        }
        public class Title : Field
        {
            public Title()
            {
                FieldCat = FieldCat.Title; RegexName = "title"; MetadataName = "DWGTITLE";
                ColumnName = "Drwg Title";
            }
        }
        public class Revision : Field
        { public Revision() { FieldCat = FieldCat.Revision; RegexName = "revision"; MetadataName = "DWGREVINDEX";
                ColumnName = "Rev. idx"; }
        }
        public class Extension : Field
        {
            public Extension() { FieldCat = FieldCat.Extension; RegexName = "extension"; MetadataName = ""; }
        }
        public class Scale : Field
        {
            public Scale() { FieldCat = FieldCat.Scale; RegexName = ""; MetadataName = "DWGSCALE";
                ColumnName = "Scale"; }
        }
        public class Date : Field
        { public Date() { FieldCat = FieldCat.Date; RegexName = ""; MetadataName = "DWGDATE";
                ColumnName = "Date"; }
        }
        public class RevisionDate : Field
        { public RevisionDate() { FieldCat = FieldCat.RevisionDate; RegexName = ""; MetadataName = "DWGREVDATE";
                ColumnName = "Rev. date"; }
        }
        public class FileNameFormat : Field
        {
            public FileNameFormat()
            {
                FieldCat = FieldCat.FileNameFormat; RegexName = ""; MetadataName = "";
                ColumnName = "File name format";
            }
        }
        public class Fields
        {
            public Field _Number = new Number();
            public Field _Title = new Title();
            public Field _Revision = new Revision();
            public Field _Extension = new Extension();
            public Field _Scale = new Scale();
            public Field _Date = new Date();
            public Field _RevisionDate = new RevisionDate();
            public Field _FileNameFormat = new FileNameFormat();
        }
    }

    
}