using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MoreLinq;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure.StructuralSections;
using static Autodesk.Revit.DB.UnitTypeId;
using Microsoft.WindowsAPICodePack.Dialogs;
using Shared;
using NLog;
using fi = Shared.Filter;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MepUtils;

namespace MEPUtils.SupportTools
{
    public class UpdateLoadsFromR2
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        public static void Update(UIApplication uiApp)
        {
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(@"X:\AutoCAD DRI - Revit\Addins\NLog\NLog.config");

            try
            {
                using (TransactionGroup txGp = new TransactionGroup(doc))
                {
                    txGp.Start("Update support loads.");

                    SupportUpdater su = new SupportUpdater();
                    su.ShowDialog();
                    su.Close();

                    //log.Debug("Test!");

                    

                    #region OldCode
                    ////Process the file
                    ////Skip is to remove the first line
                    //string allLines = string.Join(Environment.NewLine, File.ReadAllLines(fileName).Skip(1).ToArray());
                    //string[] splitByEmptyLines = allLines.Split(new string[]
                    //    { Environment.NewLine +" "+ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    ////Whole block as string
                    //for (int i = 0; i < splitByEmptyLines.Length; i++)
                    //{
                    //    string[] splitByLinebreak = splitByEmptyLines[i].Split(
                    //        new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    //    string[][] atomised = new string[splitByLinebreak.Length][];

                    //    //Block as lines
                    //    for (int j = 0; j < splitByLinebreak.Length; j++)
                    //    {
                    //        string temp = Regex.Replace(splitByLinebreak[j], @"\s\s+", "");
                    //        temp = Regex.Replace(temp, @"\s;", ";");
                    //        temp = Regex.Replace(temp, @";\s", ";");
                    //        temp = Regex.Replace(temp, "\"", "");
                    //        atomised[j] = temp.Split(';');
                    //    }

                    //    Console.WriteLine("-------------------------------------------------------------------");
                    //    Console.WriteLine("Type: " + atomised[3][2]);
                    //    Console.WriteLine("Tag: " + atomised[1][2]);
                    //    Console.WriteLine("Load Case: " + atomised[7][3]);
                    //    Console.WriteLine("Value: " + atomised[0][12] + " " + atomised[7][12]);

                    //    if (atomised[1][2].IsNullOrEmpty()) continue; //Skipe record if the tag is empty

                    //    string[] tagParts = atomised[1][2].Split('_');

                    //    FilteredElementCollector col = new FilteredElementCollector(doc);
                    //    col = col.OfCategory(BuiltInCategory.OST_PipeAccessory).OfClass(typeof(FamilyInstance));

                    //    //Tag 1
                    //    ElementParameterFilter epf = fi.ParameterValueGenericFilter(
                    //        doc, tagParts[0], new Guid("a93679f7-ca9e-4a1e-bb44-0d890a5b4ba1"));
                    //    col = col.WherePasses(epf);
                    //    //Tag 2
                    //    epf = fi.ParameterValueGenericFilter(
                    //        doc, tagParts[1], new Guid("3b2afba4-447f-422a-8280-fd394718ad4e"));
                    //    col = col.WherePasses(epf);

                    //    var ids = col.ToElementIds();
                    //    if (ids.Count < 1) throw new Exception($"No element found for tag: {atomised[1][2]}");
                    //    if (ids.Count > 1) throw new Exception($"Multiple elements found for tag: {atomised[1][2]}");

                    //    using (Transaction tx = new Transaction(doc))
                    //    {
                    //        tx.Start("Update element's load");
                    //        foreach (ElementId id in ids)
                    //        {
                    //            Element e = doc.GetElement(id);
                    //            Parameter par = e.LookupParameter("Belastning");
                    //            if (par == null) throw new Exception($"Element id {e.Id} does not have 'Belastning' parameter!");
                    //            double rawValue = Math.Abs(double.Parse(atomised[7][12]));
                    //            double convertedValue = UnitUtils.ConvertToInternalUnits(
                    //                rawValue, UnitTypeId.Kilonewtons);
                    //            par.Set(convertedValue);
                    //        }
                    //        tx.Commit();
                    //    }

                    //    //Console.WriteLine(PrettyPrintArrayOfArrays(atomised)); 

                    //}
                    #endregion
                    txGp.Assimilate();
                }
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                //return Result.Failed;
            }
        }

    }
}
