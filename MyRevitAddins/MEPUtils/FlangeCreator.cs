using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace MEPUtils
{
    class FlangeCreator
    {
        public static Result CreateFlangeForElements(ExternalCommandData cData)
        {
            try
            {
                Document doc = cData.Application.ActiveUIDocument.Document;

                //One element selected, creates pipe at random connector
                Selection selection = cData.Application.ActiveUIDocument.Selection;
                var elemIds = selection.GetElementIds();
                if (elemIds == null) throw new Exception("Getting element from selection failed!");

                //Choose the right flange to create
                FlangeCreatorChooser fcc = new FlangeCreatorChooser(cData);
                fcc.ShowDialog();
                fcc.Close();
                string familyAndTypeName = fcc.flangeName;

                //Collect the family symbol of the flange
                var collector = new FilteredElementCollector(doc);
                var symbolFilter = fi.ParameterValueFilter(familyAndTypeName,
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
                var classFilter = fi.FamSymbolsAndPipeTypes();
                Element familySymbol = collector.WherePasses(classFilter).WherePasses(symbolFilter).FirstElement();

                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("Create flanges");

                    //Process the elements
                    foreach (var id in elemIds)
                    {
                        Element element = doc.GetElement(id);
                        if (element is Pipe) throw new Exception("This method does not work on pipes!");
                        var origCons = mp.GetConnectors(element);

                        #region Primary flange
                        CreateFlange(doc, origCons.Primary, origCons.Secondary, familySymbol, element);
                        #endregion

                        #region Secondary flange
                        CreateFlange(doc, origCons.Secondary, origCons.Primary, familySymbol, element);
                        #endregion
                    }
                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private static void CreateFlange(Document doc, Connector start, Connector end, Element familySymbol, Element element)
        {
            //TODO: Handle case with reducers at ends
            //TODO: Set correct system type for the new elements
            //Gather the information about the connected elements
            var allRefs = start.AllRefs;
            Connector modCon1 = null;
            foreach (Connector c in allRefs) modCon1 = c;

            //If the connector is connected to anything else than a Pipe then abort
            //Case: connected to other element than pipe
            if (modCon1 != null && !(modCon1.Owner is Pipe)) return;

            //Create the flange (must be rotated AND moved in place)
            Element flange = doc.Create.NewFamilyInstance(start.Origin, (FamilySymbol)familySymbol,
                StructuralType.NonStructural);

            //Set the diameter of the flange
            double diaValue = start.Radius * 2;
            Parameter dia = flange.LookupParameter("Nominal Diameter 1");
            dia.Set(diaValue);

            doc.Regenerate();

            //Access the newly created flange's connectors
            var flangeCons = mp.GetConnectors(flange);

            //Transform the flange to align with the connector
            RotateElementInPosition(start.Origin, flangeCons.Primary, start, end, flange);
            
            //Move flange to location
            ElementTransformUtils.MoveElement(doc, flange.Id, start.Origin - flangeCons.Primary.Origin);

            //Retrieve host element systemTypeId
            var pipingSystemTypeParameter = element.get_Parameter(BuiltInParameter.RBS_PIPING_SYSTEM_TYPE_PARAM);
            var pipingSystemTypeId = pipingSystemTypeParameter.AsElementId();
            
            //Case: Connected to Pipe
            if (modCon1 != null)
            {
                Connector modCon2 =
                (from Connector c in ((Pipe) modCon1.Owner).ConnectorManager.Connectors //End of the host/dummy pipe
                    where c.Id != modCon1.Id && (int) c.ConnectorType == 1
                    select c).FirstOrDefault();

                //Get the typeId of most used pipeType
                var filter = fi.ParameterValueFilter("Stålrør, sømløse", BuiltInParameter.ALL_MODEL_TYPE_NAME);
                FilteredElementCollector col = new FilteredElementCollector(doc);
                var pipeType = col.OfClass(typeof(PipeType)).WherePasses(filter).FirstElement();

                //Create new pipe
                Pipe newPipe = Pipe.Create(doc, pipingSystemTypeId, pipeType.Id, element.LevelId, flangeCons.Secondary.Origin, modCon2.Origin);

                //Set pipe diameter
                Parameter pipeDia = newPipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                pipeDia.Set(diaValue);

                //Delete the original pipe
                doc.Delete(modCon1.Owner.Id);

                //Connect the new flange to element
                //start.ConnectTo(flangeCons.Primary);
            }
            //Case: Not connected to anything
            else
            {
                //start.ConnectTo(flangeCons.Primary);
            }

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

                if (dir.X.Round3() == 0 && dir.Y.Round3() == 0 && dir.Z.Round3() != 0)
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
    }
}
