using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;
using Mathcad;
using Shared;

namespace GeneralStability
{
    public partial class GeneralStabilityForm : Form
    {
        public static ExternalCommandData _commandData;
        public static UIApplication _uiapp;
        public static UIDocument _uidoc;
        public static Document _doc;
        public string _message;

        public GeneralStabilityForm(ExternalCommandData cData, ref string message)
        {
            InitializeComponent();
            _commandData = cData;
            _uiapp = _commandData.Application;
            _uidoc = _uiapp.ActiveUIDocument;
            _doc = _uidoc.Document;
            _message = message;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filePath;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get file path
                filePath = openFileDialog1.FileName;
                Mathcad.Application mc = new Mathcad.Application();
                Mathcad.Worksheets wk;
                Mathcad.Worksheet ws;

                mc.Visible = true;

                wk = mc.Worksheets;
                ws = wk.Open(filePath);
                

                try
                {
                    ws.SetValue("ina", 1000);
                    ws.SetValue("inb", 500);
                    ws.Recalculate();
                    var result = ws.GetValue("ab") as Mathcad.INumericValue;
                    Util.InfoMsg(result.Real.ToString());

                    var result2 = ws.GetValue("test3");

                    var out1 = result2 as Mathcad.
                    Util.InfoMsg(out1.UnitsXML);

                }
                catch (Exception ex)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wk);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(mc);
                    Console.WriteLine(ex);
                    Util.InfoMsg(ex.Message);
                }

                ws.Close(Mathcad.MCSaveOption.mcSaveChanges);
                mc.Quit(MCSaveOption.mcSaveChanges);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wk);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(mc);
            }
        }
    }
}
