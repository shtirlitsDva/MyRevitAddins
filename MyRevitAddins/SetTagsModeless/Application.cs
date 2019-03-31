//
// (C) Copyright 2003-2017 by Autodesk, Inc. All rights reserved.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.

//
// AUTODESK PROVIDES THIS PROGRAM 'AS IS' AND WITH ALL ITS FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable. 

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;

using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace MEPUtils.SetTagsModeless
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
        // ModelessForm instance
        private ModelessForm m_MyForm;

        #region IExternalApplication Members
        /// <summary>
        /// Implements the OnShutdown event
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            if (m_MyForm != null && m_MyForm.Visible)
            {
                m_MyForm.Close();
            }

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
            m_MyForm = null;   // no dialog needed yet; the command will bring it
            thisApp = this;  // static access to this application instance

            return Result.Succeeded;
        }

        private void AddMenu(UIControlledApplication application)
        {
            RibbonPanel rvtRibbonPanel = application.CreateRibbonPanel("MyRevitAddins");

            //MEPUtils.SetTagsModeless
            PushButtonData data = new PushButtonData("SetTags", "TGS", ExecutingAssemblyPath, "MEPUtils.SetTagsModeless.Command");
            data.ToolTip = "Modeless tag setter";
            data.Image = NewBitmapImage(exe, "MEPUtils.SetTagsModeless.Resources.ImgSetTags16.png");
            data.LargeImage = NewBitmapImage(exe, "MEPUtils.SetTagsModeless.Resources.ImgSetTags32.png");
            PushButton MEPUtils = rvtRibbonPanel.AddItem(data) as PushButton;
        }

        /// <summary>
        ///   This method creates and shows a modeless dialog, unless it already exists.
        /// </summary>
        /// <remarks>
        ///   The external command invokes this on the end-user's request
        /// </remarks>
        /// 
        public void ShowForm(UIApplication uiapp)
        {
            // If we do not have a dialog yet, create and show it
            if (m_MyForm == null || m_MyForm.IsDisposed)
            {
                // A new handler to handle request posting by the dialog
                RequestHandler handler = new RequestHandler();

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible fore disposing them, eventually.
                m_MyForm = new ModelessForm(exEvent, handler);
                m_MyForm.Show();
            }
        }


        /// <summary>
        ///   Waking up the dialog from its waiting state.
        /// </summary>
        /// 
        public void WakeFormUp()
        {
            if (m_MyForm != null)
            {
                m_MyForm.WakeUp();
            }
        }
        #endregion
    }
}