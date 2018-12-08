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
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MepUtils;

namespace MEPUtils.PAHangers
{
    public class CalculateHeight
    {
        public static Result Calculate(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            try
            {
                using (TransactionGroup txGp = new TransactionGroup(doc))
                {
                    txGp.Start("Calculate height of hangers");

                    FilteredElementCollector lvlCol = new FilteredElementCollector(doc);
                    List<Element> levels = lvlCol.OfClass(typeof(Level)).ToList();
                    List<string> lvlNames = levels.Select(x => x.Name).ToList();

                    //The idea is to select a different lvl for each differen reflvl
                    //To start with, we do calculations for all hangers
                    //As all our projects are with two levels only

                    BaseFormTableLayoutPanel_Basic lvlSelector = new BaseFormTableLayoutPanel_Basic(lvlNames);
                    lvlSelector.ShowDialog();

                    string lvlName = lvlSelector.strTR;

                    var topLvl = fi.GetElements<Level, BuiltInParameter>(doc, BuiltInParameter.DATUM_TEXT, lvlName).First();
                    double topLvlElevation = topLvl.Elevation;

                    //Collect elements
                    var springs = fi.GetElements<FamilyInstance, BuiltInParameter>(doc, BuiltInParameter.ELEM_FAMILY_PARAM, "Spring Hanger - Simple");
                    var rigids = fi.GetElements<FamilyInstance, BuiltInParameter>(doc, BuiltInParameter.ELEM_FAMILY_PARAM, "Rigid Hanger - Simple");
                    HashSet<Element> allHangers = new HashSet<Element>();
                    allHangers.UnionWith(springs.Cast<Element>().ToHashSet());
                    allHangers.UnionWith(rigids.Cast<Element>().ToHashSet());

                    using (Transaction trans1 = new Transaction(doc))
                    {
                        trans1.Start("Calculate height");

                        foreach (Element hanger in allHangers)
                        {
                            ElementId refLvlId = hanger.LevelId;
                            Level refLvl = (Level)doc.GetElement(refLvlId);
                            double refLvlElevation = refLvl.Elevation;
                            double lvlHeight = topLvlElevation - refLvlElevation;
                            hanger.LookupParameter("LevelHeight").Set(lvlHeight);

                            Parameter offsetPar = hanger.get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM);
                            double offsetFromLvl = offsetPar.AsDouble();
                            hanger.LookupParameter("PipeOffsetFromLevel").Set(offsetFromLvl);
                        }
                        trans1.Commit();
                    }
                    txGp.Assimilate();
                }
                return Result.Succeeded;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                //return Result.Failed;
            }
        }

    }
}
