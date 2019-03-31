using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using MEPUtils.SharedStaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using MoreLinq;
using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace MEPUtils.SharedStaging
{
    public static class Extensions
    {
        /// <summary>
        /// Returns, for fittings only, the PartType of the element in question.
        /// </summary>
        /// <param name="e">Element to get the PartType property.</param>
        /// <returns>The PartType of the passed element.</returns>
        public static PartType MechFittingPartType(this Element e)
        {
            if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
            {
                var mf = ((FamilyInstance)e).MEPModel as MechanicalFitting;
                return mf.PartType;
            }
            else return PartType.Undefined;
        }
    }

    public interface IAsyncCommand
    {
        void Execute(Document dbDoc);
    }

    public static class AsyncCommandManager
    {
        private static List<IAsyncCommand> CommandList = new List<IAsyncCommand>();
        private static bool IsRegistered { get; set; }

        public static void PostCommand(UIApplication uiApp, IAsyncCommand cmd)
        {
            CommandList.Add(cmd);
            RegisterIdlingEvent(uiApp);
        }

        private static void RegisterIdlingEvent(UIApplication uiApp)
        {
            if (!IsRegistered)
            {
                uiApp.Idling += new EventHandler<IdlingEventArgs>(Execute);
                IsRegistered = true;
            }
        }

        private static void UnregisterIdlingEvent(UIApplication uiApp)
        {
            if (IsRegistered)
            {
                uiApp.Idling -= new EventHandler<IdlingEventArgs>(Execute);
                IsRegistered = false;
            }
        }

        private static void Execute(object sender, IdlingEventArgs eventArgs)
        {
            UIApplication uiApp = sender as UIApplication;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                if (CommandList.Count > 0)
                {
                    // make a copy of the command list so that we can loop through the copy and modify the original while still inside the loop.
                    List<IAsyncCommand> tempCommandList = CommandList.ToList();

                    using (TransactionGroup transGroup = new TransactionGroup(doc, "Asynchronous Idling Commands"))
                    {
                        transGroup.Start();

                        foreach (IAsyncCommand cmd in tempCommandList)
                        {
                            cmd.Execute(doc);
                            CommandList.Remove(cmd);
                        }
                        transGroup.Assimilate();
                    }

                    if (CommandList.Count == 0)
                    {
                        UnregisterIdlingEvent(uiApp);
                    }

                }
            }

            catch (Autodesk.Revit.Exceptions.ExternalApplicationException e)
            {
                Debug.WriteLine("Exception Encountered (Application)\n" + e.Message + "\nStack Trace: " + e.StackTrace);
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                Debug.WriteLine("Operation cancelled\n" + e.Message);
            }

            catch (Exception e)
            {
                Debug.WriteLine("Exception Encountered (General)\n" + e.Message + "\nStack Trace: " + e.StackTrace);
            }
        }
    }
}
