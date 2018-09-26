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
                    txGp.Start("Create Instrumentation!");

                    Element elToMove;

                    using (Transaction trans1 = new Transaction(doc))
                    {
                        trans1.Start("SelectElementToMove");
                        elToMove = SelectElement(doc, uidoc, "Select element TO MOVE!");
                        trans1.Commit();
                    }

                    Element MoveToEl;

                    using (Transaction trans2 = new Transaction(doc))
                    {
                        trans2.Start("SelectElementToMoveTo");
                        MoveToEl = SelectElement(doc, uidoc, "Select element to MOVE TO!");
                        trans2.Commit();
                    }

                    double distanceToKeep;

                    //Select the direction to create in
                    InputBoxBasic ds = new InputBoxBasic();
                    ds.ShowDialog();
                    distanceToKeep = double.Parse(ds.DistanceToKeep).MmToFt();

                    //Business logic to move but keep desired distance
                    //Argh!!! too much typing!!!
                    //TODO: Add a collection that holds all cons to Cons
                    Cons toMoveConsCons = new Cons(elToMove);
                    HashSet<Connector> toMoveCons = new HashSet<Connector>();
                    if (toMoveConsCons.Primary != null) toMoveCons.Add(toMoveConsCons.Primary);
                    if (toMoveConsCons.Secondary != null) toMoveCons.Add(toMoveConsCons.Secondary);
                    if (toMoveConsCons.Tertiary != null) toMoveCons.Add(toMoveConsCons.Tertiary);

                    Cons moveToConsCons = new Cons(MoveToEl);
                    HashSet<Connector> moveToCons = new HashSet<Connector>();
                    if (moveToConsCons.Primary != null) moveToCons.Add(moveToConsCons.Primary);
                    if (moveToConsCons.Secondary != null) moveToCons.Add(moveToConsCons.Secondary);
                    if (moveToConsCons.Tertiary != null) moveToCons.Add(moveToConsCons.Tertiary);

                    var listToCompare = new List<(Connector toMoveCon, Connector MoveToCon, double Distance)>();

                    foreach (Connector c1 in toMoveCons) foreach (Connector c2 in moveToCons) listToCompare.Add((c1, c2, c1.Origin.DistanceTo(c2.Origin)));

                    var minDist = listToCompare.MinBy(x => x.Distance).FirstOrDefault();

                    double origDist = minDist.toMoveCon.Origin.DistanceTo(minDist.MoveToCon.Origin);

                    using (Transaction trans3 = new Transaction(doc))
                    {
                        trans3.Start("Move Element!");
                        {
                            ElementTransformUtils.MoveElement(doc, elToMove.Id,
                                (minDist.MoveToCon.Origin - minDist.toMoveCon.Origin) *
                                (1 - distanceToKeep / origDist));
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
