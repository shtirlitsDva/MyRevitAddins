using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;

namespace MEPUtils.CreateInstrumentation
{
    public partial class _02_DirectionSelector : BaseFormTableLayoutPanel_BasicList
    {
        public _02_DirectionSelector(List<string> stringList) : base(stringList)
        {
            InitializeComponent();
        }
    }
}
