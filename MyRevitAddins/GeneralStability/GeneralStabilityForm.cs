using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace GeneralStability
{
    public partial class GeneralStabilityForm : Form
    {
        public GeneralStabilityForm(ExternalCommandData cData, ref string message)
        {
            InitializeComponent();
        }
    }
}
