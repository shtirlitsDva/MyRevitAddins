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

namespace MEPUtils.SetTagsModeless
{
    public interface IAsyncCommand
    {
        void Execute(UIApplication uiApp);
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
    }
}
