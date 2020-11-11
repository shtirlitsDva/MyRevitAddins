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

        //data stuff
        SelectionInformationContainer Payload;

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
            checkedListBox2.Items.Clear();
            checkedListBox2.Items.AddRange(cats);
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    checkedListBox2.SetItemChecked(i, true);
                }
            }
            else
            {
                for (int i = 0; i < checkedListBox2.Items.Count; i++)
                {
                    checkedListBox2.SetItemChecked(i, false);
                }
            }
        }
        private bool subscribedToSnSOperationComplete = false;
        /// <summary>
        /// Select button pushed.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            Payload = new SelectionInformationContainer();
            Payload.CategoriesToSearch = checkedListBox2.CheckedItems.OfType<string>().ToList();
            if (!subscribedToSnSOperationComplete)
            {
                Payload.SnSOperationComplete += UpdateTreeView;
                subscribedToSnSOperationComplete = true;
            }

            AsyncSelectByFilters asSBF = new AsyncSelectByFilters(Payload);
            ThisApp.asyncCommand = asSBF;
            m_ExEvent.Raise();
        }

        private void UpdateTreeView(object sender, MyEventArgs e)
        {
            int nrOfLevels = 3;
            string[] levelNames = new string[] { "System Abbreviation", "Category Name", "Family and Type Name" };
            //Level 1: System Abbreviation
            //Level 2: Category Name
            //Level 3: Family and Type Name

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();


            
            treeView1.EndUpdate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Request parameter data from Revit
        }
    }
}
