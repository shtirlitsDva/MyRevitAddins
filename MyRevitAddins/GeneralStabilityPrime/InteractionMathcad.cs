using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Mathcad;
using MathcadAutomationPrivate;
using Shared;
using mySettings = GeneralStability.Properties.Settings;
using con = Shared.Conversion;
using ut = Shared.Util;
using op = Shared.Output;

namespace GeneralStability
{
    public class InteractionMathcad
    {
        private readonly string _debugFilePath = mySettings.Default.debugFilePath;

        public InteractionMathcad(Document doc, Worksheet ws)
        {
            int rows, cols;

            InteractionRevit ir = new InteractionRevit(doc);

            try
            {
                System.Type objType = System.Type.GetTypeFromProgID("Mathcad.MatrixValue");

                dynamic comObject = System.Activator.CreateInstance(objType);

                rows = ir.WallsAlong.Count;
                cols = 1;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        LocationCurve loc = ir.WallsAlong[i].Location as LocationCurve;
                        double length = ut.FootToMeter(loc.Curve.Length);

                        NumericValue member = new NumericValue();
                        member.Real = length;

                        comObject.SetElement(i, j, member);
                    }
                }

                ws.SetValue("i.x", comObject);
            
                ws.Recalculate();

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
                Console.WriteLine(ex);
                StringBuilder exSb = new StringBuilder();
                exSb.Append(ex.Message);
                exSb.AppendLine();
                op.WriteDebugFile(_debugFilePath, exSb);
            }
        }
    }
}
