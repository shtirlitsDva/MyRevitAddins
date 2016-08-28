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
        Mathcad.ApplicationCreator app;

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
            try
            {
                var appCreate = new Mathcad.ApplicationCreatorClass();
                app = (Mathcad.ApplicationCreator) appCreate;
                app.Visible = true;
                var ws = app.Open(mySettings.Default._worksheetPath);
                
                InteractionMathcad interactionMathcad = new InteractionMathcad(doc, ws);
            }
            catch (Exception ex)
            {
                //Cleanup();
                Console.WriteLine(ex);
                StringBuilder exSb = new StringBuilder();
                exSb.Append(ex.Message);
                exSb.AppendLine();
                op.WriteDebugFile(_debugFilePath,exSb);
                app.CloseAll(Mathcad.SaveOption.spDiscardChanges);
                Cleanup();
                //Util.InfoMsg(ex.Message);
            }
            //app.CloseAll(Mathcad.SaveOption.spSaveChanges);
            Cleanup();
        }

        private void Cleanup()
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
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
