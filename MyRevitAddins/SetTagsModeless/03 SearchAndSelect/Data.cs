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
using System.Configuration;

namespace MEPUtils.ModelessForms.SearchAndSelect
{
    public class ElementImpression
    {
        public int ElementId { get; private set; }
        public string FamilyAndTypeName { get; private set; }
        public string CategoryName { get; private set; }
        public int CategoryNumber { get; private set; }
        public string SystemAbbreviation { get; private set; }
        public ElementImpression(Element e)
        {
            ElementId = e.Id.IntegerValue;
            FamilyAndTypeName = e.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM).AsValueString();
            CategoryName = e.Category.Name;
            CategoryNumber = e.Category.Id.IntegerValue;
            SystemAbbreviation = e.get_Parameter(BuiltInParameter.RBS_DUCT_PIPE_SYSTEM_ABBREVIATION_PARAM).AsString();
        }
    }
    public class ParameterImpression
    {
        public int ElementId { get; private set; }
        public bool IsShared { get; private set; }
        private Guid guid;
        public Guid Guid
        {
            get
            {
                if (IsShared) return guid;
                else throw new Exception("Access to GUID on a BuiltInParameter detected! Fix code!");
            }
            private set { guid = value; }
        }
        public string Name { get; private set; }
        public ParameterImpression(Parameter p)
        {
            ElementId = p.Id.IntegerValue;
            IsShared = p.IsShared;
            if (p.IsShared) Guid = p.GUID;
            Name = p.Definition.Name;
        }
    }
    [Serializable]
    public class Grouping
    {
        public List<ParameterImpression> ParameterList { get; set; }
        public Grouping() { }
        public Grouping(List<ParameterImpression> parameterList)
        {
            ParameterList = parameterList;
        }
    }

    public sealed class GroupingSettings : ApplicationSettingsBase
    {
        [UserScopedSetting]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValue("")]
        public Grouping Grouping
        {
            get { return (Grouping)this[nameof(Grouping)]; }
            set { this[nameof(Grouping)] = value; }
        }
    }
}
