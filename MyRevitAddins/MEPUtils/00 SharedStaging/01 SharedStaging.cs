using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils._00_SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Input;
using dbg = Shared.Dbg;
using fi = Shared.Filter;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using mp = Shared.MepUtils;
using tr = Shared.Transformation;
using ut = Shared.BuildingCoder.Util;

namespace MEPUtils._00_SharedStaging
{
    public static class Extensions
    {
        public static string MEPSystemAbbreviation(this Connector con, Document doc)
        {
            MEPSystem ps = con.MEPSystem;
            PipingSystemType pst = (PipingSystemType)doc.GetElement(ps.GetTypeId());
            return pst.Abbreviation;
        }
    }

    public static class Comparer
    {
        /// <summary>
        /// Return a hash string for a real number formatted to 3 decimal places.
        /// </summary>
        public static string HashString(double a) => a.ToString("0.###");

        /// <summary>
        /// Return a hash string for an XYZ point or vector with its coordinates
        /// formatted to nine decimal places.
        /// </summary>
        public static string HashString(XYZ p)
        {
            return string.Format("({0},{1},{2})", HashString(p.X), HashString(p.Y), HashString(p.Z));
        }
    }

    public class ConnectorXyzComparer2 : IEqualityComparer<Connector>
    {
        public bool Equals(Connector x, Connector y)
        {
            return null != x && null != y && x.IsEqual(y, 0.00328);
        }

        public int GetHashCode(Connector x) => Comparer.HashString(x.Origin).GetHashCode();
    }

    public static class DoubleExtensions
    {
        public static bool IsEqualTo(this double double1, double double2, double Tolerance = 1.0e-9)
        {
            return Math.Abs(double1 - double2) <= Tolerance;
        }

        public static bool IsEqual(this Connector first, Connector second, double Tolerance = 1.0e-9)
        {

            return first.Origin.X.IsEqualTo(second.Origin.X, Tolerance) &&
                 first.Origin.Y.IsEqualTo(second.Origin.Y, Tolerance) &&
                 first.Origin.Z.IsEqualTo(second.Origin.Z, Tolerance);
        }
    }
}
