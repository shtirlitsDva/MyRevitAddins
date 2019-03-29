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

namespace MEPUtils.SetTags
{
    public partial class SetTagsInterface : Form
    {
        public string ValueToSet;

        public SetTagsInterface()
        {
            InitializeComponent();
        }

        ////private void textBox1_TextChanged(object sender, EventArgs e) => DistanceToKeep = textBox1.Text;

        //private void InputBoxBasic_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    ValueToSet = textBox1.Text;
        //}

        //private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Enter) this.Close();
        //}

        //private void InputBoxBasic_Shown(object sender, EventArgs e)
        //{
        //    textBox1.Text = ValueToSet;
        //    textBox1.SelectionStart = 0;
        //    textBox1.SelectionLength = textBox1.Text.Length;
        //}
    }
}
