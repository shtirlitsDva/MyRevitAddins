using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection; // for getting the assembly path
using System.Windows.Media; // for the graphics
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;


using adWin = Autodesk.Windows;

namespace RevitUICustomization
{
    public static class RevitUICustomization
    {
        public static Result test(ExternalCommandData cdata)
        {
            DockablePaneId pbId = Autodesk.Revit.UI.DockablePanes.BuiltInDockablePanes.ProjectBrowser;

            //Autodesk.Revit.UI.DockablePane pane = new DockablePane(pbId);
            var pb = cdata.Application.GetDockablePane(pbId);

            return Result.Succeeded;

        }
    }
}
