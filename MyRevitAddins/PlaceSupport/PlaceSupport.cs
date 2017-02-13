using System;
using System.Collections.Generic;
using System.Linq;
//using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace PlaceSupport
{
    public class PlaceSupport
    {
        public static Tuple<Pipe, Element> PlaceSupports(ExternalCommandData commandData)
        {
            var app = commandData.Application;
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;
            var selection = uiDoc.Selection.GetElementIds();

            if (selection == null) throw new Exception("Select ONE pipe!");
            if (selection.Count > 1) throw new Exception("Select only ONE pipe!");

            try
            {
                //Collect pipe's connectors
                Element hostPipe = doc.GetElement((from ElementId id in selection select id).FirstOrDefault());
                var oneOfPipeCons = (from Connector c in mp.GetConnectorSet(hostPipe) where (int)c.ConnectorType == 1 select c).FirstOrDefault();

                //Get family symbol
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                ElementParameterFilter filter = fi.ParameterValueFilter("Support Symbolic: ANC", BuiltInParameter.SYMBOL_FAMILY_AND_TYPE_NAMES_PARAM);
                LogicalOrFilter classFilter = fi.FamSymbolsAndPipeTypes();
                FamilySymbol familySymbol = (FamilySymbol)collector.WherePasses(classFilter).WherePasses(filter).FirstOrDefault();

                //Get the host pipe level
                Level level = (Level) doc.GetElement(hostPipe.LevelId);
                
                //Create the support instance
                Element support = doc.Create.NewFamilyInstance(oneOfPipeCons.Origin, familySymbol, level, StructuralType.NonStructural);

                //Get the connector from the support
                FamilyInstance familyInstanceToAdd = (FamilyInstance)support;
                ConnectorSet connectorSetToAdd = new ConnectorSet();
                MEPModel mepModel = familyInstanceToAdd.MEPModel;
                connectorSetToAdd = mepModel.ConnectorManager.Connectors;
                if (connectorSetToAdd.IsEmpty)
                    throw new Exception(
                        "The support family lacks a connector. Please read the documentation for correct procedure of setting up a support element.");
                Connector connectorToConnect =
                    (from Connector c in connectorSetToAdd where true select c).FirstOrDefault();

                //Rotate into place
                tr.RotateElementInPosition(connectorToConnect, oneOfPipeCons, support);

                //Set diameter
                Parameter nominalDiameter = support.LookupParameter("Nominal Diameter");
                nominalDiameter.Set(oneOfPipeCons.Radius * 2);

                return new Tuple<Pipe, Element>((Pipe)hostPipe, support);
            }
                catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
