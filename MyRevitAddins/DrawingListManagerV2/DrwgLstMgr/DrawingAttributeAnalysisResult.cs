using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrawingAttributeAnalysisResult
    {
        public string DisplayValue { get; set; }
        public string ToolTip { get; set; }
        public DataGridViewCellStyle CellStyle { get; set; }
        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
