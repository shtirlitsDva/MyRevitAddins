using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using mySettings = MEPUtils.Properties.Settings;

namespace MEPUtils.PressureLossCalc
{
    public partial class PressureLossCalcForm : Form
    {
        double TempSupply { get; set; }
        double TempReturn { get; set; }
        double Power { get; set; }
        private CultureInfo culture = CultureInfo.CreateSpecificCulture("da-DK");

        public PressureLossCalcForm()
        {
            InitializeComponent();

            textBox_InputPower.Text = mySettings.Default.InputPowerText;
            textBox_InputTempSupply.Text = mySettings.Default.InputTempSupplyText;
            textBox_InputTempReturn.Text = mySettings.Default.InputTempReturnText;
        }

        private void textBox_PowerInput_TextChanged(object sender, EventArgs e)
        {
            this.Power = parseNumber(textBox_InputPower.Text);

            textBox_DisplayPower.Text = this.Power.ToString();
        }

        private void textBox_InputTempSupply_TextChanged(object sender, EventArgs e)
        {
            this.Power = parseNumber(textBox_InputTempSupply.Text);

            textBox_DisplayTempSupply.Text = this.Power.ToString();
        }

        private void textBox_InputTempReturn_TextChanged(object sender, EventArgs e)
        {
            this.Power = parseNumber(textBox_InputTempReturn.Text);

            textBox_DisplayTempReturn.Text = this.Power.ToString();
        }

        private double parseNumber(string input)
        {
            double power = 0;

            if (double.TryParse(
                input,
                NumberStyles.Number,
                this.culture,
                out power))
            {
                return power;
            }
            else return 0;
        }
    }
}
