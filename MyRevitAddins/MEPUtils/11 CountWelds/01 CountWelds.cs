using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils._00_SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Input;
using dbg = Shared.Dbg;
using fi = Shared.Filter;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using mp = Shared.MepUtils;
using tr = Shared.Transformation;
using ut = Shared.BuildingCoder.Util;

namespace MEPUtils.CountWelds
{
    public class CountWelds
    {
        public Result CountWeldsMethod(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            try
            {
                bool ctrl = false;
                if ((int)Keyboard.Modifiers == 2) ctrl = true;

                string pathToExport = MEPUtils.Properties.Settings.Default.CountWelds_PathToExportJson;

                //If the name of path is null or empty for some reason -- reinitialize
                if (string.IsNullOrEmpty(pathToExport)) ctrl = true;

                if (ctrl)
                {
                    CommonOpenFileDialog dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = true
                    };
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        pathToExport = dialog.FileName; //"\\" is added in the output part.
                        Properties.Settings.Default.CountWelds_PathToExportJson = dialog.FileName;
                        Properties.Settings.Default.Save();
                    }
                }

                //Gather all connectors from the document
                HashSet<Connector> AllCons = mp.GetALLConnectorsInDocument(doc);
                //var Pipes = fi.GetElements<Element, BuiltInCategory>(doc, BuiltInCategory.OST_PipeCurves);
                //var Fittings = fi.GetElements<Element, BuiltInCategory>(doc, BuiltInCategory.OST_PipeFitting);
                //var Accessories = fi.GetElements<Element, BuiltInCategory>(doc, BuiltInCategory.OST_PipeAccessory);

                //Apply some early filtering
                //1) Connectors from ARGD system disallowed
                //Pipes = Pipes.ExceptWhere(x => x.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString() == "Rigid").ToHashSet();
                AllCons = AllCons.ExceptWhere(x => x.MEPSystemAbbreviation(doc) == "ARGD").ToHashSet();

                //Gather all connectors   
                //TODO: Continue here!

                //Create collection with distinct connectors
                var DistinctCons = AllCons.ToHashSet(new ConnectorXyzComparer2());

                StringBuilder sb = new StringBuilder();

                int count = 0;

                List<double> deltas = new List<double>();

                foreach (var item in DistinctCons)
                {
                    var query = DistinctCons.Where(x => item.IsEqual(x, 0.001)).ToList();
                    if (query.Count() > 1)
                    {
                        Connector first = query.First();
                        Connector last = query.Last();

                        count++;
                        sb.AppendLine(count.ToString());
                        sb.AppendLine(first.Owner.Id.ToString());
                        sb.AppendLine(last.Owner.Id.ToString());
                        sb.AppendLine("X1: " + first.Origin.X);
                        sb.AppendLine("X2: " + last.Origin.X);
                        sb.AppendLine("DX: " + (first.Origin.X - last.Origin.X));
                        sb.AppendLine("Y1: " + first.Origin.Y);
                        sb.AppendLine("Y2: " + last.Origin.Y);
                        sb.AppendLine("DY: " + (first.Origin.Y - last.Origin.Y));
                        sb.AppendLine("Z1: " + first.Origin.Z);
                        sb.AppendLine("Z2: " + last.Origin.Z);
                        sb.AppendLine("DZ: " + (first.Origin.Z - last.Origin.Z));
                        sb.AppendLine(Comparer.HashString(first.Origin));
                        sb.AppendLine(Comparer.HashString(last.Origin));
                        sb.AppendLine();

                        deltas.Add(Math.Abs(first.Origin.X - last.Origin.X));
                        deltas.Add(Math.Abs(first.Origin.Y - last.Origin.Y));
                        deltas.Add(Math.Abs(first.Origin.Z - last.Origin.Z));
                    }
                }

                //sb.AppendLine("Largest delta: " + deltas.Max());
                sb.AppendLine();

                // Clear the output file
                System.IO.File.WriteAllBytes(pathToExport + "\\XYZ_Accuracy.txt", new byte[0]);

                // Write to output file
                using (StreamWriter w = File.AppendText(pathToExport + "\\XYZ_Accuracy.txt"))
                {
                    w.Write(sb);
                    w.Close();
                }

                //For each distinct connector find the corresponding local spatial group connectors
                List<connectorSpatialGroup> csgList = new List<connectorSpatialGroup>();
                foreach (Connector distinctCon in DistinctCons)
                {
                    csgList.Add(new connectorSpatialGroup(AllCons.Where(x => distinctCon.IsEqual(x))));
                    //AllCons = AllCons.ExceptWhere(x => distinctCon.IsEqual(x)).ToHashSet();
                }

                //Write serialized data
                var ser = new DataContractJsonSerializer(typeof(List<connectorSpatialGroup>));

                StringBuilder json = new StringBuilder();

                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, csgList);
                    string output = Encoding.UTF8.GetString(ms.ToArray());
                    json.Append(output);
                }

                string filename = pathToExport + "\\CountWelds.json";

                //Clear the output file
                System.IO.File.WriteAllBytes(filename, new byte[0]);

                // Write to output file
                using (StreamWriter w = File.AppendText(filename))
                {
                    w.Write(json);
                    w.Close();
                }

                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    [DataContract]
    internal class connectorSpatialGroup
    {
        public List<Connector> Connectors = new List<Connector>();
        //Welds can only be of one DN
        [DataMember]
        double DN = 0;
        [DataMember]
        int nrOfCons = 0;

        public connectorSpatialGroup(IEnumerable<Connector> collection)
        {
            Connectors = collection.ToList();

            //Determine DN of weld
            Connector sampleCon = Connectors.FirstOrDefault();
            DN = (sampleCon.Radius * 2).FtToMm().Round();

            nrOfCons = Connectors.Count();
        }
    }
}
