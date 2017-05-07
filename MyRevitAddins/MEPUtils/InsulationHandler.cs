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
    public static class InsulationHandler
    {
        public static Result CreateInsulationForPipes(Document doc)
        {
            string pipeInsulationName = "Rørisolering";

            var allInsulationTypes = fi.GetElements<PipeInsulationType>(doc);
            var insulationType = (from PipeInsulationType pit in allInsulationTypes
                                  where pit.Name == pipeInsulationName
                                  select pit).FirstOrDefault();

            if (insulationType == null)
            {
                ut.ErrorMsg("Create Pipe Insulation Type with name " + pipeInsulationName);
                return Result.Failed;
            }

            var allPipes = fi.GetElements<Pipe>(doc);

            return Result.Succeeded;
        }

        public static Result DeleteAllPipeInsulation(Document doc)
        {
            var allInsulation = fi.GetElements<PipeInsulation>(doc);
            if (allInsulation == null) return Result.Failed;
            else if (allInsulation.Count == 0) return Result.Failed;

            Transaction tx = new Transaction(doc);
            tx.Start("Delete all insulation!");
            foreach (Element el in allInsulation) doc.Delete(el.Id);
            tx.Commit();

            return Result.Succeeded;
        }
    }
}
