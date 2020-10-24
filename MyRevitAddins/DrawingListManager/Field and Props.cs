using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using System.Security;

namespace MEPUtils.DrawingListManager
{
    public class Field
    {
        public FieldName FieldName { get; private set; } = FieldName.None;
        public string RegexName { get; private set; } = "";
        public string MetadataName { get; private set; } = "";
        public string ColumnName { get; private set; } = "";
        public bool IsExcelField { get { return ExcelColumnIdx > 0; } }
        public int ExcelColumnIdx { get; private set; } = 0;
        public bool IsMetaDataField { get { return !MetadataName.IsNullOrEmpty(); } }
        public string Value { get; private set; } = "";
        public static bool operator ==(Field f1, Field f2) => f1.Value == f2.Value;
        public static bool operator !=(Field f1, Field f2) => f1.Value != f2.Value;
        public DrwgProps PropsRef { get; private set; }
        public string TooltipPrefix { get { return PropsRef.ToolTipPrefix; } }
        public class Empty : Field
        {
        }
        public class Number : Field
        {
            public Number()
            {
                this.FieldName = FieldName.Number; RegexName = "number"; MetadataName = "DWGNUMBER";
                ColumnName = "Drwg Nr."; ExcelColumnIdx = 1;
            }
            public Number(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Title : Field
        {
            public Title()
            {
                this.FieldName = FieldName.Title;
                RegexName = "title"; MetadataName = "DWGTITLE";
                ColumnName = "Drwg Title"; ExcelColumnIdx = 2;
            }
            public Title(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Revision : Field
        {
            public Revision()
            {
                this.FieldName = FieldName.Revision;
                RegexName = "revision"; MetadataName = "DWGREVINDEX";
                ColumnName = "Rev. idx"; ExcelColumnIdx = 5;
            }
            public Revision(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Extension : Field
        {
            public Extension() { this.FieldName = FieldName.Extension; RegexName = "extension"; ColumnName = "Ext."; }
            public Extension(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Scale : Field
        {
            public Scale()
            {
                this.FieldName = FieldName.Scale;
                MetadataName = "DWGSCALE"; ColumnName = "Scale"; ExcelColumnIdx = 3;
            }
            public Scale(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Date : Field
        {
            public Date()
            {
                this.FieldName = FieldName.Date;
                MetadataName = "DWGDATE"; ColumnName = "Date"; ExcelColumnIdx = 4;
            }
            public Date(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class RevisionDate : Field
        {
            public RevisionDate()
            {
                this.FieldName = FieldName.RevisionDate;
                MetadataName = "DWGREVDATE"; ColumnName = "Rev. date"; ExcelColumnIdx = 6;
            }
            public RevisionDate(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class DrawingListCategory : Field
        {
            public DrawingListCategory()
            {
                this.FieldName = FieldName.DrawingListCategory;
                MetadataName = "DWGLSTCAT"; ColumnName = "DrwgLstCategory";
            }
            public DrawingListCategory(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Selected : Field
        {
            public Selected()
            {
                this.FieldName = FieldName.Selected; ColumnName = "Select";
            }
        }
        public class FileNameFormat : Field
        {
            public FileNameFormat()
            {
                this.FieldName = FieldName.FileNameFormat;
                ColumnName = "File name format";
            }
            public FileNameFormat(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
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
            public Field _DrawingListCategory = new DrawingListCategory();

            public HashSet<Field> GetAllFields()
            {
                HashSet<Field> FieldsCollection = new HashSet<Field>();

                //Field is the base class from which are subclasses are derived
                var fieldType = typeof(Field);
                //We also need the "Fields" type because it is also a subclas of Field, but should not be in the list
                var fieldsType = typeof(Fields);

                var subFieldTypes = fieldType.Assembly.DefinedTypes
                    .Where(x => fieldType.IsAssignableFrom(x) && x != fieldType && x != fieldsType)
                    .ToList();

                foreach (var field in subFieldTypes)
                    FieldsCollection.Add((Field)Activator.CreateInstance(field));

                return FieldsCollection;
            }
            /// <summary>
            /// Returns the correct Field for the specified columnindex in the excel.
            /// </summary>
            /// <param name="colIdx">Index of column.</param>
            public Field GetExcelColumnField(int colIdx) =>
                new Fields().GetAllFields().Where(x => x.ExcelColumnIdx == colIdx).FirstOrDefault();
        }
    }

    public enum FieldName
    {
        None,
        Number,
        Title,
        Revision,
        Scale,
        Date,
        RevisionDate,
        DrawingListCategory,
        FileNameFormat,
        Selected,
        Extension
    }
    public enum Source
    {
        None,
        Excel,
        FileName,
        MetaData
    }

    public class DrwgProps
    {
        internal Source Source;
        public Field.Number Number { get; private set; }
        public Field.Title Title { get; private set; }
        public Field.Revision Revision { get; private set; }
        public Field.Scale Scale { get; private set; }
        public Field.Date Date { get; private set; }
        public Field.RevisionDate RevisionDate { get; private set; }
        public Field.DrawingListCategory DrawingListCategory { get; private set; }
        public Field.FileNameFormat FileNameFormat { get; private set; }
        public Field.Extension Extension { get; private set; }
        public string ToolTipPrefix { get; private set; } = string.Empty;
        public class Source_FileName : DrwgProps
        {
            public Source_FileName(string number, string title, string fileNameFormat, string revision, string extension)
            {
                Source = Source.FileName;
                Number = new Field.Number(number, this);
                Title = new Field.Title(title, this);
                Revision = new Field.Revision(revision, this);
                FileNameFormat = new Field.FileNameFormat(fileNameFormat, this);
                Extension = new Field.Extension(extension, this);
                ToolTipPrefix = "Filename: ";
            }
        }

        public class Source_Excel : DrwgProps
        {
            public Source_Excel(
                string number, string title, string revision, string scale, string date, string revisionDate)
            {
                Source = Source.Excel;
                Number = new Field.Number(number,this);
                Title = new Field.Title(title, this);
                Scale = new Field.Scale(scale, this);
                Date = new Field.Date(date, this);
                Revision = new Field.Revision(revision, this);
                RevisionDate = new Field.RevisionDate(revisionDate, this);
                ToolTipPrefix = "Excel: ";
            }
        }

        public class Source_Meta : DrwgProps
        {
            public Source_Meta() { }
            public Source_Meta(
                string number, string title, string revision, string scale, string date, string revDate, string drwgLstCat)
            {
                Source = Source.MetaData;
                Number = new Field.Number(number, this);
                Title = new Field.Title(title, this);
                Scale = new Field.Scale(scale, this);
                Date = new Field.Date(date, this);
                Revision = new Field.Revision(revision, this);
                RevisionDate = new Field.RevisionDate(revDate, this);
                DrawingListCategory = new Field.DrawingListCategory(drwgLstCat, this);
                ToolTipPrefix = "Metadata: ";
            }
        }
    }
}