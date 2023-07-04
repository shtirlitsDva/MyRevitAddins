using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal static class AnalysisService
    {
        public static IEnumerable<DrawingAnalysisResult> AnalyseDrawings(
            IEnumerable<DrawingInfo> releasedDrawings,
            IEnumerable<DrawingInfo> stagingDrawings,
            IEnumerable<DrawingInfo> excelDrawings)
        {
            var allDrawings = Utils.Concat(releasedDrawings, stagingDrawings, excelDrawings);

            var groups = allDrawings.GroupBy(x => x.GetPropertyValue(PropertiesEnum.Number));

            foreach (IGrouping<string, DrawingInfo> group in groups)
                yield return new DrawingAnalysisResult(group);

            yield break;
        }
    }
}
