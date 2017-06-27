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
                        //Process the individual connectors of the original element
                        //Start with primary connector

                        //Create the flange at first at the primary connector (must be rotated AND moved in place)
                        Element flange1 = doc.Create.NewFamilyInstance(origCons.Primary.Origin, (FamilySymbol)familySymbol, StructuralType.NonStructural);

                        //Access the newly created flange's connectors
                        var flangeCons1 = mp.GetConnectors(flange1);

                        //Transform the flange to align with the connector
                        RotateElementInPosition(origCons.Primary.Origin, flangeCons1.Primary, origCons.Primary, origCons.Secondary, flange1);

                        //Set the diameter of the flange
                        double dia1value = origCons.Primary.Radius * 2;
                        Parameter dia1 = flange1.LookupParameter("Nominal Diameter 1");
                        dia1.Set(dia1value);

                        doc.Regenerate();

                        //Move flange to location
                        ElementTransformUtils.MoveElement(doc, flange1.Id, origCons.Primary.Origin - flangeCons1.Primary.Origin);

                        //Modify the connecting pipe if any
                        var allRefs1 = origCons.Primary.AllRefs;
                        Connector modCon11 = null;
                        foreach (Connector c in allRefs1) modCon11 = c;
                        if (modCon11 != null && modCon11.Owner is Pipe)
                        {
                            var conSet = mp.GetConnectorSet(modCon11.Owner);
                            Connector modCon12 = (from Connector c in ((Pipe)modCon11.Owner).ConnectorManager.Connectors //End of the host/dummy pipe
                                                  where c.Id != modCon11.Id && (int)c.ConnectorType == 1
                                                  select c).FirstOrDefault();

                            //Get the typeId of most used pipeType
                            var filter = fi.ParameterValueFilter("Stålrør, sømløse", BuiltInParameter.ALL_MODEL_TYPE_NAME);
                            FilteredElementCollector col = new FilteredElementCollector(doc);
                            var pipeType = col.OfClass(typeof(PipeType)).WherePasses(filter).ToElements().FirstOrDefault();

                            //Create new pipe
                            Pipe.Create(doc, pipeType.Id, element.LevelId, flangeCons1.Secondary, modCon12.Origin);

                            //Delete the original pipe
                            doc.Delete(modCon11.Owner.Id);

                            //Connect the new flange to element
                            origCons.Primary.ConnectTo(flangeCons1.Primary);
                        }

                        
                        #endregion

                        #region Secondary flange

                        //Process the individual connectors of the original element
                        //Continue with secondary connector
                        Element flange2 = doc.Create.NewFamilyInstance(origCons.Secondary.Origin, (FamilySymbol)familySymbol, StructuralType.NonStructural);

                        //Access the newly created flange's connectors
                        var flangeCons2 = mp.GetConnectors(flange2);

                        //Transform the flange to align with the connector
                        RotateElementInPosition(origCons.Secondary.Origin, flangeCons2.Primary, origCons.Secondary, origCons.Primary, flange2);

                        //Set the diameter of the flange
                        double dia2value = origCons.Secondary.Radius * 2;
                        Parameter dia2 = flange2.LookupParameter("Nominal Diameter 1");
                        dia2.Set(dia2value);

                        doc.Regenerate();

                        //Move flange to location
                        ElementTransformUtils.MoveElement(doc, flange2.Id, origCons.Secondary.Origin - flangeCons2.Primary.Origin);

                        //Modify the connecting pipe if any
                        var allRefs2 = origCons.Secondary.AllRefs;
                        Connector modCon21 = null;
                        foreach (Connector c in allRefs2) modCon21 = c;
                        if (modCon21 != null && modCon21.Owner is Pipe)
                        {
                            var conSet = mp.GetConnectorSet(modCon21.Owner);
                            Connector modCon22 = (from Connector c in ((Pipe)modCon21.Owner).ConnectorManager.Connectors //End of the host/dummy pipe
                                                  where c.Id != modCon21.Id && (int)c.ConnectorType == 1
                                                  select c).FirstOrDefault();

                            //Get the typeId of most used pipeType
                            var filter = fi.ParameterValueFilter("Stålrør, sømløse", BuiltInParameter.ALL_MODEL_TYPE_NAME);
                            FilteredElementCollector col = new FilteredElementCollector(doc);
                            var pipeType = col.OfClass(typeof(PipeType)).WherePasses(filter).ToElements().FirstOrDefault();

                            //Create new pipe
                            Pipe.Create(doc, pipeType.Id, element.LevelId, flangeCons2.Secondary, modCon22.Origin);

                            //Delete the original pipe
                            doc.Delete(modCon21.Owner.Id);

                            //Connect the new flange to element
                            origCons.Secondary.ConnectTo(flangeCons2.Primary);
                        }
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

        public static void RotateElementInPosition(XYZ placementPoint, Connector conOnFamilyToConnect, Connector start, Connector end, Element element)
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
                    //XYZ yaxis = dir.CrossProduct(connectorDirection);

                    double rotationAngle = dir.AngleTo(yaxis);
                    //double rotationAngle = 90;

                    if (dir.Z.Equals(1)) rotationAngle = -rotationAngle;

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
