using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

//using pyRevitLabs.PyRevit.Runtime;

namespace pyRevitExtensions
{
    public class ShowEdgesOFF : IExternalCommand
    {
        //public ExecParams execParams;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            View curView = uidoc.ActiveView;

            if (curView is View3D)
            {
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Switch off edges!");
                    View3D v3d = (View3D)curView;
                    v3d.DisplayStyle = DisplayStyle.Shading;
                    tx.Commit();
                    return Result.Succeeded;
                }
            }
            else return Result.Failed;
        }
    }
}
