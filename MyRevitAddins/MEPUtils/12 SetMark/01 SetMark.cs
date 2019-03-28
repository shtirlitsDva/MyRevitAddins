using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MEPUtils._00_SharedStaging;
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