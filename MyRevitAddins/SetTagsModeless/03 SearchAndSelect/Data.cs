using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Shared;

namespace MEPUtils.ModelessForms.SearchAndSelect
{
    public class ElementImpression
    {
        public int ElementId { get; private set; }
        public string FamilyName { get; private set; }
        public string TypeName { get; private set; }
        public string CategoryName { get; private set; }
        public int CategoryNumber { get; private set; }
        internal Grouping Grouping { get; private set; }
        internal List<string> Values { get; private set; }
        public ElementImpression(Element e, Grouping grouping)
        {
            ElementId = e.Id.IntegerValue;
            TypeName = e.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM).AsValueString();
            FamilyName = e.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString();
            CategoryName = e.Category.Name;
            CategoryNumber = e.Category.Id.IntegerValue;
            Grouping = grouping;

            Values = new List<string>(Grouping.ParameterList.Count);

            foreach (ParameterImpression pi in Grouping.ParameterList)
            {
                Parameter par = null;
                if (pi.IsBuiltIn) e.get_Parameter(pi.BuiltInParameter);
                else if (pi.IsShared) e.get_Parameter(pi.Guid);
                if (par != null) Values.Add(par.ToValueString2());
                else Values.Add("<+>");
            }
        }
    }
    public class ParameterImpression
    {
        public int ElementId { get; private set; }
        public int HashCode { get; private set; }
        public BuiltInParameter BuiltInParameter { get; private set; }
        public bool IsBuiltIn { get; private set; }
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
            InternalDefinition definition = (InternalDefinition)p.Definition;
            Name = definition.Name;
            HashCode = p.Id.GetHashCode();
            if (definition.BuiltInParameter == BuiltInParameter.INVALID) IsBuiltIn = false;
            else
            {
                IsBuiltIn = true;
                BuiltInParameter = definition.BuiltInParameter;
            }
        }
    }

    public class ParameterImpressionComparer : IEqualityComparer<ParameterImpression>
    {
        public bool Equals(ParameterImpression x, ParameterImpression y)
        {
            return null != x && null != y && x.ElementId == y.ElementId;
        }

        public int GetHashCode(ParameterImpression x)
        {
            return x.HashCode;
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
        public GroupingSettings() { }
        public GroupingSettings(Grouping grouping) { Grouping = grouping; }
    }

    public class ParameterTypeGroup
    {
        public string Name { get; set; }
        public BindingList<ParameterImpression> ParameterList { get; set; }
        public ParameterTypeGroup(string name, BindingList<ParameterImpression> parameterList)
        {
            Name = name; ParameterList = parameterList;
        }
    }
}
