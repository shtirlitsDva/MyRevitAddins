using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Security;
using System.Reflection;

namespace MEPUtils.DrawingListManagerV2
{
    public class Field
    {
        public PropertiesEnum PropertyName { get; private set; } = PropertiesEnum.Unknown;
        public string RegexName { get; private set; } = "";
        public string MetadataName { get; private set; } = "";
        public string ColumnName { get; private set; } = "";
        public bool IsExcelField { get { return ExcelColumnIdx > 0; } }
        public int ExcelColumnIdx { get; private set; } = 0;
        public bool IsMetaDataField { get { return !MetadataName.IsNoE(); } }
        public bool IsFileAndExcelField { get; set; } = false;//Used to limit the analysis
        public class Empty : Field
        {
        }
        public class Number : Field
        {
            public Number()
            {
                this.PropertyName = PropertiesEnum.Number; RegexName = "number"; MetadataName = "DWGNUMBER";
                ColumnName = "Drwg Nr."; ExcelColumnIdx = 1; IsFileAndExcelField = true;
            }
        }
        public class Title : Field
        {
            public Title()
            {
                this.PropertyName = PropertiesEnum.Title;
                RegexName = "title"; MetadataName = "DWGTITLE";
                ColumnName = "Drwg Title"; ExcelColumnIdx = 2; IsFileAndExcelField = true;
            }
        }
        public class Revision : Field
        {
            public Revision()
            {
                this.PropertyName = PropertiesEnum.Revision;
                RegexName = "revision"; MetadataName = "DWGREVINDEX";
                ColumnName = "Rev. idx"; ExcelColumnIdx = 5; IsFileAndExcelField = true;
            }
        }
        public class Scale : Field
        {
            public Scale()
            {
                this.PropertyName = PropertiesEnum.Scale;
                MetadataName = "DWGSCALE"; ColumnName = "Scale"; ExcelColumnIdx = 3;
            }
        }
        public class Date : Field
        {
            public Date()
            {
                this.PropertyName = PropertiesEnum.Date;
                MetadataName = "DWGDATE"; ColumnName = "Date"; ExcelColumnIdx = 4;
            }
        }
        public class RevisionDate : Field
        {
            public RevisionDate()
            {
                this.PropertyName = PropertiesEnum.RevisionDate;
                MetadataName = "DWGREVDATE"; ColumnName = "Rev. date"; ExcelColumnIdx = 6;
            }
        }
        public class Fields
        {
            //Remember to add new "Field"s here!
            public Field Number { get; private set; } = new Number();
            public Field Title { get; private set; } = new Title();
            public Field Revision { get; private set; } = new Revision();
            public Field Scale { get; private set; } = new Scale();
            public Field Date { get; private set; } = new Date();
            public Field RevisionDate { get; private set; } = new RevisionDate();

            public IEnumerable<Field> GetAllFields()
            {
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
                    yield return (Field)Activator.CreateInstance(field);
            }
            public Field GetField(PropertiesEnum propertyName) =>
                GetAllFields().Where(x => x.PropertyName == propertyName).FirstOrDefault();

            /// <summary>
            /// Returns the correct Field for the specified columnindex in the excel.
            /// </summary>
            /// <param name="colIdx">Index of column.</param>
            public Field GetExcelColumnField(int colIdx) =>
                new Fields().GetAllFields().Where(x => x.ExcelColumnIdx == colIdx).FirstOrDefault();
        }
    }
}