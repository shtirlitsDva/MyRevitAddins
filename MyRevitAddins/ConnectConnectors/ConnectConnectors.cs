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
        public static void ConnectTheConnectors(ExternalCommandData commandData, bool ctrl)
        {
            var app = commandData.Application;
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;
            var selection = uiDoc.Selection.GetElementIds();

            if (selection.Count == 0) //If no elements selected, connect ALL connectors to ALL connectors
            {
                var allConnectors = mp.GetALLConnectorsInDocument(doc).Where(c => !c.IsConnected).ToList();

                //Employ reverse iteration to be able to modify the collection while iterating over it
                for (int i = allConnectors.Count - 1; i > 0; i--)
                {
                    if (allConnectors.Count < 2) break;
                    Connector c1 = allConnectors[i];
                    allConnectors.RemoveAt(i);
                    if (c1.IsConnected) continue; //Need: connectors connected in this loop are still in collection
                    Connector c2 = (from Connector c in allConnectors where ut.IsEqual(c.Origin, c1.Origin) select c).FirstOrDefault();
                    c2?.ConnectTo(c1);
                }
            }

            else if (selection.Count == 1)
            {
                var elements = new HashSet<Element>(from ElementId id in selection select doc.GetElement(id));
                var elementConnectors = mp.GetALLConnectorsFromElements(elements);
                
                foreach (Connector c in elementConnectors)
                {
                    if (c.IsConnected)
                    {
                        var set = c.AllRefs;
                        foreach (Connector c2 in set)
                        {
                            c2.DisconnectFrom(c);
                        }
                    }
                }
            }

            else //Connect the connectors of selection
            {
                var elements = new HashSet<Element>(from ElementId id in selection select doc.GetElement(id));
                var connectors = mp.GetALLConnectorsFromElements(elements).ToList();
                //Find duplicate
                bool foundIt = false;
                while (!foundIt) //I have a suspicion that the while loop is redudant.
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
}

