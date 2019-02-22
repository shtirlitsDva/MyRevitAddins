using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using fi = Shared.Filter;
using tr = Shared.Transformation;
using mp = Shared.MepUtils;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using dbg = Shared.Dbg;

namespace MEPUtils.CreateInstrumentation
{
    public class StartCreatingInstrumentation
    {
        public static Result StartCreating(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            try
            {
                using (TransactionGroup txGp = new TransactionGroup(doc))
                {
                    txGp.Start("Create Instrumentation!");

                    Pipe selectedPipe;
                    XYZ iP;

                    using (Transaction trans1 = new Transaction(doc))
                    {
                        trans1.Start("SelectPipePoint");
                        (selectedPipe, iP) = SelectPipePoint(doc, uidoc);
                        trans1.Commit();
                    }

                    string direction;

                    //Select the direction to create in
                    BaseFormTableLayoutPanel_Basic ds = new BaseFormTableLayoutPanel_Basic(lad.Directions());
                    ds.ShowDialog();
                    direction = ds.strTR;
                    //ut.InfoMsg(ds.strTR);

                    string PipeTypeName;

                    //Select type of Olet
                    BaseFormTableLayoutPanel_Basic oletSelector = new BaseFormTableLayoutPanel_Basic(lad.PipeTypeByOlet());
                    oletSelector.ShowDialog();
                    PipeTypeName = oletSelector.strTR;
                    //ut.InfoMsg(PipeTypeName);
                    PipeType pipeType = fi.GetElements<PipeType, BuiltInParameter>(doc, BuiltInParameter.SYMBOL_NAME_PARAM, PipeTypeName).First();

                    //Limit sizes for Olets
                    List<string> sizeListing;
                    switch (oletSelector.strTR)
                    {
                        case "Stålrør, sømløse, termolomme":
                        case "Stålrør, sømløse sockolet":
                            sizeListing = lad.SockoletList();
                            break;
                        case "Stålrør, sømløse weldolet":
                            sizeListing = lad.WeldoletList();
                            break;
                        default:
                            sizeListing = lad.SizeList();
                            break;
                    }

                    BaseFormTableLayoutPanel_Basic sizeSelector = new BaseFormTableLayoutPanel_Basic(sizeListing);
                    sizeSelector.ShowDialog();
                    double size = double.Parse(sizeSelector.strTR);

                    FamilyInstance olet;
                    XYZ dirPoint = null;

                    ElementId curLvlId = selectedPipe.ReferenceLevel.Id;
                    ElementId curPipingSysTypeId = selectedPipe.MEPSystem.GetTypeId();
                    ElementId curPipeTypeId = pipeType.Id;

                    using (Transaction trans2 = new Transaction(doc))
                    {
                        trans2.Start("Create Olet");

                        //Create direction point
                        switch (direction)
                        {
                            case "Top":
                                dirPoint = new XYZ(iP.X, iP.Y, iP.Z + 5);
                                break;
                            case "Bottom":
                                dirPoint = new XYZ(iP.X, iP.Y, iP.Z - 5);
                                break;
                            case "Front":
                                dirPoint = new XYZ(iP.X, iP.Y - 5, iP.Z);
                                break;
                            case "Back":
                                dirPoint = new XYZ(iP.X, iP.Y + 5, iP.Z);
                                break;
                            case "Left":
                                dirPoint = new XYZ(iP.X - 5, iP.Y, iP.Z);
                                break;
                            case "Right":
                                dirPoint = new XYZ(iP.X + 5, iP.Y, iP.Z);
                                break;
                            default:
                                break;
                        }

                        Pipe dummyPipe = Pipe.Create(doc, curPipingSysTypeId, curPipeTypeId, curLvlId, iP, dirPoint);

                        //Change size of the pipe
                        Parameter par = dummyPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                        par.Set(size.MmToFt());

                        //Find the connector from the dummy pipe at intersection
                        var cons = mp.GetALLConnectorsFromElements(dummyPipe);
                        Connector con = cons.Where(c => c.Origin.Equalz(iP, Extensions._1mmTol)).FirstOrDefault();

                        olet = doc.Create.NewTakeoffFitting(con, (MEPCurve)selectedPipe);

                        //dbg.PlaceAdaptiveFamilyInstance(doc, "Marker Line: Red", offsetPoint, dirPoint);

                        trans2.Commit();
                    }

                    txGp.Assimilate();
                }

                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static (Pipe pipe, XYZ point) SelectPipePoint(Document doc, UIDocument uidoc)
        {
            //Select the pipe to operate on
            var selectedPipe = Shared.BuildingCoder.BuildingCoderUtilities.SelectSingleElementOfType(uidoc, typeof(Pipe),
                "Select a pipe where to place a support!", false);
            //Get end connectors
            var conQuery = (from Connector c in mp.GetALLConnectorsFromElements(selectedPipe)
                            where (int)c.ConnectorType == 1
                            select c).ToList();

            Connector c1 = conQuery.First();
            Connector c2 = conQuery.Last();

            //Define a plane by three points
            //Detect if the pipe concides with X-axis
            //If true use another axis to define point
            Plane plane;

            if (c1.Origin.Y.Equalz(c2.Origin.Y, Extensions._epx) && c1.Origin.Z.Equalz(c2.Origin.Z, Extensions._epx))
                plane = Plane.CreateByThreePoints(c1.Origin, c2.Origin, new XYZ(c1.Origin.X, c1.Origin.Y + 5, c1.Origin.Z));
            else
                plane = Plane.CreateByThreePoints(c1.Origin, c2.Origin, new XYZ(c1.Origin.X + 5, c1.Origin.Y, c1.Origin.Z));

            //Set view sketch plane to the be the created plane
            var sp = SketchPlane.Create(doc, plane);
            uidoc.ActiveView.SketchPlane = sp;
            //Get a 3d point by picking a point
            XYZ point_in_3d = null;
            try { point_in_3d = uidoc.Selection.PickPoint("Please pick a point on the plane defined by the selected face"); }
            catch (OperationCanceledException) { }


            return ((Pipe)selectedPipe, point_in_3d);
        }
    }
}
