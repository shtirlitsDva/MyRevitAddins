using System;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;
using stgs = MEPUtils.Properties.Settings;

namespace MEPUtils.SetMark
{
    public partial class InputBoxBasic : Form
    {
        public string ValueToSet;

        public InputBoxBasic()
        {
            InitializeComponent();
            if (!stgs.Default.SetMark_ValueToSet.IsNullOrEmpty()) textBox1.Text = stgs.Default.SetMark_ValueToSet;
            else textBox1.Text = "0";
            textBox1.SelectionStart = 0;
            textBox1.SelectionLength = textBox1.Text.Length;
        }

        //private void textBox1_TextChanged(object sender, EventArgs e) => DistanceToKeep = textBox1.Text;

        private void InputBoxBasic_FormClosing(object sender, FormClosingEventArgs e)
        {
            ValueToSet = textBox1.Text;
            stgs.Default.Save();
        }

        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) this.Close();
        }
    }
}
