using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using mySettings = HentRestData.Properties.Settings;

namespace HentRestData
{
    public partial class Form1 : Form
    {
        public string pathToSave;
        public string fullPathAndName;
        public Form1()
        {
            InitializeComponent();
            pathToSave = mySettings.Default.pathToSave;
            fullPathAndName = mySettings.Default.fullPathAndName;
            textBox1.Text = fullPathAndName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog(); //https://stackoverflow.com/a/41511598/6073998
            if (string.IsNullOrEmpty(pathToSave)) dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%userprofile%");
            else dialog.InitialDirectory = pathToSave;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                fullPathAndName = dialog.FileName + "\\bbrRest.json";
                mySettings.Default.pathToSave = dialog.FileName;
                textBox1.Text = fullPathAndName;
                mySettings.Default.fullPathAndName = fullPathAndName;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mySettings.Default.Save();
        }
    }
}
