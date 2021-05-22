using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using System.Security;
using System.Reflection;

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
        public bool IsMetaDataField { get { return !MetadataName.IsNoE(); } }
        public bool IsFileAndExcelField { get; set; } = false;//Used to limit the analysis
        public string Value { get; private set; } = "";
        public static bool operator ==(Field f1, Field f2) => f1.Value == f2.Value;
        public static bool operator !=(Field f1, Field f2) => f1.Value != f2.Value;
        public DrwgProps PropsRef { get; private set; }
        public string TooltipPrefix { get { return PropsRef.ToolTipPrefix; } }
        public void SetValue(string value, DrwgProps props) { Value = value; PropsRef = props; }
        public Field SetValueRef(string value, DrwgProps props)
        {
            Value = value;
            PropsRef = props;
            return this;
        }
        public class Empty : Field
        {
        }
        public class Number : Field
        {
            public Number()
            {
                this.FieldName = FieldName.Number; RegexName = "number"; MetadataName = "DWGNUMBER";
                ColumnName = "Drwg Nr."; ExcelColumnIdx = 1; IsFileAndExcelField = true;
            }
            public Number(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Title : Field
        {
            public Title()
            {
                this.FieldName = FieldName.Title;
                RegexName = "title"; MetadataName = "DWGTITLE";
                ColumnName = "Drwg Title"; ExcelColumnIdx = 2; IsFileAndExcelField = true;
            }
            public Title(string value, DrwgProps props) : this() { Value = value; PropsRef = props; }
        }
        public class Revision : Field
        {
            public Revision()
            {
                this.FieldName = FieldName.Revision;
                RegexName = "revision"; MetadataName = "DWGREVINDEX";
                ColumnName = "Rev. idx"; ExcelColumnIdx = 5; IsFileAndExcelField = true;
            }
            public Revision(string value, DrwgProps props) : this()
            { Value = value; PropsRef = props; }
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
            public Field Number { get; private set; } = new Number();
            public Field Title { get; private set; } = new Title();
            public Field Revision { get; private set; } = new Revision();
            public Field Extension { get; private set; } = new Extension();
            public Field Scale { get; private set; } = new Scale();
            public Field Date { get; private set; } = new Date();
            public Field RevisionDate { get; private set; } = new RevisionDate();
            public Field FileNameFormat { get; private set; } = new FileNameFormat();
            public Field Select { get; private set; } = new Selected();
            public Field DrawingListCategory { get; private set; } = new DrawingListCategory();

            public HashSet<Field> GetAllFields()
            {
                HashSet<Field> FieldsCollection = new HashSet<Field>();

                //Field is the base class from which are subclasses are derived
                var fieldType = typeof(Field);
                //We also need the "Fields" type because it is also a subclas of Field, but should not be in the list
                var fieldsType = typeof(Fields);
                //Empty field is not needed?
                var emptyType = typeof(Field.Empty);

                var subFieldTypes = fieldType.Assembly.DefinedTypes
                    .Where(x => fieldType.IsAssignableFrom(x) && x != fieldType && x != fieldsType && x != emptyType)
                    .ToList();

                foreach (var field in subFieldTypes)
                    FieldsCollection.Add((Field)Activator.CreateInstance(field));

                return FieldsCollection;
            }
            public Field GetField(FieldName fieldName) =>
                            GetAllFields().Where(x => x.FieldName == fieldName).FirstOrDefault();

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
        MetaData,
        Staging
    }

    public class DrwgProps
    {
        internal Source Source;
        public Field.Number Number { get; internal set; } = new Field.Number();
        public Field.Title Title { get; internal set; }
        public Field.Revision Revision { get; internal set; }
        public Field.Scale Scale { get; internal set; }
        public Field.Date Date { get; internal set; }
        public Field.RevisionDate RevisionDate { get; internal set; }
        public Field.DrawingListCategory DrawingListCategory { get; internal set; }
        public Field.FileNameFormat FileNameFormat { get; internal set; }
        public Field.Extension Extension { get; internal set; }
        public string ToolTipPrefix { get; internal set; } = string.Empty;
        
        public void TrySetField(FieldName fieldName, string Value)
        {

        }
        public class Source_FileName : DrwgProps
        {
            public Source_FileName(
                string number, string title, string fileNameFormat,
                string revision, string extension)
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
                string number, string title, string revision, string scale,
                string date, string revisionDate)
            {
                Source = Source.Excel;
                Number = new Field.Number(number, this);
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
            public Source_Meta() { Source = Source.MetaData; ToolTipPrefix = "Metadata: "; }
            public Source_Meta(bool initEmpty) : this()
            {
                Number = new Field.Number();
                Title = new Field.Title();
                Scale = new Field.Scale();
                Date = new Field.Date();
                Revision = new Field.Revision();
                RevisionDate = new Field.RevisionDate();
                DrawingListCategory = new Field.DrawingListCategory();
            }
            public Source_Meta(
                string number, string title, string revision, string scale,
                string date, string revDate, string drwgLstCat) : this()
            {
                Number = new Field.Number(number, this);
                Title = new Field.Title(title, this);
                Scale = new Field.Scale(scale, this);
                Date = new Field.Date(date, this);
                Revision = new Field.Revision(revision, this);
                RevisionDate = new Field.RevisionDate(revDate, this);
                DrawingListCategory = new Field.DrawingListCategory(drwgLstCat, this);
            }
        }

        public class Source_Staging : DrwgProps
        {
            public Source_Staging() { Source = Source.Staging; ToolTipPrefix = "Staging: "; }
            public Source_Staging(string number, string title, string revision, string extension) : this()
            {
                Number = new Field.Number(number, this);
                Title = new Field.Title(title, this);
                Revision = new Field.Revision(revision, this);
                Extension = new Field.Extension(extension, this);
            }
            public Source_Staging(string number, string title, string revision, string extension,
                string scale, string date, string revDate, string drwgLstCat) :
                this(number, title, revision, extension)
            {
                Scale = new Field.Scale(scale, this);
                Date = new Field.Date(date, this);
                RevisionDate = new Field.RevisionDate(revDate, this);
                DrawingListCategory = new Field.DrawingListCategory(drwgLstCat, this);
            }
        }


    }
}