﻿using System;
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

                DrawingInfo released = default;
                if (infosDict.ContainsKey(DrawingInfoTypeEnum.Released))
                    released = infosDict[DrawingInfoTypeEnum.Released];

                DrawingInfo staging = default;
                if (infosDict.ContainsKey(DrawingInfoTypeEnum.Staging))
                    staging = infosDict[DrawingInfoTypeEnum.Staging];

                DrawingInfo excel = default;
                if (infosDict.ContainsKey(DrawingInfoTypeEnum.DrawingList))
                    excel = infosDict[DrawingInfoTypeEnum.DrawingList];

                if (released != default)
                {//Case drawing is in released
                    if (excel != default)
                    {//Case drawing is in drawing list
                        //Compare the data from file and from excel
                        DrawingAttributeAnalysisResult daar = new DrawingAttributeAnalysisResult();
                        daar.DisplayValue = group.Key;

                    }
                }
                else
                {//Case drawing has not been placed in released

                }
            }

            yield break;
        }
    }
}
