using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using MoreLinq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;

namespace PlaceSupport
{
    public partial class SupportChooser : System.Windows.Forms.Form
    {
        public SupportChooser(ExternalCommandData commandData, ref string message)
        {
            InitializeComponent();
            Document doc = commandData.Application.ActiveUIDocument.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var family = collector.OfClass(typeof (Family)).Where(e => e.Name == "Support Symbolic").Cast<Family>().FirstOrDefault();
            if (family == null) throw new Exception("No Support Symbolic family in project!");
            var famSymbolList = family.GetFamilySymbolIds();
            var query = famSymbolList.Select(t => doc.GetElement(t)).ToHashSet();
            var list = query.Select(e => e.Name).ToList();
            list.Sort();

            //StringBuilder sb = new StringBuilder();
            //foreach (var f in query)
            //{
            //    sb.AppendLine(f.Name);
            //}
            //ut.InfoMsg(sb.ToString());

        }
    }
}
