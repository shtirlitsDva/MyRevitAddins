using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI.Selection;

namespace Data
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    [Serializable]
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
        public ParameterImpression() { }
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

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class Grouping
    {
        private List<ParameterImpression> parameterList;
        public List<ParameterImpression> ParameterList
        {
            get { return parameterList != null ? parameterList : new List<ParameterImpression>(); }
            set { parameterList = value; }
        }
        public Grouping() { }
        //public Grouping(List<ParameterImpression> parameterList)
        //{
        //    ParameterList = parameterList;
        //}
        //ParameterList = new List<ParameterImpression>();
    }
}
