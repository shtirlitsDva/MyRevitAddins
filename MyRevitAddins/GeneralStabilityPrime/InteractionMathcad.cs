using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Mathcad = Ptc.MathcadPrime.Automation;
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

        public InteractionMathcad(Document doc, Mathcad.IMathcadPrimeWorksheet ws)
        {
            int rows, cols;
            Mathcad.IMathcadPrimeMatrix matrix;

            InteractionRevit ir = new InteractionRevit(doc);

            try
            {
                Mathcad.IMathcadPrimeWorksheet3 ws3 = (Mathcad.IMathcadPrimeWorksheet3) ws;

                //Walls along first
                cols = 1; rows = ir.WallsAlong.WallSymbols.Count;
                //Init the i variable
                ws3.SetRealValue("numberWallsAlong", rows, "");
                //Length of walls along: lx
                matrix = ws3.CreateMatrix(rows, cols);
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        matrix.SetMatrixElement(i, j, ir.WallsAlong.Length[i]);
                    }
                }
                ws3.SetMatrixValue("lx", matrix, "m");

                //Distance from Origo to the wall centreline: y
                StringBuilder wsSb = new StringBuilder();
                foreach (double y in ir.WallsAlong.Y)
                {
                    wsSb.Append(ut.FootToMeter(y));
                    wsSb.AppendLine();
                }
                op.WriteDebugFile(_debugFilePath, wsSb);

                #region Development

                //StringBuilder wsSb = new StringBuilder();
                //wsSb.Append(ws.FullName);
                //wsSb.AppendLine();
                //wsSb.Append(ws.Inputs.Count);
                //wsSb.AppendLine();
                //wsSb.Append(ws.IsReadOnly);
                //wsSb.AppendLine();
                //wsSb.Append(ws.Modified);
                //wsSb.AppendLine();
                //wsSb.Append(ws.Name);
                //wsSb.AppendLine();
                //wsSb.Append(ws.Outputs.Count);
                //wsSb.AppendLine();

                //Mathcad.IMathcadPrimeInputs inputs = ws.Inputs;
                //Mathcad.IMathcadPrimeOutputs outputs = ws.Outputs;

                //wsSb.Append("Inputs");
                //wsSb.AppendLine();
                //for (var i = 0; i < inputs.Count; i++)
                //{
                //    var input = inputs.GetAliasByIndex(i);
                //    wsSb.Append(input);
                //    wsSb.AppendLine();
                //}

                //wsSb.Append("Outputs");
                //wsSb.AppendLine();
                //for (var i = 0; i < outputs.Count; i++)
                //{
                //    var output = outputs.GetAliasByIndex(i);
                //    wsSb.Append(output);
                //    wsSb.AppendLine();
                //}

                //op.WriteDebugFile(_debugFilePath, wsSb);

                //System.Type objType = System.Type.GetTypeFromProgID("Mathcad.MatrixValue");

                //dynamic comObject = System.Activator.CreateInstance(objType);

                //rows = ir.WallsAlong.Count;
                //cols = 1;

                //for (int i = 0; i < rows; i++)
                //{
                //    for (int j = 0; j < cols; j++)
                //    {
                //        LocationCurve loc = ir.WallsAlong[i].Location as LocationCurve;
                //        double length = ut.FootToMeter(loc.Curve.Length);

                //        NumericValue member = new NumericValue();
                //        member.Real = length;

                //        comObject.SetElement(i, j, member);
                //    }
                //}

                //ws.SetValue("i.x", comObject);

                //ws.Recalculate();

                ////Regions regions = ws.Regions;

                //var value = ws.GetValue("i.x") as MatrixValue;

                //StringBuilder sb2 = new StringBuilder();

                //sb2.Append(value.AsString);
                //sb2.AppendLine();

                //rows = value.Rows;
                //cols = value.Cols;

                //for (int i = 0; i < rows; i++)
                //{
                //    for (int j = 0; j < cols; j++)
                //    {
                //        INumericValue numval = value.GetElement(i, j);
                //        sb2.Append(numval.Real.ToString());
                //        sb2.AppendLine();
                //    }
                //}
                //op.WriteDebugFile(_debugFilePath, sb2);
                //double[] matrix = new double[ir.WallsAlong.Count];

                //for (int i = 0; i < ir.WallsAlong.Count; i++)
                //{
                //    FamilyInstance fi = ir.WallsAlong[i];
                //    LocationCurve loc = fi.Location as LocationCurve;
                //    double length = Util.FootToMeter(loc.Curve.Length);
                //    matrix[i] = length;
                //}

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



                //ws.SetValue("i.x", matrix);

                //ws.Recalculate();
                //ws.Save();

                //Regions regions = ws.Regions;

                //var value = ws.GetValue("i.x") as MatrixValue;

                //StringBuilder sb2 = new StringBuilder();

                //sb2.Append(value.AsString);
                //sb2.AppendLine();

                //rows = value.Rows;
                //cols = value.Cols;

                //for (int i = 0; i < rows; i++)
                //{
                //    for (int j = 0; j < cols; j++)
                //    {
                //        INumericValue numval = value.GetElement(i, j);
                //        sb2.Append(numval.Real.ToString());
                //        sb2.AppendLine();
                //    }
                //}
                //op.WriteDebugFile(_debugFilePath, sb2);

                #endregion

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
