using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

namespace MEPUtils.ModelessForms.SearchAndSelect
{
    public class ElementImpression
    {
        public int ElementId { get; private set; }
        public string FamilyAndTypeName { get; private set; }
        public string CategoryName { get; private set; }
        public int CategoryNumber { get; private set; }
        public ElementImpression(Element e)
        {
            ElementId = e.Id.IntegerValue;
            FamilyAndTypeName = e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();
            CategoryName = e.Category.Name;
            CategoryNumber = e.Category.Id.IntegerValue;
        }
    }
}
