using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data;

namespace MEPUtils.DrawingListManager
{
    public class DrwgNamingFormat
    {
        public FileNameFormat Format { get; private set; }
        public Regex Regex { get; private set; }
        public string DrwgFileNameFormatDescription { get; private set; }
        public bool TestFormat(string fileName) => this.Regex.IsMatch(fileName);

        /// <summary>
        /// "Other" is an exception and should be handled separately.
        /// </summary>
        public class Other : DrwgNamingFormat
        {
            public Other()
            {
                Format = FileNameFormat.Other;
                Regex = null;
                DrwgFileNameFormatDescription = "Andet";
            }
        }
        public class VeksNyNoRevision : DrwgNamingFormat
        {
            public VeksNyNoRevision()
            {
                Format = FileNameFormat.VeksNyNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}-\d{3})\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{2,5}$)");
                DrwgFileNameFormatDescription = "VEKS NY U. REV";
            }
        }

        public class VeksNyWithRevision : DrwgNamingFormat
        {
            public VeksNyWithRevision()
            {
                Format = FileNameFormat.VeksNyWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{2,5}$)");
                DrwgFileNameFormatDescription = "VEKS NY M. REV";
            }
        }
        public class VeksNoRevision : DrwgNamingFormat
        {
            public VeksNoRevision()
            {
                Format = FileNameFormat.VeksNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{3,5}$)");
                DrwgFileNameFormatDescription = "VEKS GL U. REV";
            }
        }

        public class VeksWithRevision : DrwgNamingFormat
        {
            public VeksWithRevision()
            {
                Format = FileNameFormat.VeksWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{2}-\p{L}{3}\d-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?:\.)(?<extension>[^.]{3,5}$)");
                DrwgFileNameFormatDescription = "VEKS GL M. REV";
            }
        }

        public class DRI_BygNoRevision : DrwgNamingFormat
        {
            public DRI_BygNoRevision()
            {
                Format = FileNameFormat.DRI_BygNoRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrwgFileNameFormatDescription = "DRI BYG U. REV";
            }
        }

        public class DRI_BygWithRevision : DrwgNamingFormat
        {
            public DRI_BygWithRevision()
            {
                Format = FileNameFormat.DRI_BygWithRevision;
                Regex = new Regex(@"(?<number>\d{3}-\d{4}-BYG\d{2})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrwgFileNameFormatDescription = "DRI BYG M. REV";
            }
        }

        public class STD_NoRevision : DrwgNamingFormat
        {
            public STD_NoRevision()
            {
                Format = FileNameFormat.STD_NoRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrwgFileNameFormatDescription = "DRI STD U. REV";
            }
        }

        public class STD_WithRevision : DrwgNamingFormat
        {
            public STD_WithRevision()
            {
                Format = FileNameFormat.STD_WithRevision;
                Regex = new Regex(@"(?<number>STD-\d{3}-\d{3})(?:-)(?<revision>[\p{L}0-9]+)\s-\s(?<title>.+?)(?<extension>\.[^.]{3,5}$)");
                DrwgFileNameFormatDescription = "DRI STD M. REV";
            }
        }

        public List<DrwgNamingFormat> GetDrwgNamingFormatListExceptOther()
        {
            List<DrwgNamingFormat> list = new List<DrwgNamingFormat>();

            //Field is the base class from which are subclasses are derived
            var dnfType = typeof(DrwgNamingFormat);
            //We also need the "Fields" type because it is also a subclas of Field, but should not be in the list
            var otherType = typeof(DrwgNamingFormat.Other);

            var subFieldTypes = dnfType.Assembly.DefinedTypes
                .Where(x => dnfType.IsAssignableFrom(x) && x != dnfType && x != otherType)
                .ToList();

            foreach (var field in subFieldTypes)
                list.Add((DrwgNamingFormat)Activator.CreateInstance(field));

            return list;
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