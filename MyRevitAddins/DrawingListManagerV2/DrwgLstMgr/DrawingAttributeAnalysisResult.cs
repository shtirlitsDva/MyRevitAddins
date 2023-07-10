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
            this._property = property;
            foreach (DrawingInfo info in group)
            {
                Data.SetData(info.GetPropertyValue(property), info.DrawingType);
                if (property == PropertiesEnum.Number) fileNames.Add(info.FileNameWithPath);
            }
                
        }
        private List<string> fileNames = new List<string>();
        private PropertyDataService Data = new PropertyDataService();
        public string ToolTip { get => _getToolTip(); }
        private static string[] enumNames = Enum.GetNames(typeof(DrawingInfoTypeEnum));
        private static string _getKey(int i) => enumNames[i].Substring(0, 1) + ": ";
        private string _getToolTip()
        {
            List<string> toolTip = new List<string>();

            // Create the tooltips with padding for alignment
            for (int i = 1; i < numberOfEnumValues; i++)
                if (Data[i].IsNotNoE())
                    toolTip.Add($"{_getKey(i)}{Data[i]}");

            return string.Join("\n", toolTip);
        }
        private string _displayValue { get => _getDisplayValue(); }
        private string _getDisplayValue()
        {
            if (Data.HasExcel) return Data.Excel;
            else if (Data.HasReleased) return Data.Released;
            else if (Data.HasStaging) return Data.Staging;
            else return string.Join("\n", fileNames);
            //else return "¯\\_(ツ)_/¯";
        }
        public override string ToString() => _displayValue;
        public bool IsValid() => _displayValue.IsNotNoE();
        private PropertiesEnum _property;
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
            
            if (_property == PropertiesEnum.Number)
                style.Alignment = DataGridViewContentAlignment.MiddleRight;
            else if (_property == PropertiesEnum.Title)
                style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            else style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            return style;
        }
        public DataGridViewCellStyle CellStyle { get => _getCellStyle(); }
    }
}
