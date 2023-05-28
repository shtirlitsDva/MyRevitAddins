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
        
        private Dictionary<DrawingInfoPropsEnum, string> ReleasedData { get; }
        private Dictionary<DrawingInfoPropsEnum, string> StagingData { get; }
        private Dictionary<DrawingInfoPropsEnum, string> MetaData { get; }
        private Dictionary<DrawingInfoPropsEnum, string> ExcelData { get; }
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
                default:
                case DrawingInfoTypeEnum.Unknown:
                    throw new Exception(
                        $"Unknown DrawingInfoTypeEnum encountered for {fileNameWithPath}");
                case DrawingInfoTypeEnum.Released:
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
                        MetaData = MetadataReaderService.ReadData(fileNameWithPath);
                    }
                    break;
                case DrawingInfoTypeEnum.Staging:
                    {
                        if (DrawingNamingFormat.Format == FileNameFormat.Other)
                        {
                            StagingData = new Dictionary<DrawingInfoPropsEnum, string>
                            {
                                { DrawingInfoPropsEnum.Title, FileName }
                            };
                        }
                        else
                        {
                            StagingData = FileNameDataReaderService.ReadData(
                                FileName, DrawingNamingFormat);
                        }
                        //Try to read pdf metadata
                        MetaData = MetadataReaderService.ReadData(fileNameWithPath);
                    }
                    break;
                case DrawingInfoTypeEnum.DrawingList:
                    {
                        throw new Exception("Wrong DrawingInfo constructor for Excel Data!");
                    }
            }
        }

        public DrawingInfo(Dictionary<DrawingInfoPropsEnum, string> dict, DrawingInfoTypeEnum drawingType)
        {
            ExcelData = dict;
            DrawingType = drawingType;
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