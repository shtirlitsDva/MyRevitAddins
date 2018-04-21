using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace MEPUtils
{
    public static class PDFExporter
    {
        public static Result ExportPDF(ExternalCommandData cData)
        {
            try
            {
                PDFExporterForm ef = new PDFExporterForm(cData);
                ef.ShowDialog();

                return Result.Succeeded;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
