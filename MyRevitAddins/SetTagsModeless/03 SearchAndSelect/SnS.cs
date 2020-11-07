using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEPUtils.ModelessForms.SearchAndSelect
{
    public partial class SnS : Form
    {
        //Modeless stuff
        private Autodesk.Revit.UI.ExternalEvent m_ExEvent;
        private ExternalEventHandler m_Handler;
        Application ThisApp;

        public SnS(Autodesk.Revit.UI.ExternalEvent exEvent,
                   ExternalEventHandler handler,
                   MEPUtils.ModelessForms.Application thisApp)
        {
            InitializeComponent();

            m_ExEvent = exEvent;
            m_Handler = handler;
            ThisApp = thisApp;

            //Setup a rudimentary list with categories
            string[] cats = { "Pipes", "Pipe Fittings", "Pipe Accessories" };
            checkedListBox1.Items.Clear();
            checkedListBox1.Items.AddRange(cats);
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, true);
                }
            }
            else
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, false);
                }
            }
        }
        /// <summary>
        /// Select button pushed.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            SelectionPredicateContainer payload = new SelectionPredicateContainer();
            payload.CategoriesToSearch = checkedListBox1.CheckedItems.OfType<string>().ToList();

            AsyncSelectByFilters asSBF = new AsyncSelectByFilters(payload);
            ThisApp.asyncCommand = asSBF;
            m_ExEvent.Raise();
        }
    }
}
