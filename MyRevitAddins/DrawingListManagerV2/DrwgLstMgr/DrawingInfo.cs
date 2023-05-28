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

        private FileNameFormat Format;
        private DrawingNamingFormat DrawingNamingFormat;

        private Dictionary<PropertiesEnum, string> PropertyData;
        private Dictionary<PropertiesEnum, string> MetaData;
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
                case DrawingInfoTypeEnum.Staging:
                    {
                        if (DrawingNamingFormat.Format == FileNameFormat.Other)
                        {
                            PropertyData = new Dictionary<PropertiesEnum, string>
                            {
                                { PropertiesEnum.Title, FileName }
                            };
                        }
                        else
                        {
                            PropertyData = FileNameDataReaderService.ReadData(
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

        public DrawingInfo(Dictionary<PropertiesEnum, string> dict, DrawingInfoTypeEnum drawingType)
        {
            PropertyData = dict;
            DrawingType = drawingType;
        }

        public string GetPropertyValue(PropertiesEnum prop) =>
            PropertyData.ContainsKey(prop) ? PropertyData[prop] : "";
        public string GetMetadataValue(PropertiesEnum prop) =>
            MetaData.ContainsKey(prop) ? MetaData[prop] : "";

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

    public enum PropertiesEnum
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