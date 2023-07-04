using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;

namespace MEPUtils.DrawingListManagerV2
{
    public static class DrawingNamingFormatService
    {
        public static IEnumerable<DrawingNamingFormat> GetDrawingNamingFormatsList()
        {
            //Field is the base class from which are subclasses are derived
            var dnfType = typeof(DrawingNamingFormat);
            //We also need the "Fields" type because it is also a subclas of Field, but should not be in the list
            //var otherType = typeof(DrawingNamingFormat.Other);

            var subFieldTypes = dnfType.Assembly.DefinedTypes
                .Where(x => dnfType.IsAssignableFrom(x) && x != dnfType);// && x != otherType);

            foreach (var field in subFieldTypes)
                yield return (DrawingNamingFormat)Activator.CreateInstance(field);
        }
    }
    public class DrawingNamingFormat
    {
        public FileNameFormat Format { get; private set; }
        public Regex? Regex { get; private set; }
        public string? DrawingFileNameFormatDescription { get; private set; }
        public bool TestFormat(string fileName) => this.Regex.IsMatch(fileName);
        
        /// <summary>
        /// "Other" is an exception and should be handled separately.
        /// </summary>
        public class Other : DrawingNamingFormat
        {
            public Other()
            {
                Format = FileNameFormat.Other;
                Regex = new Regex("Andet");
                DrawingFileNameFormatDescription = "Andet";
            }
        }
        public class VeksNyNoRevision : DrawingNamingFormat
        {
            public VeksNyNoRevision()
            {
                Format = FileNameFormat.VeksNyNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}-\d{3})\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{2,5}$)");
                DrawingFileNameFormatDescription = "VEKS NY U. REV";
            }
        }

        public class VeksNyWithRevision : DrawingNamingFormat
        {
            public VeksNyWithRevision()
            {
                Format = FileNameFormat.VeksNyWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{2,5}$)");
                DrawingFileNameFormatDescription = "VEKS NY M. REV";
            }
        }
        public class VeksNoRevision : DrawingNamingFormat
        {
            public VeksNoRevision()
            {
                Format = FileNameFormat.VeksNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{3,5}$)");
                DrawingFileNameFormatDescription = "VEKS GL U. REV";
            }
        }

        public class VeksWithRevision : DrawingNamingFormat
        {
            public VeksWithRevision()
            {
                Format = FileNameFormat.VeksWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{3,5}$)");
                DrawingFileNameFormatDescription = "VEKS GL M. REV";
            }
        }

        public class DRI_BygNoRevision : DrawingNamingFormat
        {
            public DRI_BygNoRevision()
            {
                Format = FileNameFormat.DRI_BygNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrawingFileNameFormatDescription = "DRI BYG U. REV";
            }
        }

        public class DRI_BygWithRevision : DrawingNamingFormat
        {
            public DRI_BygWithRevision()
            {
                Format = FileNameFormat.DRI_BygWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrawingFileNameFormatDescription = "DRI BYG M. REV";
            }
        }

        public class STD_NoRevision : DrawingNamingFormat
        {
            public STD_NoRevision()
            {
                Format = FileNameFormat.STD_NoRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrawingFileNameFormatDescription = "DRI STD U. REV";
            }
        }

        public class STD_WithRevision : DrawingNamingFormat
        {
            public STD_WithRevision()
            {
                Format = FileNameFormat.STD_WithRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrawingFileNameFormatDescription = "DRI STD M. REV";
            }
        }
    }

    public enum FileNameFormat
    {
        Other,
        VeksNoRevision,
        VeksWithRevision,
        VeksNyNoRevision,
        VeksNyWithRevision,
        DRI_BygNoRevision,
        DRI_BygWithRevision,
        STD_NoRevision,
        STD_WithRevision
    }
}