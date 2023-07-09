using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrawingAttributeAnalysisResult
    {
        private DrawingAttributeAnalysisResult() { }
        public DrawingAttributeAnalysisResult(PropertiesEnum property,
            IGrouping<string, DrawingInfo> group)
        {
            //int count = 0;
            foreach (DrawingInfo info in group)
            {
                //count++;
                //if (count > 3)
                //    throw new Exception("There are more than three " +
                //        $"DrawingInfos for drawing number {group.Key}!");

                Data.SetData(info.GetPropertyValue(property), info.DrawingType);
            }
        }

        private PropertyDataService Data = new PropertyDataService();
        public string ToolTip { get => _getToolTip(); }
        private string _getToolTip()
        {
            List<string> toolTip = new List<string>();
            for (int i = 1; i < 4; i++)
            {
                if (Data[i].IsNotNoE())
                    toolTip.Add(
                        Enum.GetName(typeof(DrawingInfoTypeEnum), i) + ": "
                        + Data[i]);
            }

            return string.Join("\n", toolTip);
        }
        private string _displayValue { get => _getDisplayValue(); }
        private string _getDisplayValue()
        {
            if (Data.HasExcel) return Data.Excel;
            else if (Data.HasReleased) return Data.Released;
            else if (Data.HasStaging) return Data.Staging;
            else return "WRN0001:Empty property!";
        }
        public override string ToString() => _displayValue;
        public bool IsValid() => _displayValue.IsNotNoE();
        private DataGridViewCellStyle _getCellStyle()
        {
            DataGridViewCellStyle style = default;
            //Cases:
            if (Data.HasExcel && Data.HasReleased)
            {
                if (!Data.HasStaging)
                {
                    if (Data.Excel == Data.Released) style = DgvStyles.AllOkay;
                    else style = DgvStyles.Warning;
                }
                else style = DgvStyles.RevisionPending;
            }
            else if (Data.HasExcel && !Data.HasReleased)
            {
                if (!Data.HasStaging) style = DgvStyles.Error;
                else style = DgvStyles.NewDrawing;
            }
            else if (!Data.HasExcel && Data.HasReleased)
            {
                if (!Data.HasStaging) style = DgvStyles.OnlyExcelData;
                else style = DgvStyles.OnlyExcelData;
            }
            else if (!Data.HasExcel && !Data.HasReleased)
            {
                if (!Data.HasStaging) style = DgvStyles.Error;
                else style = DgvStyles.Error;
            }
            else throw new Exception("This should never happen!");
            
            return style ?? new DataGridViewCellStyle();
        }
        public DataGridViewCellStyle CellStyle { get => _getCellStyle(); }
    }
}
