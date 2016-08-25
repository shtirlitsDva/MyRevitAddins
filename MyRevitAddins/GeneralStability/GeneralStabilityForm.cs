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
using mySettings = GeneralStability.Properties.Settings;
using op = Shared.Output;
using Region = Mathcad.Region;

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

            try
            {
                RevitInteraction.InteractWithRevit(doc);

                Regions regions = ws.Regions;
                foreach (Region reg in regions)
                {
                    var type = reg.GetType();
                    
                    StringBuilder sb = new StringBuilder();

                    sb.Append(type.FullName);
                    sb.AppendLine();

                    op.WriteDebugFile(_debugFilePath, sb);
                }

                var value = ws.GetValue("i.x") as IMatrixValue;

                StringBuilder sb2 = new StringBuilder();

                sb2.Append(value.AsString);
                sb2.AppendLine();

                int rows = value.Rows;
                int cols = value.Cols;

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
            ws.Close(Mathcad.MCSaveOption.mcSaveChanges);
            mc.Quit(MCSaveOption.mcSaveChanges);
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
