using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace MEPUtils.DrawingListManagerV2
{
    public class DrawingInfo
    {
        #region Fields
        public DrawingInfoTypeEnum DrawingType { get; set; }
        public string FileNameWithPath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }

        FileNameFormat Format;
        DrawingNamingFormat DrawingNamingFormat;

        private Dictionary<DrawingInfoPropsEnum, string> ReleasedData;
        private Dictionary<DrawingInfoPropsEnum, string> StagingData;
        private Dictionary<DrawingInfoPropsEnum, string> MetaData;
        private Dictionary<DrawingInfoPropsEnum, string> ExcelData;
        #endregion

        public DrawingInfo(string fileNameWithPath, DrawingInfoTypeEnum drawingType)
        {
            DrawingType = drawingType;
            FileNameWithPath = fileNameWithPath;
            FileName = Path.GetFileName(fileNameWithPath);
            Extension = Path.GetExtension(fileNameWithPath);

            //Test to see if there are multiple matches for filenames -> meaning multiple regex matches -> should be only one match
            if (TestFormatsForMultipleMatches(FileName) > 1) throw
                      new Exception($"Filename {FileName} matched multiple Regex patterns! Must only match one!");

            //Analyze file name and find the format
            var result = DetermineFormat(FileName);

            Format = result.fnf;
            DrawingNamingFormat = result.dnf;
        }
        private int TestFormatsForMultipleMatches(string fileName)
        {
            int count = 0;
            foreach (DrawingNamingFormat dnf in 
                DrawingNamingFormatService.GetDrawingNamingFormatsList())
            {
                if (dnf.TestFormat(fileName)) count++;
            }
            return count;
        }
        private (FileNameFormat fnf, DrawingNamingFormat dnf) DetermineFormat(string fileName)
            => DrawingNamingFormatService.GetDrawingNamingFormatsList()
            .Where(x => x.TestFormat(fileName)).Select(x => (x.Format, x)).FirstOrDefault();
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
        
    }

    public enum DrawingInfoPropsEnum
    {
        Unknown,
        Number,
        Name,
        Revision,
        Scale,
        Date,
        RevisionDate
    }
    public enum DrawingInfoTypeEnum
    {
        Unknown,
        Released,
        Staging,
        DrawingList
    }
}