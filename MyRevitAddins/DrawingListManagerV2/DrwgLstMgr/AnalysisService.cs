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
            {
                if (group.Count() > 3)
                    throw new Exception("There are more than three " +
                        $"DrawingInfos for drawing number {group.Key}!");

                var infosDict = group.ToDictionary(x => x.DrawingType, x => x);

                var analysisResult = new DrawingAnalysisResult();

                DrawingInfo? released = default;
                if (infosDict.ContainsKey(DrawingInfoTypeEnum.Released))
                    released = infosDict[DrawingInfoTypeEnum.Released];

                DrawingInfo? staging = default;
                if (infosDict.ContainsKey(DrawingInfoTypeEnum.Staging))
                    staging = infosDict[DrawingInfoTypeEnum.Staging];

                DrawingInfo? excel = default;
                if (infosDict.ContainsKey(DrawingInfoTypeEnum.DrawingList))
                    excel = infosDict[DrawingInfoTypeEnum.DrawingList];

                //Define cases
                //0. Case where all three are absent is not possible
                //1. Released and excel present, staging is absent
                //released and excel data must match else warning
                if (released != default && excel != default && staging == default)
                {

                }
                //2. Released present, excel and staging are absent
                //Error about missing excel, staging is not important
                else if (released != default && excel == default && staging == default)
                {

                }
                //3. Excel present, released and staging are absent
                //Error about missing released, staging is not important
                else if (released == default && excel != default && staging == default)
                {

                }
                //4. Released and staging present, excel is absent
                //somehow the released went missing from excel
                //while already a revision is issued
                //Error about missing excel
                else if (released != default && excel == default && staging != default)
                {

                }
                //5. Released and excel absent, staging is present
                //A new drawing is issued, but not yet in excel list
                //Issue a warning about missing excel
                else if (released == default && excel == default && staging != default)
                {

                }
                //6. Excel and staging present, released is absent
                //A drawing is issued -> mark it as new drawing

            }

            yield break;
        }
    }

    internal class StrategyChains
    {
        public Dictionary<PropertiesEnum, AnalysisStrategy> Dict { get; }

        public StrategyChains()
        {
            Dict = new Dictionary<PropertiesEnum, AnalysisStrategy>()
            {
                {PropertiesEnum.Number, new DrawingNumberAllSourcesOkay()
                .SetNext()}
            };
        }
    }
}
