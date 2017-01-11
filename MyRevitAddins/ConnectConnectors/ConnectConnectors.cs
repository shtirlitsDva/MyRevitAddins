using System;
using System.Collections.Generic;
using System.Linq;
//using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using mp = Shared.MyMepUtils;
//using mySettings = GeneralStability.Properties.Settings;

namespace ConnectConnectors
{
    public class ConnectConnectors
    {
        public static void ConnectTheConnectors(ExternalCommandData commandData)
        {
            var app = commandData.Application;
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;
            var selection = uiDoc.Selection.GetElementIds();
            var elements = new HashSet<Element>(from ElementId id in selection select doc.GetElement(id));
            var connectors = mp.GetALLConnectors(elements);
            //Find duplicate
            bool foundIt = false;
            while (!foundIt)
            {
                if (connectors.Count < 2) throw new Exception("No eligible connectors found! Check alignment.");
                Connector c1 = connectors[0];
                connectors.RemoveAt(0);
                foreach (Connector c2 in connectors.Where(c2 => ut.IsEqual(c1.Origin, c2.Origin)))
                {
                    if (c1.IsConnectedTo(c2))
                    {
                        c1.DisconnectFrom(c2);
                        foundIt = true;
                        break;
                    }
                    c1.ConnectTo(c2);
                    foundIt = true;
                    break;
                }
            }
        }
    }
}

