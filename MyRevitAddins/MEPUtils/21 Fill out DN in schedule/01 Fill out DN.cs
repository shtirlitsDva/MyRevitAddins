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
using Autodesk.Revit.Attributes;

namespace MEPUtils.FillOutDn
{
    [Transaction(TransactionMode.Manual)]
    public class FillOutDn : IExternalCommand
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
                    tx.Start("Fill out DN");

                    var allPas = fi.GetElements<FamilyInstance, BuiltInCategory>(doc, BuiltInCategory.OST_PipeAccessory);
                    foreach (Element el in allPas)
                    {
                        Parameter dnPar = el.LookupParameter("DRI.Management.Schedule DN");
                        if (dnPar == null) continue;

                        Parameter sizePar = el.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE);
                        if (sizePar == null) continue;
                        string contents = sizePar.AsString();
                        if (contents.Contains("-"))
                        {
                            var result = contents.Split('-');
                            dnPar.Set(result[0]);
                        }
                        else dnPar.Set(contents);
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
    }
}
