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
using Form = System.Windows.Forms.Form;
using TextBox = System.Windows.Forms.TextBox;

namespace MEPUtils
{
    public partial class NumberStuffForm : Form
    {
        public TableLayoutPanel mainPanel { get; private set; }
        public DataTable Settings;
        public Result Result = Result.Cancelled;

        public NumberStuffForm(DataTable settings)
        {
            InitializeComponent();
            Settings = settings;
            this.mainPanel = new TableLayoutPanel(); //{ Dock = DockStyle.Fill };
            mainPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            mainPanel.ColumnCount = 4;
            mainPanel.RowCount = 2;

            mainPanel.Controls.Add(new Button { Text = "Cancel" }, 2, 0);
            mainPanel.Controls.Add(new Button { Text = "(Re-)Number" }, 3, 0);

            mainPanel.Controls.Add(new Label() { Text = "Family" }, 0, 1);
            mainPanel.Controls.Add(new Label() { Text = "TAG 1\n(Prefix)" }, 1, 1);
            mainPanel.Controls.Add(new Label() { Text = "TAG 2\n(Start nr.)" }, 2, 1);
            mainPanel.Controls.Add(new Label() { Text = "Nr. of digits" }, 3, 1);

            //See if element is not allowed to be insulated
            var query = Settings.AsEnumerable()
                .Where(row => row.Field<string>("Number") == "True");

            foreach (var row in query)
            {
                AddRow(row.Field<string>("Family"));
            }
        }

        public void AddRow(string familyName)
        {
            string prefix = Settings.AsEnumerable()
                .Where(row => row.Field<string>("Family") == familyName)
                .Select(row => row.Field<string>("Prefix")).FirstOrDefault()
                ?? "NA";
            int? startNr = Settings.AsEnumerable()
                .Where(row => row.Field<string>("Family") == familyName)
                .Select(row => row.Field<int>("StartNumber")).FirstOrDefault();
            if (startNr == null) startNr = -1;

            int? digits = Settings.AsEnumerable()
                .Where(row => row.Field<string>("Family") == familyName)
                .Select(row => row.Field<int>("Digits")).FirstOrDefault();
            if (digits == null) digits = 2;

            mainPanel.RowCount++;
            mainPanel.Controls.Add(new Label() { Text =  familyName}, 0, mainPanel.RowCount - 1);
            mainPanel.Controls.Add(new TextBox() { Text = prefix}, 1, mainPanel.RowCount - 1);
            mainPanel.Controls.Add(new TextBox() { Text = startNr.ToString() }, 2, mainPanel.RowCount - 1);
            mainPanel.Controls.Add(new TextBox() { Text = digits.ToString() }, 3, mainPanel.RowCount - 1);
        }

        private void NumberStuffForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
