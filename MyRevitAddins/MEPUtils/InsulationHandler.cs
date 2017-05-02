using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace MEPUtils
{
    public class InsulationHandler
    {
        public Result CreateInsulationForPipes(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;
            var allPipes = fi.GetElements<Pipe>(doc);

            var allInsulationTypes = fi.GetElements<PipeInsulationType>(doc);
            var insulationType = (from PipeInsulationType pit in allInsulationTypes
                                  where pit.)

            return Result.Succeeded;
        }
    }
}
