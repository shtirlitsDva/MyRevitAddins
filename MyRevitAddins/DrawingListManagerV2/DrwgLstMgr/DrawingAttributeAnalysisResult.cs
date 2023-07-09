using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrawingAttributeAnalysisResult
    {
        private static readonly int numberOfEnumValues =
            Enum.GetValues(typeof(DrawingInfoTypeEnum)).Length;
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
        private static Bitmap bmp = new Bitmap(1, 1);
        private static Graphics g = Graphics.FromImage(bmp);
        private static Font font = SystemFonts.DefaultFont;
        private static string[] enumNames = Enum.GetNames(typeof(DrawingInfoTypeEnum));
        private static string _getKey(int i) => enumNames[i].Substring(0, 1) + "; ";
        private string _getToolTip()
        {
            List<string> toolTip = new List<string>();
            float maxWidth = 0;

            // Find the maximum string width
            for (int i = 1; i < numberOfEnumValues; i++)
            {
                if (Data[i].IsNotNoE())
                {
                    var key = _getKey(i);
                    var keySize = g.MeasureString(key + ": ", font);
                    maxWidth = Math.Max(maxWidth, keySize.Width);
                }
            }

            // Create the tooltips with padding for alignment
            for (int i = 1; i < numberOfEnumValues; i++)
            {
                if (Data[i].IsNotNoE())
                {
                    var key = _getKey(i);
                    var keySize = g.MeasureString(key + ": ", font);
                    int numSpaces = (int)((maxWidth - keySize.Width) / g.MeasureString(" ", font).Width);
                    var paddedKey = key + ": " + new string(' ', numSpaces);
                    toolTip.Add($"{paddedKey}{Data[i]}");
                }
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
            DataGridViewCellStyle? style = default;
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
            
            return style;
        }
        public DataGridViewCellStyle CellStyle { get => _getCellStyle(); }
    }
}
