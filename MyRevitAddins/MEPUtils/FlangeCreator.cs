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
        public static Result CreateFlangeForElements(ExternalCommandData cData, string familyAndTypeName)
        {
            try
            {
                Document doc = cData.Application.ActiveUIDocument.Document;

                //One element selected, creates pipe at random connector
                Selection selection = cData.Application.ActiveUIDocument.Selection;
                var elemIds = selection.GetElementIds();
                if (elemIds == null) throw new Exception("Getting element from selection failed!");

                //Collect the family symbol of the flange
                var collector = new FilteredElementCollector(doc);
                var symbolFilter = fi.ParameterValueFilter(familyAndTypeName,
                    BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
                var classFilter = fi.FamSymbolsAndPipeTypes();
                Element familySymbol = collector.WherePasses(classFilter).WherePasses(symbolFilter).FirstElement();

                //Process the elements
                foreach (var id in elemIds)
                {
                    Element element = doc.GetElement(id);
                    if (element is Pipe) throw new Exception("This method does not work on pipes!");
                    var cons = mp.GetALLConnectorsFromElements(element);

                    //Process the individual connectors of the original element
                    foreach (Connector conToPutFlangeOn in cons)
                    {
                        //Create the flange at first at the connector (must be rotated AND moved in place)
                        Element flange = doc.Create.NewFamilyInstance(conToPutFlangeOn.Origin, (FamilySymbol)familySymbol, StructuralType.NonStructural);
                    }

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static void RotateElementInPosition(XYZ placementPoint, Connector conOnFamilyToConnect, Connector start, Element element)
        {
            #region Geometric manipulation

            //http://thebuildingcoder.typepad.com/blog/2012/05/create-a-pipe-cap.html

            //Centre of element
            XYZ placementPoint = elementSymbol.CentrePoint.Xyz;

            //Select the OTHER connector
            MEPCurve hostPipe = start.Owner as MEPCurve;

            Connector end = (from Connector c in hostPipe.ConnectorManager.Connectors //End of the host/dummy pipe
                             where c.Id != start.Id && (int)c.ConnectorType == 1
                             select c).FirstOrDefault();

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
