using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;

namespace MEPUtils.DrawingListManager
{
    public class Drwg
    {
        #region Fields
        string FileNameWithPath;
        internal string FileName;

        internal Guid Id;

        FileNameFormat Format;
        DrwgNamingFormat Dnf;
        //Field.Fields Fields;

        internal DataRow dataRowGV;

        internal DrwgProps.Source_FileName DataFromFileName;
        internal DrwgProps.Source_Excel DataFromExcel;
        internal DrwgProps.Source_Meta DataFromMetadata;

        internal StateFlags State;
        //internal string Extension;
        #endregion

        internal List<DrwgNamingFormat> NamingFormats;
        public Drwg()
        {
            Id = Guid.NewGuid(); //Fields = new Field.Fields();
        }
        public Drwg(string fileNameWithPath) : this()
        {
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
        internal void ReadDrwgDataFromFileName()
        {
            if (Dnf.Format == FileNameFormat.Other)
            {
                DataFromFileName = new DrwgProps.Source_FileName("", Path.GetFileNameWithoutExtension(FileNameWithPath),
                    Dnf.DrwgFileNameFormatDescription, "", "");
            }
            else
            {
                Match match = Dnf.Regex.Match(FileName);
                string number = match.Groups[new Field.Number().RegexName].Value ?? "";
                string title = match.Groups[new Field.Title().RegexName].Value ?? "";
                string revision = match.Groups[new Field.Revision().RegexName].Value ?? "";
                string extension = match.Groups[new Field.Extension().RegexName].Value ?? "";
                DataFromFileName = new DrwgProps.Source_FileName(
                    number, title, Dnf.DrwgFileNameFormatDescription, revision, extension);
            }
        }
        internal void CalculateState()
        {
            StateFlags Calc(string testValue, StateFlags flag)
            {
                if (testValue.IsNullOrEmpty()) return StateFlags.None;
                else return flag;
            }
            StateFlags state = 0;
            state |= Calc(DataFromFileName?.Number?.Value, StateFlags.NumberFromFileName);
            state |= Calc(DataFromExcel?.Number?.Value, StateFlags.NumberFromExcel);
            state |= Calc(DataFromMetadata?.Number?.Value, StateFlags.NumberFromMeta);
            state |= Calc(DataFromFileName?.Title?.Value, StateFlags.TitleFromFileName);
            state |= Calc(DataFromExcel?.Title?.Value, StateFlags.TitleFromExcel);
            state |= Calc(DataFromMetadata?.Title?.Value, StateFlags.TitleFromMeta);
            state |= Calc(DataFromFileName?.Revision?.Value, StateFlags.RevFromFileName);
            state |= Calc(DataFromExcel?.Revision?.Value, StateFlags.RevFromExcel);
            state |= Calc(DataFromMetadata?.Revision?.Value, StateFlags.RevFromMeta);
            state |= Calc(DataFromExcel?.Scale?.Value, StateFlags.ScaleFromExcel);
            state |= Calc(DataFromMetadata?.Scale?.Value, StateFlags.ScaleFromMeta);
            state |= Calc(DataFromExcel?.Date?.Value, StateFlags.DateFromExcel);
            state |= Calc(DataFromMetadata?.Date?.Value, StateFlags.DateFromMeta);
            state |= Calc(DataFromExcel?.RevisionDate?.Value, StateFlags.RevDateFromExcel);
            state |= Calc(DataFromMetadata?.RevisionDate?.Value, StateFlags.RevDateFromMeta);
            State = state;
        }
        internal string TryGetValueOfSpecificPropsField(FieldName fieldName)
        {
            switch (fieldName)
            {
                case FieldName.None:
                    return "";
                case FieldName.Number:
                    return DataFromFileName?.Number?.Value ??
                           DataFromExcel?.Number?.Value ??
                           DataFromMetadata?.Number?.Value ?? "";
                case FieldName.Title:
                    return DataFromFileName?.Title?.Value ??
                           DataFromExcel?.Title?.Value ??
                           DataFromMetadata?.Title?.Value ?? "";
                case FieldName.Revision:
                    return DataFromFileName?.Revision?.Value ??
                           DataFromExcel?.Revision?.Value ??
                           DataFromMetadata?.Revision?.Value ?? "";
                case FieldName.Scale:
                    return DataFromExcel?.Scale?.Value ??
                           DataFromMetadata?.Scale?.Value ?? "";
                case FieldName.Date:
                    return DataFromExcel?.Date?.Value ??
                           DataFromMetadata?.Date?.Value ?? "";
                case FieldName.RevisionDate:
                    return DataFromExcel?.RevisionDate?.Value ??
                           DataFromMetadata?.RevisionDate?.Value ?? "";
                case FieldName.DrawingListCategory:
                    return DataFromMetadata?.DrawingListCategory?.Value ?? "";
                case FieldName.FileNameFormat:
                    return DataFromFileName?.FileNameFormat?.Value ?? "";
                case FieldName.Selected:
                    return "";
                case FieldName.Extension:
                    return DataFromFileName?.Extension?.Value ?? "";
                default:
                    return "";
            }
        }
        internal string GetValue(Source source, FieldName fieldName)
        {
            switch (source)
            {
                case Source.None:
                    return "";
                case Source.Excel:
                    if (DataFromExcel != null) return GetValue(DataFromExcel, fieldName);
                    break;
                case Source.FileName:
                    if (DataFromFileName != null) return GetValue(DataFromFileName, fieldName);
                    break;
                case Source.MetaData:
                    if (DataFromMetadata != null) return GetValue(DataFromMetadata, fieldName);
                    break;
                default:
                    return "";
            }
            return "";
        }
        internal string GetValue(DrwgProps props, FieldName fieldName)
        {

            string fn = fieldName.ToString();
            return (string)props.GetPropertyValue(fn);

            //switch (fieldName)
            //{
            //    case FieldName.None:
            //        return "";
            //case FieldName.Number:
            //    return props.Number.Value;
            //case FieldName.Title:
            //    return props.Title.Value;
            //case FieldName.Revision:
            //    return props.Revision.Value;
            //case FieldName.Scale:
            //    return props.Scale.Value;
            //case FieldName.Date:
            //    return props.Date.Value;
            //case FieldName.RevisionDate:
            //    return props.RevisionDate.Value;
            //case FieldName.DrawingListCategory:
            //    return props.DrawingListCategory.Value;
            //case FieldName.FileNameFormat:
            //    return props.FileNameFormat.Value;
            //case FieldName.Selected:
            //    throw new NotImplementedException();
            //case FieldName.Extension:
            //    return props.FileNameFormat.Value;
            //default:
            //    return "";
            //}
        }
        internal Field GetFieldRef(DrwgProps props, FieldName fieldName)
        {
            string fn = fieldName.ToString();
            return (Field)props.GetPropertyValue(fn);

            //switch (fieldName)
            //{
            //    case FieldName.None:
            //        return new Field.Empty();
            //    case FieldName.Number:
            //        return props.Number;
            //    case FieldName.Title:
            //        return props.Title;
            //    case FieldName.Revision:
            //        return props.Revision;
            //    case FieldName.Scale:
            //        return props.Scale;
            //    case FieldName.Date:
            //        return props.Date;
            //    case FieldName.RevisionDate:
            //        return props.RevisionDate;
            //    case FieldName.DrawingListCategory:
            //        return props.DrawingListCategory;
            //    case FieldName.FileNameFormat:
            //        return props.FileNameFormat;
            //    case FieldName.Selected:
            //        throw new NotImplementedException();
            //    case FieldName.Extension:
            //        return props.FileNameFormat;
            //    default:
            //        return new Field.Empty();
            //}
        }
        internal Field GetFieldRef(Source source, FieldName fieldName)
        {
            switch (source)
            {
                case Source.None:
                    return new Field.Empty();
                case Source.Excel:
                    if (DataFromExcel != null) return GetFieldRef(DataFromExcel, fieldName);
                    break;
                case Source.FileName:
                    if (DataFromFileName != null) return GetFieldRef(DataFromFileName, fieldName);
                    break;
                case Source.MetaData:
                    if (DataFromMetadata != null) return GetFieldRef(DataFromMetadata, fieldName);
                    break;
                default:
                    return new Field.Empty();
            }
            return new Field.Empty();
        }
        internal List<Field> GetAllFieldRefs(FieldName fieldName)
        {
            List<Field> refLst = new List<Field>() { };

            switch (fieldName)
            {
                case FieldName.None:
                    return refLst;
                case FieldName.Number:
                case FieldName.Title:
                case FieldName.Revision:
                    if (DataFromExcel != null) refLst.Add(GetFieldRef(DataFromExcel, fieldName));
                    if (DataFromFileName != null) refLst.Add(GetFieldRef(DataFromFileName, fieldName));
                    if (DataFromMetadata != null) refLst.Add(GetFieldRef(DataFromMetadata, fieldName));
                    break;
                case FieldName.Scale:
                case FieldName.Date:
                case FieldName.RevisionDate:
                    if (DataFromExcel != null) refLst.Add(GetFieldRef(DataFromExcel, fieldName));
                    if (DataFromMetadata != null) refLst.Add(GetFieldRef(DataFromMetadata, fieldName));
                    break;
                case FieldName.DrawingListCategory:
                    if (DataFromMetadata != null) refLst.Add(GetFieldRef(DataFromMetadata, fieldName));
                    break;
                case FieldName.FileNameFormat:
                case FieldName.Extension:
                    if (DataFromFileName != null) refLst.Add(GetFieldRef(DataFromFileName, fieldName));
                    break;
                case FieldName.Selected:
                    return refLst;
                default:
                    return refLst;
            }
            return refLst;
        }
        internal string BuildToolTip(FieldName fieldName)
        {
            List<Field> fLst = GetAllFieldRefs(fieldName);
            List<string> strLst = new List<string>();
            if (fLst.Count > 1)
            {
                foreach (Field field in fLst)
                {
                    strLst.Add(field.TooltipPrefix + field.Value);
                }
                return string.Join("\n", strLst);
            }
            if (fLst.Count == 1)
            {
                foreach (Field field in fLst)
                {
                    strLst.Add(field.TooltipPrefix + field.Value);
                }
                return strLst.First();
            }
            else return "";
        }
        /// <summary>
        /// Used to check if values do really match
        /// </summary>
        internal bool CompareFieldValues(FieldName fieldName)
        {
            List<Field> fLst = GetAllFieldRefs(fieldName);
            if (fLst.Count == 3) return (fLst[0].Value == fLst[1].Value) &&
                                        (fLst[0].Value == fLst[2].Value) &&
                                        (fLst[1].Value == fLst[2].Value);
            if (fLst.Count == 2) return (fLst[0].Value == fLst[1].Value);
            else return false;
        }

        [Flags]
        internal enum StateFlags
        {
            //None should not counted when converting to binary
            //Thus the length of the bit mask is
            //Number of enum flags - 1
            None = 0,
            NumberFromFileName = 1,
            NumberFromExcel = 2,
            NumberFromMeta = 4,
            TitleFromFileName = 8,
            TitleFromExcel = 16,
            TitleFromMeta = 32,
            RevFromFileName = 64,
            RevFromExcel = 128,
            RevFromMeta = 256,
            ScaleFromExcel = 512,
            ScaleFromMeta = 1024,
            DateFromExcel = 2048,
            DateFromMeta = 4096,
            RevDateFromExcel = 8192,
            RevDateFromMeta = 16384
        }
    }
}