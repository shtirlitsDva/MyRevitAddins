using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrawingAttributeAnalysisResult
    {
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

                SetData(info.GetPropertyValue(property), info.DrawingType);
            }
        }

        private string[] data = new string[4];
        public string ToolTip { get => GetToolTip(); }
        private void SetData(string value, DrawingInfoTypeEnum drawingType)
        {
            data[(int)drawingType] = value;
        }
        public string GetData(DrawingInfoTypeEnum drawingType)
        {
            return data[(int)drawingType];
        }
        private string GetToolTip()
        {
            List<string> toolTip = new List<string>();
            for (int i = 1; i < 4; i++)
            {
                if (data[i].IsNotNoE())
                    toolTip.Add(
                        Enum.GetName(typeof(DrawingInfoTypeEnum), i) + ": "
                        + data[i]);
            }

            return string.Join("\n", toolTip);
        }
        private string _displayValue { get => _getDisplayValue(); }
        private string _getDisplayValue()
        {
            if (data[1].IsNotNoE()) return data[1];
            else if (data[2].IsNotNoE()) return data[2];
            else if (data[3].IsNotNoE()) return data[3];
            else return "";
        }
        public override string ToString() => _displayValue;
        public bool IsValid() => _displayValue.IsNotNoE();
        public DataGridViewCellStyle? CellStyle { get; set; }
    }
}
