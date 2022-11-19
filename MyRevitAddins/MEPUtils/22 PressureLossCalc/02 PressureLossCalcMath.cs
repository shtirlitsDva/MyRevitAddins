using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI.Selection;

namespace MEPUtils.PressureLossCalc
{
    public static class CalcPressureLoss
    {
        public static double currentFlow;
    }
    public static class CalcFlow
    {
        static double waterSpecificThermalCapacity = 4545.0; //J/(kg*K)
        static double waterDensity = 958.05; //kg/m³
        public static double calculateFlow(double power, double tF, double tR)
        {
            return power / (
                waterSpecificThermalCapacity * waterDensity * (
                    tF - tR)) * (1000 * 3600); //m³/h
        }
    }
}
