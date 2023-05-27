using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrwgLstMgr
    {
        private HashSet<DrawingInfo> drwgs;

        internal void Reset()
        {
            drwgs.Clear();
        }
    }
}
