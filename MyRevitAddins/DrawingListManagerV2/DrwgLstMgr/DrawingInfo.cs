using System;
using System.Collections.Generic;
using System.Linq;
//using MoreLinq;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.Net;

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

            switch (drawingType)
            {
                case DrawingInfoTypeEnum.Unknown:
                    throw new Exception(
                        $"Unknown DrawingInfoTypeEnum encountered for {fileNameWithPath}");
                case DrawingInfoTypeEnum.Released:
                case DrawingInfoTypeEnum.Staging:
                    {
                        if (DrawingNamingFormat.Format == FileNameFormat.Other)
                        {
                            ReleasedData = new Dictionary<DrawingInfoPropsEnum, string>
                            {
                                { DrawingInfoPropsEnum.Title, FileName }
                            };
                        }
                        else
                        {
                            ReleasedData = FileNameDataReaderService.ReadData(
                                FileName, DrawingNamingFormat);
                        }
                        //Try to read pdf metadata


                    }
                
                    break;
                case DrawingInfoTypeEnum.DrawingList:
                    break;
                default:
                    break;
            }
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
            if (DrawingNamingFormat.Format == FileNameFormat.Other)
            {
            }
            else
            {
                
            }
        }
        
    }

    public enum DrawingInfoPropsEnum
    {
        Unknown,
        Number,
        Title,
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