using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using dbg = Shared.Dbg;
using fi = Shared.Filter;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using mp = Shared.MepUtils;
using tr = Shared.Transformation;

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

                    //TODO: Implement a selection of: 1) Point on a pipe selected directly 2) Distance from element on the pipe
                    using (Transaction trans1 = new Transaction(doc))
                    {
                        trans1.Start("SelectPipePoint");
                        (selectedPipe, iP) = SelectPipePoint(doc, uidoc);
                        trans1.Commit();
                    }

                    string operation;

                    //Select operation to perform
                    BaseFormTableLayoutPanel_Basic op = new BaseFormTableLayoutPanel_Basic(lad.Operations());
                    op.ShowDialog();
                    operation = op.strTR;

                    string direction;

                    //Select the direction to create in
                    BaseFormTableLayoutPanel_Basic ds = new BaseFormTableLayoutPanel_Basic(lad.Directions());
                    ds.ShowDialog();
                    direction = ds.strTR;
                    //ut.InfoMsg(ds.strTR);

                    string PipeTypeName;
                    PipeType pipeType;
                    double size;
                    FamilyInstance olet;
                    ElementId curLvlId;
                    ElementId curPipingSysTypeId;
                    ElementId curPipeTypeId;

                    switch (operation)
                    {
                        case "Auto ML":
                            using (Transaction trans2 = new Transaction(doc))
                            {
                                trans2.Start("Auto ML");
                                Element dummyPipe;
                                (olet, dummyPipe) = CreateOlet(doc, iP, direction, selectedPipe, 20, "Stålrør, sømløse sockolet");
                                doc.Delete(dummyPipe.Id);
                                doc.Regenerate();
                                
                                //"DN20-SM-EL: Udluftn."
                                Element cpValve = createNextElement(doc, olet, "DN20-SM-EL: Udluftn.");
                                if (cpValve == null) throw new Exception("Creation of cpValve failed for some reason!");

                                Element mlValve = createNextElement(doc, cpValve,
                                    "Spirotop-Autotomatic-Air-Vent-PN25: Spirotop 1/2  150°C 25bar AB050/025");
                                if (mlValve == null) throw new Exception("Creation of mlValve failed for some reason!");

                                trans2.Commit();
                            }
                            break;
                        case "PT":
                            using (Transaction trans3 = new Transaction(doc))
                            {
                                trans3.Start("PT");

                                Element dummyPipe;
                                (olet, dummyPipe) = CreateOlet(doc, iP, direction, selectedPipe, 15, "Stålrør, sømløse sockolet");
                                doc.Delete(dummyPipe.Id);
                                doc.Regenerate();

                                //"DN15-SM-EL: SM-EL"
                                Element cpValve = createNextElement(doc, olet, "DN15-SM-EL: SM-EL");
                                if (cpValve == null) throw new Exception("Creation of cpValve failed for some reason!");

                                Element instr = createNextElement(doc, cpValve, "Sitrans_P200: Standard");
                                if (instr == null) throw new Exception("Creation of instrument failed for some reason!");

                                trans3.Commit();
                            }
                            break;
                        case "Manometer":
                            using (Transaction trans4 = new Transaction(doc))
                            {
                                trans4.Start("Manometer");
                                Element dummyPipe;
                                (olet, dummyPipe) = CreateOlet(doc, iP, direction, selectedPipe, 15, "Stålrør, sømløse sockolet");
                                doc.Delete(dummyPipe.Id);
                                doc.Regenerate();

                                Element cpValve = createNextElement(doc, olet, "DN15-SM-EL: SM-EL");
                                if (cpValve == null) throw new Exception("Creation of cpValve failed for some reason!");

                                //TODO: Places the instrument at wrong angle!!!!
                                Element instr = createNextElement(doc, cpValve, "WIKA.Manometer.233.50.100: Standard");
                                if (instr == null) throw new Exception("Creation of instrument failed for some reason!");

                                trans4.Commit();
                            }
                            break;
                        case "TT":
                            using (Transaction trans5 = new Transaction(doc))
                            {
                                trans5.Start("Temperaturtransmitter");
                                Element dummyPipe;
                                (olet, dummyPipe) = CreateOlet(doc, iP, direction, selectedPipe, 20, "Stålrør, sømløse, termolomme");
                                doc.Delete(dummyPipe.Id);
                                doc.Regenerate();

                                Element cpValve = createNextElement(doc, olet, "DN15-SM-EL: SM-EL");
                                if (cpValve == null) throw new Exception("Creation of cpValve failed for some reason!");

                                //TODO: Places the instrument at wrong angle!!!!
                                Element instr = createNextElement(doc, cpValve, "WIKA.Manometer.233.50.100: Standard");
                                if (instr == null) throw new Exception("Creation of instrument failed for some reason!");

                                trans5.Commit();
                            }
                            break;
                        case "Termometer":
                            break;
                        case "Pipe":
                            #region "Case PIPE"
                            //Select type of Olet
                            BaseFormTableLayoutPanel_Basic oletSelector = new BaseFormTableLayoutPanel_Basic(lad.PipeTypeByOlet());
                            oletSelector.ShowDialog();
                            PipeTypeName = oletSelector.strTR;
                            //ut.InfoMsg(PipeTypeName);
                            pipeType = fi.GetElements<PipeType, BuiltInParameter>(doc, BuiltInParameter.SYMBOL_NAME_PARAM, PipeTypeName).First();

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
                            size = double.Parse(sizeSelector.strTR);

                            curLvlId = selectedPipe.ReferenceLevel.Id;
                            curPipingSysTypeId = selectedPipe.MEPSystem.GetTypeId();
                            curPipeTypeId = pipeType.Id;

                            using (Transaction trans2 = new Transaction(doc))
                            {
                                trans2.Start("Create Olet");

                                Element dummyPipe;
                                (olet, dummyPipe) = CreateOlet(doc, iP, direction, selectedPipe, size, oletSelector.strTR);
                                if (olet == null || dummyPipe == null)
                                {
                                    txGp.RollBack();
                                    return Result.Cancelled;
                                };

                                //dbg.PlaceAdaptiveFamilyInstance(doc, "Marker Line: Red", offsetPoint, dirPoint);

                                trans2.Commit();
                            }
                            #endregion
                            break;
                        default:
                            return Result.Cancelled;
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

        private static Element createNextElement(Document doc, Element prevElem, string elemFamType)
        {
            Cons prevElemCons = mp.GetConnectors(prevElem);

            Element elementSymbol = fi.GetElements<FamilySymbol, BuiltInParameter>(
                doc, BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM, elemFamType).FirstOrDefault();
            if (elementSymbol == null) throw new Exception(elemFamType + " not found!");

            Element elem = doc.Create.NewFamilyInstance(prevElemCons.Secondary.Origin, (FamilySymbol)elementSymbol,
                                                           StructuralType.NonStructural);
            doc.Regenerate();
            Cons elemCons = mp.GetConnectors(elem);

            RotateElementInPosition(prevElemCons.Secondary.Origin, elemCons.Primary,
                        prevElemCons.Secondary, prevElemCons.Primary, elem);

            ElementTransformUtils.MoveElement(doc, elem.Id,
                prevElemCons.Secondary.Origin - elemCons.Primary.Origin);

            return elem;
        }

        private static void RotateElementInPosition(XYZ placementPoint, Connector conOnFamilyToConnect, Connector start, Connector end, Element element)
        {
            #region Geometric manipulation

            //http://thebuildingcoder.typepad.com/blog/2012/05/create-a-pipe-cap.html

            XYZ dir = (start.Origin - end.Origin);

            // rotate the cap if necessary
            // rotate about Z first

            XYZ pipeHorizontalDirection = new XYZ(dir.X, dir.Y, 0.0).Normalize();
            //XYZ pipeHorizontalDirection = new XYZ(dir.X, dir.Y, 0.0);

            XYZ connectorDirection = -conOnFamilyToConnect.CoordinateSystem.BasisZ;

            double zRotationAngle = pipeHorizontalDirection.AngleTo(connectorDirection);

            Transform trf = Transform.CreateRotationAtPoint(XYZ.BasisZ, zRotationAngle, placementPoint);

            XYZ testRotation = trf.OfVector(connectorDirection).Normalize();

            if (Math.Abs(testRotation.DotProduct(pipeHorizontalDirection) - 1) > 0.00001) zRotationAngle = -zRotationAngle;

            Line axis = Line.CreateBound(placementPoint, placementPoint + XYZ.BasisZ);

            ElementTransformUtils.RotateElement(element.Document, element.Id, axis, zRotationAngle);

            //Parameter comments = element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            //comments.Set("Horizontal only");

            // Need to rotate vertically?

            if (Math.Abs(dir.DotProduct(XYZ.BasisZ)) > 0.000001)
            {
                // if pipe is straight up and down, 
                // kludge it my way else

                if (dir.X.Round(3) == 0 && dir.Y.Round(3) == 0 && dir.Z.Round(3) != 0)
                {
                    XYZ yaxis = new XYZ(0.0, 1.0, 0.0);

                    double rotationAngle = dir.AngleTo(yaxis); //<-- value in radians

                    if (dir.Z > 0) rotationAngle = -rotationAngle; //<-- Here is the culprit: Equals(1) was wrong!

                    axis = Line.CreateBound(placementPoint, new XYZ(placementPoint.X, placementPoint.Y + 5, placementPoint.Z));

                    ElementTransformUtils.RotateElement(element.Document, element.Id, axis, rotationAngle);

                    //comments.Set("Vertical!");
                }
                else
                {
                    #region sloped pipes

                    double rotationAngle = dir.AngleTo(pipeHorizontalDirection);

                    XYZ normal = pipeHorizontalDirection.CrossProduct(XYZ.BasisZ);

                    trf = Transform.CreateRotationAtPoint(normal, rotationAngle, placementPoint);

                    testRotation = trf.OfVector(dir).Normalize();

                    if (Math.Abs(testRotation.DotProduct(pipeHorizontalDirection) - 1) < 0.00001)
                        rotationAngle = -rotationAngle;

                    axis = Line.CreateBound(placementPoint, placementPoint + normal);

                    ElementTransformUtils.RotateElement(element.Document, element.Id, axis, rotationAngle);

                    //comments.Set("Sloped");

                    #endregion
                }
            }
            #endregion
        }


        private static (FamilyInstance, Pipe) CreateOlet(Document doc, XYZ iP, string direction,
                                                         Pipe selectedPipe, double size, string PipeTypeName)
        {
            PipeType pipeType = fi.GetElements<PipeType, BuiltInParameter>(doc, BuiltInParameter.SYMBOL_NAME_PARAM, PipeTypeName).FirstOrDefault();
            if (pipeType == null) throw new Exception(PipeTypeName + " does not exist in current project!");

            ElementId curLvlId = selectedPipe.ReferenceLevel.Id;
            ElementId curPipingSysTypeId = selectedPipe.MEPSystem.GetTypeId();
            ElementId curPipeTypeId = pipeType.Id;

            XYZ dirPoint = CreateDummyDirectionPoint(iP, direction);
            if (dirPoint == null) return (null, null);

            Pipe dummyPipe = Pipe.Create(doc, curPipingSysTypeId, curPipeTypeId, curLvlId, iP, dirPoint);

            //Change size of the pipe
            Parameter par = dummyPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            par.Set(size.MmToFt());

            //Find the connector from the dummy pipe at intersection
            var cons = mp.GetALLConnectorsFromElements(dummyPipe);
            Connector con = cons.Where(c => c.Origin.Equalz(iP, Extensions._1mmTol)).FirstOrDefault();

            return (doc.Create.NewTakeoffFitting(con, (MEPCurve)selectedPipe), dummyPipe);

            //dbg.PlaceAdaptiveFamilyInstance(doc, "Marker Line: Red", offsetPoint, dirPoint);
        }

        private static XYZ CreateDummyDirectionPoint(XYZ iP, string direction)
        {
            //Create direction point
            switch (direction)
            {
                case "Top":
                    return new XYZ(iP.X, iP.Y, iP.Z + 5);
                case "Bottom":
                    return new XYZ(iP.X, iP.Y, iP.Z - 5);
                case "Front":
                    return new XYZ(iP.X, iP.Y - 5, iP.Z);
                case "Back":
                    return new XYZ(iP.X, iP.Y + 5, iP.Z);
                case "Left":
                    return new XYZ(iP.X - 5, iP.Y, iP.Z);
                case "Right":
                    return new XYZ(iP.X + 5, iP.Y, iP.Z);
                default:
                    return null;
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
