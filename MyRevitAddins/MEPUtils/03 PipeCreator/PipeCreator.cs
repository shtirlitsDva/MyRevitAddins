using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using System.Windows.Input;
using Shared;
using fi = Shared.Filter;
using ut = Shared.BuildingCoder.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MepUtils;

namespace MEPUtils
{
    public static class PipeCreator
    {
        public static Result CreatePipeFromConnector(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;

            bool ctrl = false;
            if ((int)Keyboard.Modifiers == 2) ctrl = true;

            string pipeTypeName = MEPUtils.Properties.Settings.Default.PipeCreator_SelectedPipeTypeName;

            bool test = string.IsNullOrEmpty(pipeTypeName);

            //If the name of pipeType is null or empty for some reason -- reinitialize
            if (string.IsNullOrEmpty(pipeTypeName)) ctrl = true;

            if (ctrl)
            {
                FilteredElementCollector colPipeTypes = new FilteredElementCollector(doc);
                var pipeTypes = colPipeTypes.OfClass(typeof(PipeType)).ToElements();

                var pipeTypeNames = colPipeTypes.Select(x => x.Name).ToList();

                int count = pipeTypeNames.Count;



                var pc = new PipeTypeSelector(cData, pipeTypeNames);
                pc.ShowDialog();

                pipeTypeName = pc.pipeTypeName;
                MEPUtils.Properties.Settings.Default.PipeCreator_SelectedPipeTypeName = pipeTypeName;
            }

            try
            {
                //One element selected, creates pipe at random connector
                Selection selection = cData.Application.ActiveUIDocument.Selection;
                ElementId id = selection.GetElementIds().FirstOrDefault();
                if (id == null) throw new Exception("Getting element from selection failed!");
                Element element = doc.GetElement(id);
                if (element is Pipe) throw new Exception("This method does not work on pipes!");
                var cons = mp.GetALLConnectorsFromElements(element);
                Connector con = (from Connector c in cons where c.IsConnected == false select c).FirstOrDefault();
                if (con == null) throw new Exception("No not connected connectors in element!");

                //Create a point in space to connect the pipe
                XYZ direction = con.CoordinateSystem.BasisZ.Multiply(2);
                XYZ origin = con.Origin;
                XYZ pointInSpace = origin.Add(direction);

                //Get the typeId of most used pipeType
                var filter = fi.ParameterValueGenericFilter(doc, pipeTypeName, BuiltInParameter.ALL_MODEL_TYPE_NAME);
                FilteredElementCollector col = new FilteredElementCollector(doc);
                var pipeType = col.OfClass(typeof(PipeType)).WherePasses(filter).ToElements().FirstOrDefault();
                if (pipeType == null) throw new Exception("Collection of PipeType failed!");

                //LevelId can be null -> work around
                ElementId levelId;
                ;
                if (element.LevelId.IntegerValue == -1)
                {
                    FilteredElementCollector lcol = new FilteredElementCollector(doc);
                    var randomLvl = lcol.OfClass(typeof(Level)).ToElements().FirstOrDefault(); //Select random levelid
                    levelId = randomLvl.Id;
                }
                else levelId = element.LevelId;

                //Transaction that creates the pipe
                Transaction tx = new Transaction(doc);
                tx.Start("Create pipe!");
                //Create the pipe
                Pipe.Create(doc, pipeType.Id, levelId, con, pointInSpace);
                tx.Commit();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }



            return Result.Succeeded;
        }
    }
}
