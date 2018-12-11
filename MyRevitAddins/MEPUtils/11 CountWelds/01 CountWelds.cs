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

                //Apply some early filtering
                //1) Connectors from ARGD system disallowed (Rigids for equipment analytical model)
                AllCons = AllCons.ExceptWhere(x => x.MEPSystemAbbreviation(doc) == "ARGD").ToHashSet();

                //2) Remove all "Curve" Connectors -- Olet welds are counted in comp. schedule
                AllCons = AllCons.ExceptWhere(x => x.ConnectorType == ConnectorType.Curve).ToHashSet();

                //Create collection with distinct connectors
                var DistinctCons = AllCons.ToHashSet(new ConnectorXyzComparer());

                #region Debug ConnectorXyzComparer
                //StringBuilder sb = new StringBuilder();
                //int count = 0;
                //List<double> deltas = new List<double>();
                //foreach (var item in DistinctCons)
                //{
                //    var query = DistinctCons.Where(x => item.IsEqual(x, 0.001)).ToList();
                //    if (query.Count() > 1)
                //    {
                //        Connector first = query.First();
                //        Connector last = query.Last();

                //        count++;
                //        sb.AppendLine(count.ToString());
                //        sb.AppendLine(first.Owner.Id.ToString());
                //        sb.AppendLine(last.Owner.Id.ToString());
                //        sb.AppendLine("X1: " + first.Origin.X);
                //        sb.AppendLine("X2: " + last.Origin.X);
                //        sb.AppendLine("DX: " + (first.Origin.X - last.Origin.X));
                //        sb.AppendLine("Y1: " + first.Origin.Y);
                //        sb.AppendLine("Y2: " + last.Origin.Y);
                //        sb.AppendLine("DY: " + (first.Origin.Y - last.Origin.Y));
                //        sb.AppendLine("Z1: " + first.Origin.Z);
                //        sb.AppendLine("Z2: " + last.Origin.Z);
                //        sb.AppendLine("DZ: " + (first.Origin.Z - last.Origin.Z));
                //        sb.AppendLine(Comparer.HashString(first.Origin));
                //        sb.AppendLine(Comparer.HashString(last.Origin));
                //        sb.AppendLine();

                //        deltas.Add(Math.Abs(first.Origin.X - last.Origin.X));
                //        deltas.Add(Math.Abs(first.Origin.Y - last.Origin.Y));
                //        deltas.Add(Math.Abs(first.Origin.Z - last.Origin.Z));
                //    }
                //}
                ////sb.AppendLine("Largest delta: " + deltas.Max());
                //sb.AppendLine();
                //// Clear the output file
                //System.IO.File.WriteAllBytes(pathToExport + "\\XYZ_Accuracy.txt", new byte[0]);
                //// Write to output file
                //using (StreamWriter w = File.AppendText(pathToExport + "\\XYZ_Accuracy.txt"))
                //{
                //    w.Write(sb);
                //    w.Close();
                //}
                #endregion

                //For each distinct connector find the corresponding local spatial group connectors
                List<connectorSpatialGroup> csgList = new List<connectorSpatialGroup>();
                foreach (Connector distinctCon in DistinctCons)
                {
                    csgList.Add(new connectorSpatialGroup(AllCons.Where(x => distinctCon.Equalz(x, Shared.Extensions._1mmTol))));
                    AllCons = AllCons.ExceptWhere(x => distinctCon.Equalz(x, Shared.Extensions._1mmTol)).ToHashSet();
                }

                #region Filtering
                //1) If SpatialGroup contains only one connector -> discard
                csgList = csgList.ExceptWhere(x => x.nrOfCons < 2).ToList();

                //2) If all specs are EXISTING ignore group
                csgList = csgList.ExceptWhere(x => x.SpecList.Distinct().Count() < 2 && x.SpecList.First() == "EXISTING" ).ToList();
                #endregion

                #region Analysis
                foreach (var csg in csgList)
                {
                    //csg.
                }
                #endregion

                #region QualityAssurance
                //List<string> allids = new List<string>();
                //foreach (connectorSpatialGroup csg in csgList)
                //{
                //    foreach (Connector con in csg.Connectors)
                //    {
                //        Element owner = con.Owner;
                //        ElementId elId = owner.Id;
                //        string elIdString = elId.ToString();
                //        allids.Add(elIdString);
                //    }
                //}

                ////using (Transaction tx = new Transaction(doc))
                ////{
                ////    tx.Start("Place marker!");
                ////    Shared.Dbg.PlaceAdaptiveFamilyInstance(doc, "Marker Line: Red", new XYZ(15.658937314, 3.170790710, 9.219160105), new XYZ());
                ////    tx.Commit();

                ////}

                //Dictionary<string, int> grp = allids.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

                #endregion

                //Write serialized data
                var ser = new DataContractJsonSerializer(typeof(List<connectorSpatialGroup>));
                //var ser = new DataContractJsonSerializer(typeof(Dictionary<string, int>));

                StringBuilder json = new StringBuilder();

                using (var ms = new MemoryStream())
                {
                    ser.WriteObject(ms, csgList);
                    //ser.WriteObject(ms, grp);
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

                //var watch = System.Diagnostics.Stopwatch.StartNew();
                //Code()
                //watch.Stop();
                //Shared.BuildingCoder.BuildingCoderUtilities.InfoMsg(watch.ElapsedMilliseconds.ToString());

                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }

    internal enum ConnectionType
    {
        Uninitialized, //Assigned at object creation -> can be used to check if alle objects are processed -> all objects must get another type assigned
        Unknown, //Assigned if the program cannot determine the connection type -> the program must be modified to accomodate for causing case
        Invalid, //The conditions state that the connection should not be there at all -> investigate
        PipeToPipe,
        PipeToFitting,
        PipeToAccessory,
        FittingToFitting,
        FittingToAccessory,
        PipeSupport
    }

    [DataContract]
    internal class connectorSpatialGroup
    {
        public List<Connector> Connectors = new List<Connector>();
        //Welds can only be of one DN
        [DataMember]
        public double DN = 0;
        [DataMember]
        public int nrOfCons = 0;
        [DataMember]
        public List<string> SpecList = new List<string>();
        [DataMember]
        ConnectionType connectionType = 0;
        [DataMember]
        Element pipe1 = null;
        [DataMember]
        Element pipe2 = null;
        [DataMember]
        Element fitting1 = null;
        [DataMember]
        Element fitting2 = null;
        [DataMember]
        Element accessory1 = null;
        [DataMember]
        Element accessory2 = null;
        [DataMember]
        Element pipeSupport = null;

        internal connectorSpatialGroup(IEnumerable<Connector> collection)
        {
            Connectors = collection.ToList();
            nrOfCons = Connectors.Count();
            foreach (Connector con in collection)
            {
                Element owner = con.Owner;
                Parameter par = owner.get_Parameter(new Guid("90be8246-25f7-487d-b352-554f810fcaa7")); //PCF_ELEM_SPEC parameter
                SpecList.Add(par.AsString());
            }
        }

        public void Analyze()
        {
            //Determine DN of weld
            Connector sampleCon = Connectors.FirstOrDefault();
            DN = (sampleCon.Radius * 2).FtToMm().Round();

            //Analyze connectors
            switch (nrOfCons)
            {
                case 1:
                    connectionType = ConnectionType.Invalid;
                    break;
                case 2:
                    Element firstEl = null;
                    Element secondEl = null;
                    BuiltInCategory firstCat = BuiltInCategory.INVALID;
                    BuiltInCategory secondCat = BuiltInCategory.INVALID;

                    int counter = 0;
                    foreach (Connector con in Connectors)
                    {
                        Element owner = con.Owner;
                        Category cat = owner.Category;
                        BuiltInCategory bic = (BuiltInCategory)cat.Id.IntegerValue;

                        if (counter == 0) { firstEl = owner; firstCat = bic; }
                        else { secondEl = owner; secondCat = bic; }
                        counter++;
                    }

                    switch (firstCat)
                    {
                        case BuiltInCategory.OST_PipeCurves:
                            break;
                        default:
                            break;
                    }

                    break;
                case 3:
                    break;
                case 4:
                    break;
                default:
                    break;
            }
        }
    }
}
