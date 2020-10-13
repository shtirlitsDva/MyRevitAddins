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
            DataRow row;

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "Drwg Nr.";
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "Drwg Title";
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "File name format";
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "Scale";
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "Date";
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "Rev. idx";
            Data.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("String");
            column.ColumnName = "Rev. date";
            Data.Columns.Add(column);
            #endregion
        }

        private void PopulateDataTable()
        {
            foreach (string fileNameWithPath in drwgFileNameList)
            {
                Drwg drwg = new Drwg(fileNameWithPath);
            }
        }
    }

    public class Drwg
    {
        string FileNameWithPath;
        string FileName;

        FileNameFormat Format;
        DrwgNamingFormat Dnf;

        string DrwgNumberFromFileName;
        string DrwgNumberFromMeta;

        string DrwgTitleFromFileName;
        string DrwgTitleFromMeta;

        string DrwgRevFromFileName;
        string DrwgRevFromMeta;

        string DrwgScaleFromMeta;
        string DrwgDateFromMeta;
        string DrwgRevDateFromMeta;

        private List<DrwgNamingFormat> NamingFormats = new List<DrwgNamingFormat>
                                    {  new DrwgNamingFormat.VeksNoRevision(), new DrwgNamingFormat.VeksWithRevision(),
                                       new DrwgNamingFormat.DRI_BygNoRevision(), new DrwgNamingFormat.DRI_BygWithRevision(),
                                       new DrwgNamingFormat.STD_NoRevision(), new DrwgNamingFormat.STD_WithRevision() };

        public Drwg(string fileNameWithPath)
        {
            FileNameWithPath = fileNameWithPath;
            FileName = Path.GetFileName(FileNameWithPath);

            //Test to see if there are multiple matches for filenames -> meaning multiple regex matches -> should be only one match
            if (TestFormatsForMultipleMatches(FileName) > 1) throw
                      new Exception($"Filename {FileName} matched multiple Regex patterns! Must only match one!");

            //Analyze file name and find the format
            Format = DetermineFormat(FileName);

            //Find the correct analyzing format
            if (Format == FileNameFormat.Other) return;
            else Dnf = NamingFormats.Where(x => x.Format == Format).FirstOrDefault();
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
        

    }

    public class DrwgNamingFormat
    {
        public FileNameFormat Format { get; private set; }
        public Regex Regex { get; private set; }
        public string Message { get; private set; }
        public List<Field> ActiveFields { get; private set; }
        public bool TestFormat(string fileName) => this.Regex.IsMatch(fileName);

        public class VeksNoRevision : DrwgNamingFormat
        {
            public VeksNoRevision()
            {
                Format = FileNameFormat.VeksNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "VEKS U. REV";
                ActiveFields = new List<Field> { new Number(), new Title() };
            }
        }

        public class VeksWithRevision : DrwgNamingFormat
        {
            public VeksWithRevision()
            {
                Format = FileNameFormat.VeksWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "VEKS M. REV";
                ActiveFields = new List<Field> { new Number(), new Revision(), new Title() };
            }
        }

        public class DRI_BygNoRevision : DrwgNamingFormat
        {
            public DRI_BygNoRevision()
            {
                Format = FileNameFormat.DRI_BygNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI BYG U. REV";
                ActiveFields = new List<Field> { new Number(), new Title() };
            }
        }

        public class DRI_BygWithRevision : DrwgNamingFormat
        {
            public DRI_BygWithRevision()
            {
                Format = FileNameFormat.DRI_BygWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>[\p{L}0-9 -]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI BYG M. REV";
                ActiveFields = new List<Field> { new Number(), new Revision(), new Title() };
            }
        }

        public class STD_NoRevision : DrwgNamingFormat
        {
            public STD_NoRevision()
            {
                Format = FileNameFormat.STD_NoRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})\s-\s(?<title>[\p{L}0-9 ,'-]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI STD U. REV";
                ActiveFields = new List<Field> { new Number(), new Title() };
            }
        }

        public class STD_WithRevision : DrwgNamingFormat
        {
            public STD_WithRevision()
            {
                Format = FileNameFormat.STD_WithRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>[\p{L}0-9 ,'-]*)(?<extension>.[\p{L}0-9 -]*)");
                Message = "DRI STD M. REV";
                ActiveFields = new List<Field> { new Number(), new Revision(), new Title() };
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
        Extension
    }

    public abstract class Field
    {
        public FieldCat FieldCat { get; set; }
        public string RegexName { get; set; }
        public string MetadataName { get; set; }
    }
    public class Number : Field
    {
        public Number() { FieldCat = FieldCat.Number; RegexName = "number"; MetadataName = "DWGNUMBER"; }
    }

    public class Title : Field
    {
        public Title() { FieldCat = FieldCat.Title; RegexName = "title"; MetadataName = "DWGTITLE"; }
    }

    public class Revision : Field
    {
        public Revision() { FieldCat = FieldCat.Revision; RegexName = "revision"; MetadataName = "DWGREVINDEX"; }
    }

    public class Extension : Field
    {
        public Extension() { FieldCat = FieldCat.Extension; RegexName = "extension"; MetadataName = ""; }
    }

    public class Scale : Field
    {
        public Scale() { FieldCat = FieldCat.Scale; RegexName = ""; MetadataName = "DWGSCALE"; }
    }

    public class Date : Field
    {
        public Date() { FieldCat = FieldCat.Date; RegexName = ""; MetadataName = "DWGDATE"; }
    }

    public class RevisionDate : Field
    {
        public RevisionDate() { FieldCat = FieldCat.RevisionDate; RegexName = ""; MetadataName = "DWGREVDATE"; }
    }
}

