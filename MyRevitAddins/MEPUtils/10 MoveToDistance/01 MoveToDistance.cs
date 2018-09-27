using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using fi = Shared.Filter;
using ut = Shared.BuildingCoder.Util;
using tr = Shared.Transformation;
using mp = Shared.MepUtils;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using MEPUtils._00_SharedStaging;
using dbg = Shared.Dbg;

namespace MEPUtils.MoveToDistance
{
    public class MoveToDistance
    {
        public static Result Move(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            try
            {
                using (TransactionGroup txGp = new TransactionGroup(doc))
                {
                    txGp.Start("Move elements to distance!");

                    Selection selection = uidoc.Selection;
                    var selIds = selection.GetElementIds();
                    if (selIds.Count == 0) throw new Exception("Empty selection: must select element(s) to move first!");

                    HashSet<Element> elsToMove = selIds.Select(x => doc.GetElement(x)).ToHashSet();

                    var elId = uidoc.Selection.PickObject(ObjectType.Element, "Select element to move to!");
                    Element MoveToEl = doc.GetElement(elId);
                    elId.
                    double distanceToKeep;

                    //Select the direction to create in
                    InputBoxBasic ds = new InputBoxBasic();
                    ds.ShowDialog();
                    distanceToKeep = double.Parse(ds.DistanceToKeep).MmToFt();

                    //Business logic to move but keep desired distance
                    HashSet<Connector> toMoveCons = Shared.MepUtils.GetALLConnectorsFromElements(elsToMove);

                    HashSet<Connector> moveToCons = Shared.MepUtils.GetALLConnectorsFromElements(MoveToEl);

                    var listToCompare = new List<(Connector toMoveCon, Connector MoveToCon, double Distance)>();

                    foreach (Connector c1 in toMoveCons) foreach (Connector c2 in moveToCons) listToCompare.Add((c1, c2, c1.Origin.DistanceTo(c2.Origin)));

                    var minDist = listToCompare.MinBy(x => x.Distance).FirstOrDefault();

                    double origDist = minDist.toMoveCon.Origin.DistanceTo(minDist.MoveToCon.Origin);

                    using (Transaction trans3 = new Transaction(doc))
                    {
                        trans3.Start("Move Element!");
                        {
                            foreach (Element elToMove in elsToMove)
                            {
                                ElementTransformUtils.MoveElement(doc, elToMove.Id,
                                    (minDist.MoveToCon.Origin - minDist.toMoveCon.Origin) *
                                    (1 - distanceToKeep / origDist));
                            }
                        }
                        trans3.Commit();
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

        private static Element SelectElement(Document doc, UIDocument uidoc, string msg)
        {
            //Select the pipe to operate on
            return ut.SelectSingleElementOfType(uidoc, typeof(Element), msg, true);
        }
    }
}
