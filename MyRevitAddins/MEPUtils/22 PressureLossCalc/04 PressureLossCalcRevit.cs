using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using static Shared.Filter;

namespace MEPUtils.PressureLossCalc
{
    public static class RevitInteraction
    {
        public static void PrintSegmentInfo(ExternalCommandData cData)
        {
            UIApplication uiApp = cData.Application;
            Document doc = cData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            Guid parGuid = new Guid("6180df8e-5a26-41ce-94ca-4b1933f4a60e");
            SharedParameterElement spe = SharedParameterElement.Lookup(doc, parGuid);

            FilterRule fr = ParameterFilterRuleFactory.CreateHasValueParameterRule(spe.Id);
            ElementParameterFilter epf = new ElementParameterFilter(fr);

            FilteredElementCollector col = new FilteredElementCollector(doc);
            List<BuiltInCategory> cats = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_PipeAccessory,
                BuiltInCategory.OST_PipeCurves,
                BuiltInCategory.OST_PipeFitting
            };
            var emf = new ElementMulticategoryFilter(cats);

            col = col.WherePasses(emf).WherePasses(epf);

            StringBuilder sb = new StringBuilder();

            var groups = col.GroupBy(
                x => x.get_Parameter(
                    BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM)
                .AsString());

            foreach (var group in groups)
            {
                sb.AppendLine(group.Key);
                var strGroups = group.GroupBy(
                    x => x.get_Parameter(parGuid).AsString())
                    .OrderBy(x => x.Key);


            }

        }
    }
}
