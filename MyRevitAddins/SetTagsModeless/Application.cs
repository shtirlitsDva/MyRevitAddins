using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

namespace MEPUtils.ModelessForms
{
    /// <summary>
    /// Implements the Revit add-in interface IExternalApplication
    /// </summary>
    public class Application : IExternalApplication
    {
        public const string myRibbonPanelToolTip = "My Own Ribbon Panel";

        //Method to get the button image
        BitmapImage NewBitmapImage(Assembly a, string imageName)
        {
            Stream s = a.GetManifestResourceStream(imageName);

            BitmapImage img = new BitmapImage();

            img.BeginInit();
            img.StreamSource = s;
            img.EndInit();

            return img;
        }

        // get the absolute path of this assembly
        static string ExecutingAssemblyPath = Assembly.GetExecutingAssembly().Location;
        // get ref to assembly
        Assembly exe = Assembly.GetExecutingAssembly();

        // class instance
        internal static Application thisApp = null;
        // ModelessTagsForm instance
        private SetTagsInterface m_TagsForm;
        // ModelessMepUtils instance
        private MEPUtilsChooser m_MepUtilsForm;
        // ModelessSearchAndSelect instance
        private SearchAndSelect.SnS m_SearchAndSelect;
        //Modeless payload
        public IAsyncCommand asyncCommand;

        /// <summary>
        /// Implements the OnShutdown event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_TagsForm != null && m_TagsForm.Visible) m_TagsForm.Close();
            return Result.Succeeded;
        }

        /// <summary>
        /// Implements the OnStartup event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {
            AddMenu(application);
            m_TagsForm = null;   // no dialog needed yet; the command will bring it
            thisApp = this;  // static access to this application instance

            return Result.Succeeded;
        }

        private void AddMenu(UIControlledApplication application)
        {
            RibbonPanel rvtRibbonPanel = application.CreateRibbonPanel("Modeless");

            //MEPUtils.ModelessForms.SetTags
            PushButtonData data = new PushButtonData("SetTGS", "TGS", ExecutingAssemblyPath, "MEPUtils.ModelessForms.SetTags");
            data.ToolTip = "Modeless tag setter";
            data.Image = NewBitmapImage(exe, "MEPUtils.ModelessForms.Resources.ImgSetTags16.png");
            data.LargeImage = NewBitmapImage(exe, "MEPUtils.ModelessForms.Resources.ImgSetTags32.png");
            PushButton SetTags = rvtRibbonPanel.AddItem(data) as PushButton;

            //MEPUtils.ModelessForms.MepUtils
            data = new PushButtonData("MepUtils", "MEPU", ExecutingAssemblyPath, "MEPUtils.ModelessForms.MepUtils");
            data.ToolTip = "Modeless MEP utilities";
            data.Image = NewBitmapImage(exe, "MEPUtils.ModelessForms.Resources.ImgMEPUtils16.png");
            data.LargeImage = NewBitmapImage(exe, "MEPUtils.ModelessForms.Resources.ImgMEPUtils32.png");
            PushButton MepUtils = rvtRibbonPanel.AddItem(data) as PushButton;

            //MEPUtils.ModelessForms.SearchAndSelect
            data = new PushButtonData("Search and Select", "SnS", ExecutingAssemblyPath, "MEPUtils.ModelessForms.SnSLauncher");
            data.ToolTip = "Modeless Search and Select";
            data.Image = NewBitmapImage(exe, "MEPUtils.ModelessForms.Resources.ImgSnS16.png");
            data.LargeImage = NewBitmapImage(exe, "MEPUtils.ModelessForms.Resources.ImgSnS32.png");
            PushButton SnSButoon = rvtRibbonPanel.AddItem(data) as PushButton;
        }

        /// <summary>
        ///   This method creates and shows a modeless dialog, unless it already exists.
        /// </summary>
        /// <remarks>
        ///   The external command invokes this on the end-user's request
        /// </remarks>
        /// 
        public void ShowTagsForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            if (m_TagsForm == null || m_TagsForm.IsDisposed)
            {
                // A new handler to handle request posting by the dialog
                ExternalEventHandler handler = new ExternalEventHandler(thisApp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_TagsForm = new SetTagsInterface(exEvent, handler, thisApp);
                m_TagsForm.Show();
            }
        }

        /// <summary>
        ///   This method creates and shows a modeless dialog, unless it already exists.
        /// </summary>
        /// <remarks>
        ///   The external command invokes this on the end-user's request
        /// </remarks>
        /// 
        public void ShowMepUtilsForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            if (m_MepUtilsForm == null || m_MepUtilsForm.IsDisposed)
            {
                // A new handler to handle request posting by the dialog
                ExternalEventHandler handler = new ExternalEventHandler(thisApp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_MepUtilsForm = new MEPUtilsChooser(exEvent, handler, thisApp);
                m_MepUtilsForm.Show();
            }
        }

        internal void ShowSnSForm(UIApplication application)
        {
            // If we do not have a dialog yet, create and show it
            if (m_SearchAndSelect == null || m_SearchAndSelect.IsDisposed)
            {
                // A new handler to handle request posting by the dialog
                ExternalEventHandler handler = new ExternalEventHandler(thisApp);

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_SearchAndSelect = new SearchAndSelect.SnS(exEvent, handler, thisApp);
                m_SearchAndSelect.Show();
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class SetTags : IExternalCommand
    {

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                MEPUtils.ModelessForms.Application.thisApp.ShowTagsForm(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class MepUtils : IExternalCommand
    {

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                MEPUtils.ModelessForms.Application.thisApp.ShowMepUtilsForm(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class SnSLauncher : IExternalCommand
    {

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            try
            {
                MEPUtils.ModelessForms.Application.thisApp.ShowSnSForm(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
