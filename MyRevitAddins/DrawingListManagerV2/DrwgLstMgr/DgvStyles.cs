using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal static class DgvStyles
    {
        internal static DataGridViewCellStyle AllOkay { get; } = new DataGridViewCellStyle()
        { ForeColor = Color.Green, BackColor = Color.GreenYellow };
        internal static DataGridViewCellStyle Warning { get; } = new DataGridViewCellStyle()
        { ForeColor = Color.Yellow, BackColor = Color.DeepSkyBlue };
        internal static DataGridViewCellStyle RevisionPending { get; } = new DataGridViewCellStyle()
        { ForeColor = Color.Green, BackColor = Color.LemonChiffon };
        internal static DataGridViewCellStyle Error { get; } = new DataGridViewCellStyle()
        { ForeColor = Color.DarkRed, BackColor = Color.Thistle };
        internal static DataGridViewCellStyle OnlyExcelData { get; } = new DataGridViewCellStyle()
        { ForeColor = Color.Green, BackColor = Color.SpringGreen };
    }
}
