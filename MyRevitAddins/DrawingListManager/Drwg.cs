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
        Field.Fields Fields;

        internal DataRow dataRowGV;

        internal DrwgProps.Source_FileName DataFromFileName;
        internal DrwgProps.Source_Excel DataFromExcel;
        internal DrwgProps.Source_Meta DataFromMetadata;

        internal StateFlags State;
        internal string Extension;

        //public string DrwgFileNameFormat = string.Empty;
        #endregion

        internal List<DrwgNamingFormat> NamingFormats;
        public Drwg()
        {
            Id = Guid.NewGuid(); Fields = new Field.Fields();
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
                    Dnf.DrwgFileNameFormatDescription, "");
            }
            else
            {
                Match match = Dnf.Regex.Match(FileName);
                string number = match.Groups[Fields._Number.RegexName].Value ?? "";
                string title = match.Groups[Fields._Title.RegexName].Value ?? "";
                string revision = match.Groups[Fields._Revision.RegexName].Value ?? "";
                DataFromFileName = new DrwgProps.Source_FileName(
                    number, title, Dnf.DrwgFileNameFormatDescription, revision);
                Extension = match.Groups[Fields._Extension.RegexName].Value ?? "";
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
        internal string TryGetValueOfSpecificPropsField(Field field)
        {
            switch (field.FieldName)
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
                default:
                    return "";
            }
        }

        internal void ActOnState()
        {
            throw new NotImplementedException();
        }

        [Flags]
        internal enum StateFlags
        {
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

        internal interface IAction
        {
            void Act();
        }

        internal class StateActions
        {
            internal StateFlags AcceptedStates { get; private set; } = StateFlags.None;

            internal class AllDataPresent : StateActions, IAction
            {
                AllDataPresent()
                {
                    AcceptedStates |= (StateFlags)32767;
                }

                public void Act()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}