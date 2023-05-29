using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal abstract class AnalysisStrategy
    {
        protected AnalysisStrategy _nextStrategy;

        public AnalysisStrategy SetNext(AnalysisStrategy next)
        {
            _nextStrategy = next;
            return _nextStrategy;
        }

        public virtual DrawingAttributeAnalysisResult Analyze(
            DrawingInfo released, DrawingInfo staging, DrawingInfo excel, PropertiesEnum property)
        {
            var result = new DrawingAttributeAnalysisResult()
            {
                DisplayValue = "Default analysis result",
                CellStyle = DgvStyles.Error  // default cell style
            };

            // Call the specific analysis of the derived strategy
            bool handled = SpecificAnalysis(released, staging, excel, result, property);

            // If the derived strategy did not handle the situation, pass to the next strategy
            if (!handled && _nextStrategy != null)
            {
                return _nextStrategy.Analyze(released, staging, excel, property);
            }

            return result;
        }

        protected abstract bool SpecificAnalysis(
            DrawingInfo released, DrawingInfo staging, DrawingInfo excel, 
            DrawingAttributeAnalysisResult result, PropertiesEnum property);

        internal bool AllEqual(params string[] values) => values.Distinct().Count() == 1;
    }
    /// <summary>
    /// The property from all drawing sources is the same.
    /// </summary>
    internal class DrawingNumberAllSourcesOkay : AnalysisStrategy
    {
        protected override bool SpecificAnalysis(
            DrawingInfo released, DrawingInfo staging, DrawingInfo excel, 
            DrawingAttributeAnalysisResult result, PropertiesEnum property)
        {
            //Pass null-checks
            if (released == null || staging == null || excel == null) return false;

            //Pass all equality (not really neccessary in the case of Number)
            string rlsValue = released.GetPropertyValue(property);
            string stgValue = staging.GetPropertyValue(property);
            string xlsValue = excel.GetPropertyValue(property);

            if (!AllEqual(rlsValue, stgValue, xlsValue)) return false;

            //The criteria cleared, build tool tip
            var tooltips = new List<string>
            {
                "RLS " + rlsValue,
                "STG " + stgValue,
                "XLS " + xlsValue,
            };

            if (released.HasMetaData) tooltips.Add(released.GetMetadataValue(property));

            result.ToolTip = string.Join(Environment.NewLine, tooltips);
            result.CellStyle = DgvStyles.AllOkay;
            return true;
        }
    }
    /// <summary>
    /// Released and Excel okay, no staging.
    /// Standard situation, drawing list is synchronised with released.
    /// </summary>
    internal class DrawingNumberReleasedExcelOk : AnalysisStrategy
    {
        protected override bool SpecificAnalysis(
            DrawingInfo released, DrawingInfo staging, DrawingInfo excel,
            DrawingAttributeAnalysisResult result, PropertiesEnum property)
        {
            //Pass null-checks
            if (released == null || excel == null || staging != null) return false;

            //Pass all equality (not really neccessary in the case of Number)
            string rlsValue = released.GetPropertyValue(property);
            string xlsValue = excel.GetPropertyValue(property);
            string? metaValue = default;
            if (released.HasMetaData) metaValue = released.GetMetadataValue(property);
            var vals = new List<string>() { rlsValue, xlsValue };
            if (metaValue.IsNotNoE()) vals.Add(metaValue);

            if (!AllEqual(vals.ToArray())) return false;

            //The criteria cleared, build tool tip
            var tooltips = new List<string>
            {
                "RLS " + rlsValue,
                "XLS " + xlsValue,
            };

            if (released.HasMetaData) tooltips.Add(released.GetMetadataValue(property));

            result.ToolTip = string.Join(Environment.NewLine, tooltips);
            result.CellStyle = DgvStyles.AllOkay;
            return true;
        }
    }



}
