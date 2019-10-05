using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

using static Shared.Filter;

namespace MEPUtils.SetTagsModeless
{
    public interface IAsyncCommand
    {
        void Execute(UIApplication uiApp);
    }

    class AsyncFindSelectElement : IAsyncCommand
    {
        private DataGridView Dgw { get; set; }
        private AsyncFindSelectElement() { }
        public AsyncFindSelectElement(DataGridView dgw)
        {
            Dgw = dgw;
        }
        public void Execute(UIApplication uiApp)
        {
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            Selection selection = uidoc.Selection;

            //I cannot find a way to get to a shared parameter element
            //whithout GUID so I must assume I am working with
            //Pipe Accessories and I use a first element as donor
            FilteredElementCollector donorFec = new FilteredElementCollector(doc);
            Element paDonor = donorFec.OfCategory(BuiltInCategory.OST_PipeAccessory).FirstElement();
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfCategory(BuiltInCategory.OST_PipeAccessory);

            //Iterate over each column of dgw, only acting on filled out cells for parameter names
            //then filter collector by a elementparameterfilter
            int i = 0;
            foreach (DataGridViewColumn column in Dgw.Columns)
            {
                //Test to see if there's a name of parameter specified
                var parNameValue = Dgw.Rows[1].Cells[i].Value;
                if (parNameValue == null) { i++; continue; }
                string parName = parNameValue.ToString();
                if (string.IsNullOrEmpty(parName)) { i++; continue; }
                Parameter parToTest = paDonor.LookupParameter(parName);
                if (parToTest == null) continue;

                //Retrieve value to filter against
                var parValue = Dgw.Rows[0].Cells[i].Value;
                if (parValue == null) { i++; continue; }
                string parValueString = parValue.ToString();
                if (string.IsNullOrEmpty(parValueString)) { i++; continue; }

                ElementParameterFilter epf = ParameterValueGenericFilter(doc, parValueString, parToTest.GUID);
                col.WherePasses(epf);
            }

            uidoc.Selection.SetElementIds(col.ToElementIds());
        }
    }

    class AsyncUpdateParameterValues : IAsyncCommand
    {
        private DataGridView Dgw { get; set; }

        private AsyncUpdateParameterValues() { }

        public AsyncUpdateParameterValues(DataGridView dgw)
        {
            Dgw = dgw;
        }

        public void Execute(UIApplication uiApp)
        {
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            Selection selection = uidoc.Selection;
            var selIds = selection.GetElementIds();

            if (selIds.Count > 1)
            {
                ErrorMsg("More than one element selected! Please select only one element.");
                return;
            }
            if (selIds.Count < 1)
            {
                ErrorMsg("No element selected! Please select only one element.");
                return;
            }

            ElementId elId = selIds.FirstOrDefault();

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Update parameter values");

                int i = 0;
                foreach (DataGridViewColumn column in Dgw.Columns)
                {
                    //Test to see if there's a name of parameter specified
                    var parNameValue = Dgw.Rows[1].Cells[i].Value;

                    if (parNameValue == null) { i++; continue; }

                    string parName = parNameValue.ToString();

                    if (string.IsNullOrEmpty(parName)) { i++; continue; }

                    Element el = doc.GetElement(elId);

                    Parameter parToSet = el.LookupParameter(parName);
                    if (parToSet == null) throw new Exception($"Parameter name {parName} does not exist for element {el.Id.ToString()}!");

                    var parValue = Dgw.Rows[0].Cells[i].Value;

                    if (parValue == null) { i++; continue; }

                    parToSet.Set(parValue.ToString());

                    i++;
                }
                tx.Commit();
            }
        }

        public static void ErrorMsg(string msg)
        {
            Debug.WriteLine(msg);
            WinForms.MessageBox.Show(msg,
              "Error",
              WinForms.MessageBoxButtons.OK,
              WinForms.MessageBoxIcon.Error);
        }
    }
}
