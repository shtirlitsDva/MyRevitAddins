using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;
using Mathcad = Ptc.MathcadPrime.Automation;
using Shared;
using mySettings = GeneralStability.Properties.Settings;
using op = Shared.Output;

namespace GeneralStability
{
    public partial class GeneralStabilityForm : Form
    {
        private static ExternalCommandData _commandData;
        private static UIApplication _uiapp;
        private static UIDocument _uidoc;
        private static Document doc;
        private string _message;
        private string _debugFilePath;

        //Declare the Mathcad Elements
        Mathcad.Application mc;
        Mathcad.Worksheets wk;
        Mathcad.Worksheet ws;

        public GeneralStabilityForm(ExternalCommandData cData, ref string message)
        {
            InitializeComponent();

            _commandData = cData;
            _uiapp = _commandData.Application;
            _uidoc = _uiapp.ActiveUIDocument;
            doc = _uidoc.Document;
            _message = message;

            //Init interface info boxes
            textBox1.Text = mySettings.Default._worksheetPath;
            textBox2.Text = mySettings.Default.debugFilePath;

            //Init variables
            _debugFilePath = mySettings.Default.debugFilePath;

            //Clear the debug file
            System.IO.File.WriteAllBytes(_debugFilePath, new byte[0]);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Get file path
                string filePath = openFileDialog1.FileName;
                textBox1.Text = filePath;
                mySettings.Default._worksheetPath = filePath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mc = new Mathcad.Application {Visible = true};
            wk = mc.Worksheets;
            ws = wk.Open(mySettings.Default._worksheetPath);

            int rows, cols;

            try
            {
                InteractionRevit ir = new InteractionRevit(doc);

                double[] matrix = new double[ir.WallsAlong.Count];

                for (int i = 0; i < ir.WallsAlong.Count; i++)
                {
                    FamilyInstance fi = ir.WallsAlong[i];
                    LocationCurve loc = fi.Location as LocationCurve;
                    double length = Util.FootToMeter(loc.Curve.Length);
                    matrix[i] = length;
                }

                //rows = ir.WallsAlong.Count;
                //cols = 1;

                //NumericValue member = matrix.GetElement(0, 0) as NumericValue;

                //for (int i = 0; i < rows; i++)
                //{
                //    for (int j = 0; j < cols; j++)
                //    {
                //        LocationCurve loc = ir.WallsAlong[i].Location as LocationCurve;
                //        double length = Util.FootToMeter(loc.Curve.Length);

                //        member.Real = length;

                //        matrix.SetElement(i, j, member);
                //    }
                //}

                ws.SetValue("i.x", matrix);

                ws.Recalculate();
                ws.Save();

                //Regions regions = ws.Regions;

                var value = ws.GetValue("i.x") as MatrixValue;

                StringBuilder sb2 = new StringBuilder();

                sb2.Append(value.AsString);
                sb2.AppendLine();

                rows = value.Rows;
                cols = value.Cols;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        INumericValue numval = value.GetElement(i, j);
                        sb2.Append(numval.Real.ToString());
                        sb2.AppendLine();
                    }
                }
                op.WriteDebugFile(_debugFilePath, sb2);
            }
            catch (Exception ex)
            {
                //Cleanup();
                Console.WriteLine(ex);
                StringBuilder exSb = new StringBuilder();
                exSb.Append(ex.Message);
                exSb.AppendLine();
                op.WriteDebugFile(_debugFilePath,exSb);
                //Util.InfoMsg(ex.Message);
            }
            Cleanup();
        }

        private void Cleanup()
        {
            //ws.Close(Mathcad.MCSaveOption.mcSaveChanges);
            //mc.Quit(MCSaveOption.mcSaveChanges);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(wk);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(mc);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                //Get file path
                string filePath = openFileDialog2.FileName;
                textBox2.Text = filePath;
                _debugFilePath = filePath;
                mySettings.Default.debugFilePath = filePath;
            }
        }
    }
}
