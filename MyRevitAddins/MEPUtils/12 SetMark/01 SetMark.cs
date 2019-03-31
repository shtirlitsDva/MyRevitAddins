using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MoreLinq;
using System;
using System.Linq;
namespace MEPUtils.SetMark
{
    public static class SetMark
    {
        public static Result SetMarkExecute(ExternalCommandData commandData)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            Selection selection = uidoc.Selection;
            var selIds = selection.GetElementIds();
            if (selIds.Count == 0) throw new Exception("Empty selection: must select element(s) to set values first!");

            Element elToSet = selIds.Select(x => doc.GetElement(x)).FirstOrDefault();

            if (elToSet == null) throw new Exception("Failed to get an element! doc.GetElement returned null!");

            Parameter par = elToSet.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

            string existingValue = par.AsString();
            if (existingValue == null) existingValue = "";

            //Type in the value to set
            InputBoxBasic ds = new InputBoxBasic();
            ds.ValueToSet = existingValue;
            ds.ShowDialog();

            using (Transaction t1 = new Transaction(doc))
            {
                t1.Start("Set Mark value!");
                par.Set(ds.ValueToSet);
                t1.Commit();
            }

            return Result.Succeeded;
        }
    }
}