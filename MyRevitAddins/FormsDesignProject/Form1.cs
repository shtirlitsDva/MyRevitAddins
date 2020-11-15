using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsDesignProject
{
    public partial class Form1 : Form
    {
        List<string> parTypeList;
        List<string> parList;

        public Form1()
        {
            InitializeComponent();

            parTypeList = new List<string>() { "Built-in Parameter", "Shared Parameter" };
            parList = new List<string>() { "one", "two", "three", "four", "five" };

            comboBox1.DataSource = new BindingSource { DataSource = parTypeList };
            comboBox2.DataSource = new BindingSource { DataSource = parList };


        }

        private void addRowMethod(object sender, EventArgs e)
        {
            int rowIndex = tableLayoutPanel1.GetRow((Button)sender);

            //Take the last row and create same one at end of collection
            RowStyle temp = tableLayoutPanel1.RowStyles[tableLayoutPanel1.RowCount - 1];
            tableLayoutPanel1.RowCount++;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(temp.SizeType, temp.Height));

            //Take the previous row and change style
            temp = tableLayoutPanel1.RowStyles[0];
            tableLayoutPanel1.RowStyles[tableLayoutPanel1.RowCount - 2] = new RowStyle(temp.SizeType, temp.Height);

            if (tableLayoutPanel1.RowCount - rowIndex != 3) //= 3 means the control is the last and need not moved
            {
                //Move controls down
                for (int i = tableLayoutPanel1.RowCount - 3; i > rowIndex; i--)
                {
                    for (int j = 0; j < tableLayoutPanel1.ColumnCount; j++)
                    {
                        var control = tableLayoutPanel1.GetControlFromPosition(j, i);
                        if (control != null)
                        {
                            tableLayoutPanel1.SetRow(control, i + 1);
                        }
                    }
                }
            }

            #region Add controls
            //Add controls
            tableLayoutPanel1.Controls.Add(new ComboBox()
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = new BindingSource
                {
                    DataSource = parTypeList
                }
            }, 0, rowIndex + 1); ;

            tableLayoutPanel1.Controls.Add(new ComboBox()
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = new BindingSource
                {
                    DataSource = parList
                }
            }, 1, rowIndex + 1);

            Button button = new Button() { Dock = DockStyle.Fill, Text = "+" };
            button.Click += addRowMethod;

            tableLayoutPanel1.Controls.Add(button, 2, rowIndex + 1);

            button = new Button() { Dock = DockStyle.Fill, Text = "-" };
            button.Click += removeRowMethod;

            tableLayoutPanel1.Controls.Add(button, 3, rowIndex + 1);
            #endregion
        }

        private void removeRowMethod(object sender, EventArgs e)
        {
            int rowIndex = tableLayoutPanel1.GetRow((Button)sender);

            if (rowIndex >= tableLayoutPanel1.RowCount)
            {
                return;
            }

            // delete all controls of row that we want to delete
            for (int i = 0; i < tableLayoutPanel1.ColumnCount; i++)
            {
                var control = tableLayoutPanel1.GetControlFromPosition(i, rowIndex);
                tableLayoutPanel1.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
            for (int i = rowIndex + 1; i < tableLayoutPanel1.RowCount; i++)
            {
                for (int j = 0; j < tableLayoutPanel1.ColumnCount; j++)
                {
                    var control = tableLayoutPanel1.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        tableLayoutPanel1.SetRow(control, i - 1);
                    }
                }
            }

            var removeStyle = tableLayoutPanel1.RowCount - 1;

            if (tableLayoutPanel1.RowStyles.Count > removeStyle)
                tableLayoutPanel1.RowStyles.RemoveAt(removeStyle);

            tableLayoutPanel1.RowCount--;
        }
    }
}
