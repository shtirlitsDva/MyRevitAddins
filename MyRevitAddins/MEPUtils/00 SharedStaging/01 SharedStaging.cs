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

        /// <summary>
        /// Method is taken from here:
        /// https://spiderinnet.typepad.com/blog/2011/08/revit-parameter-api-asvaluestring-tostring-tovaluestring-and-tovaluedisplaystring.html
        /// </summary>
        /// <param name="p">Revit parameter</param>
        /// <returns>Stringified contents of the parameter</returns>
        internal static string ToValueString(this Autodesk.Revit.DB.Parameter p)
        {
            string ret = string.Empty;

            switch (p.StorageType)
            {
                case StorageType.ElementId:
                    ret = p.AsElementId().ToString();
                    break;
                case StorageType.Integer:
                    ret = p.AsInteger().ToString();
                    break;
                case StorageType.String:
                    ret = p.AsString();
                    break;
                case StorageType.Double:
                    ret = p.AsValueString();
                    break;
                default:
                    break;
            }

            return ret;
        }

    }
}
