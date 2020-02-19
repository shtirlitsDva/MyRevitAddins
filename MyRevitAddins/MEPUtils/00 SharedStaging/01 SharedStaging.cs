using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using MEPUtils.SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace MEPUtils.SharedStaging
{
    public static class ExtensionsStaging
    {
        public static string MEPSystemAbbreviation(this Connector con, Document doc, bool ignoreMepSystemNull = false)
        {
            if (con.MEPSystem != null)
            {
                MEPSystem ps = con.MEPSystem;
                PipingSystemType pst = (PipingSystemType)doc.GetElement(ps.GetTypeId());
                return pst.Abbreviation;
            }
            else if (ignoreMepSystemNull) return "";
            else throw new Exception($"A connector at element {con.Owner.Id.IntegerValue} has MEPSystem = null!");
        }
    }
}
