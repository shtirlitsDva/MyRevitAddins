using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.ModelessForms.SearchAndSelect
{
    class SelectionInformationContainer
    {
        internal List<string> CategoriesToSearch { get; set; }
        public HashSet<ElementImpression> ElementsInSelection { get; set; }
        public event EventHandler SnSOperationComplete;
        public void RaiseSnSOperationComplete()
            => SnSOperationComplete.Raise(this, new MyEventArgs("Operation complete!"));

    }

    public delegate void EventHandler(object source, MyEventArgs e);
    public class MyEventArgs : EventArgs
    {
        private string EventInfo;
        public MyEventArgs(string Text) => EventInfo = Text;
        public string GetInfo() => EventInfo;
    }
    public static class EventHelper
    {
        public static void Raise(this EventHandler eventHandler, object sender, MyEventArgs args)
        {
            if (eventHandler == null) return;
            eventHandler(sender, args);
        }
    }
}
