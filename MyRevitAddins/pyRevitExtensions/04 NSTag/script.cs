using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using pyRevitLabs.NLog;


namespace pyRevitExtensions.NSTag
{
    internal class NSTag : IExternalCommand
    {
        private Logger log = LogManager.GetCurrentClassLogger();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            HashSet<Element> element = new HashSet<Element>();

            var bics = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Walls, 
                BuiltInCategory.OST_Columns, 
                BuiltInCategory.OST_StructuralColumns
            };
                 

            return Result.Succeeded;
        }
    }
}
