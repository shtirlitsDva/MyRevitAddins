using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Mechanical;
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

namespace MEPUtils._00_SharedStaging
{
    public static class Extensions
    {
        /// <summary>
        /// Returns, for fittings only, the PartType of the element in question.
        /// </summary>
        /// <param name="e">Element to get the PartType property.</param>
        /// <returns>The PartType of the passed element.</returns>
        public static PartType MechFittingPartType(this Element e)
        {
            if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                var mf = ((FamilyInstance)e).MEPModel as MechanicalFitting;
                return mf.PartType;
            }
            else return PartType.Undefined;
        }
    }
}
