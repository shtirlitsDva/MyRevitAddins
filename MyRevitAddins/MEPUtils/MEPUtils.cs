using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using MoreLinq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace MEPUtils
{
    public partial class MEPUtilsChooser : System.Windows.Forms.Form
    {
        Document Doc { get; }
        public int MethodToExecute { get; private set; }
        
        public MEPUtilsChooser(ExternalCommandData commandData)
        {
            InitializeComponent();
            Doc = commandData.Application.ActiveUIDocument.Document;

            //From here: http://stackoverflow.com/questions/34426888/dynamic-button-creation-placing-them-in-a-predefined-order-using-c-sharp
            int columnCount = 1;
            int rowCount = 2;

            this.tableLayoutPanel1.ColumnCount = columnCount;
            this.tableLayoutPanel1.RowCount = rowCount;

            this.tableLayoutPanel1.ColumnStyles.Clear();
            this.tableLayoutPanel1.RowStyles.Clear();

            for (int i = 0; i < columnCount; i++)
            {
                this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(System.Windows.Forms.SizeType.Percent, 100 / columnCount));
            }
            for (int i = 0; i < rowCount; i++)
            {
                this.tableLayoutPanel1.RowStyles.Add(new RowStyle(System.Windows.Forms.SizeType.Percent, 100 / rowCount));
            }

            var b1 = new Button()
            {
                Text = "Create all insulation",
                Name = "B1"
            };
            b1.Click += B1_Click;
            b1.Dock = DockStyle.Fill;
            b1.AutoSizeMode = 0;
            tableLayoutPanel1.Controls.Add(b1);

            var b2 = new Button()
            {
                Text = "Delete all insulation",
                Name = "B2"
            };
            b2.Click += B2_Click;
            b2.Dock = DockStyle.Fill;
            b2.AutoSizeMode = 0;
            tableLayoutPanel1.Controls.Add(b2);
        }

        private void B1_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            MethodToExecute = 1;
            Close();
        }

        private void B2_Click(object sender, EventArgs e)
        {
            var b = sender as Button;
            MethodToExecute = 2;
            Close();
        }

        //StringBuilder sb = new StringBuilder();
        //foreach (var f in query)
        //{
        //    sb.AppendLine(f.Name);
        //}
        //ut.InfoMsg(sb.ToString());
    }
}
