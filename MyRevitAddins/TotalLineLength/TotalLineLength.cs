using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using MoreLinq;
using System.Text;
using Autodesk.Revit.UI;
using Shared;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using mp = Shared.MyMepUtils;
//using mySettings = GeneralStability.Properties.Settings;

namespace TotalLineLength
{
    public class TotalLineLength
    {
        public static void TotalLineLengths(ExternalCommandData commandData)
        {
            var app = commandData.Application;
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;
            var selection = uiDoc.Selection.GetElementIds();
            var elements = new HashSet<Element>(from ElementId id in selection select doc.GetElement(id));

            double totalLength = 0;

            foreach (Element el in elements)
            {
                if (el == null)
                {
                    ut.ErrorMsg("One of the selected elements is null.");
                    break;
                }

                if (el is DetailCurve)
                {
                    DetailCurve dc = el as DetailCurve;
                    totalLength += dc.GeometryCurve.Length;
                }

                if (el is ModelCurve)
                {
                    ModelCurve mc = el as ModelCurve;
                    totalLength += mc.GeometryCurve.Length;
                }

                else ut.ErrorMsg(el.Name.ToString()+" is not implemented!");
            }

            ut.InfoMsg(totalLength.FtToMillimeters().Round4().ToString());
        }
    }
}

