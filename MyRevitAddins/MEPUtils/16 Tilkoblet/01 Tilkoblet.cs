using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils.SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Input;
using dbg = Shared.Dbg;
using fi = Shared.Filter;
using lad = MEPUtils.CreateInstrumentation.ListsAndDicts;
using mp = Shared.MepUtils;
using tr = Shared.Transformation;

namespace MEPUtils.Tilkoblet
{
    public class Tilkoblet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            try
            {
                using (Transaction tx = new Transaction(doc))
                {
                    var allAccessories = fi.GetElements<Element, BuiltInCategory>(doc, BuiltInCategory.OST_PipeAccessory);

                    tx.Start("Populate tilkoblet");
                    foreach (var el in allAccessories)
                    {
                        Cons cons = new Cons(el);

                        Connector firstSideCon = cons.Primary; Connector secondSideCon = cons.Secondary;

                        Connector refFirstCon = null; Connector refSecondCon = null;

                        if (firstSideCon.IsConnected)
                        {
                            var refFirstCons = MepUtils.GetAllConnectorsFromConnectorSet(firstSideCon.AllRefs);
                            refFirstCon = refFirstCons.FirstOrDefault();
                        }
                        else refFirstCon = DetectUnconnectedConnector(doc, firstSideCon);

                        if (secondSideCon.IsConnected)
                        {
                            var refSecondCons = MepUtils.GetAllConnectorsFromConnectorSet(secondSideCon.AllRefs);
                            refSecondCon = refSecondCons.FirstOrDefault();
                        }
                        else refSecondCon = DetectUnconnectedConnector(doc, secondSideCon);

                        Element firstSideOwner = null; Element secondSideOwner = null;

                        if (refFirstCon != null)
                        {
                            firstSideOwner = refFirstCon.Owner;
                        }

                    }

                    tx.Commit();
                    return Result.Succeeded; 
                }
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { return Result.Cancelled; }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private Connector DetectUnconnectedConnector(Document doc, Connector knownCon)
        {
            var allCons = MepUtils.GetALLConnectorsInDocument(doc);
            return allCons.Where(c => c.Equalz(knownCon, 0.00328) && c.Owner.Id.IntegerValue != knownCon.Owner.Id.IntegerValue).FirstOrDefault();
        }
    }
}
