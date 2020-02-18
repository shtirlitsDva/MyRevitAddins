using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace pyRevitExtensions
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ForceDivideSystem : IExternalCommand
    {
        //public ExecParams execParams;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            List<Element> sel = new List<Element>();
            foreach (ElementId eid in uidoc.Selection.GetElementIds()) sel.Add(doc.GetElement(eid));

            foreach (Element el in sel)
            {
                Pipe pipe = (Pipe)el;
                ConnectorSet cset = pipe.MEPSystem.ConnectorManager.Connectors;
                Connector con = null;
                foreach (Connector c in cset)
                {
                    if (c.ConnectorType.ToString() == "End") con = c;
                }

                if (con == null) return Result.Failed;

                try
                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        tx.Start("Force divide system!");
                        con.MEPSystem.DivideSystem(doc);
                        tx.Commit();
                    }
                }
                catch (Exception)
                {
                    return Result.Failed;
                }

                return Result.Succeeded;
            }

            return Result.Failed;

            //View curView = uidoc.ActiveView;

            //if (curView is View3D)
            //{
            //    using (Transaction tx = new Transaction(doc))
            //    {
            //        tx.Start("Switch off edges!");
            //        View3D v3d = (View3D)curView;
            //        v3d.DisplayStyle = DisplayStyle.Shading;
            //        tx.Commit();
            //        return Result.Succeeded;
            //    }
            //}
            //else return Result.Failed;
        }
    }
}
