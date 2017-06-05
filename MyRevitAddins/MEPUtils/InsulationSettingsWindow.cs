using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MoreLinq;
using System.IO;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using Color = System.Drawing.Color;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace MEPUtils
{
    public partial class InsulationSettingsWindow : System.Windows.Forms.Form
    {
        public InsulationSettingsWindow(ExternalCommandData cData)
        {
            InitializeComponent();

            this.Height = 800;

            Document doc = cData.Application.ActiveUIDocument.Document;

            //Collect all pipe accessories names and types to determine number of rows
            var elements = fi.GetElements(doc, BuiltInCategory.OST_PipeAccessory)
                .OrderBy(o => o.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString())
                .ToList();

            tableLayoutPanel1.ColumnCount = 2; //First column name of inst, Second setting to add insulation
            tableLayoutPanel1.RowCount = elements.Count;

            //TODO: Fix column width!!
            //TODO: Fix text box width!!

            for (int i = 0; i < elements.Count; i++)
            {
                var tb = new System.Windows.Forms.TextBox()
                {
                    Text = elements[i].get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString(),
                    Name = $"TextBox_{i}",
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    TextAlign = HorizontalAlignment.Right,
                    Width = TextRenderer.MeasureText(Text, Font).Width,
                    Height = TextRenderer.MeasureText(Text, Font).Height,
                    TabStop = false,
            };
                
                tableLayoutPanel1.Controls.Add(tb, 0, i);

                var cb = new CheckBox()
                {
                    Name = $"CheckBox_{i}",
                    Anchor = AnchorStyles.None,
                };

                cb.Enter += Cb_Enter;
                cb.Leave += Cb_Leave;
                
                tableLayoutPanel1.Controls.Add(cb, 1, i);
            }

        }

        private void Cb_Leave(object sender, EventArgs e)
        {
            var c = sender as CheckBox;
            c.BackColor = DefaultBackColor;
        }

        private void Cb_Enter(object sender, EventArgs e)
        {
            var c = sender as CheckBox;
            c.BackColor = Color.DarkGray;
        }
    }
}
