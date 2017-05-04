using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Data;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Shared;
using fi = Shared.Filter;
using ut = Shared.Util;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MyMepUtils;


namespace MEPUtils
{
    public class MEPUtilsClass
    {
        public static Dictionary<int, Func<Document, Result>> CreateMethodDict()
        {
            var dictionary = new Dictionary<int, Func<Document, Result>>
            {
                {1, InsulationHandler.CreateInsulationForPipes },
                {2, InsulationHandler.DeleteAllPipeInsulation }
            };
            return dictionary;
        }
        
        public static Result FormCaller(ExternalCommandData cData)
        {
            Document doc = cData.Application.ActiveUIDocument.Document;

            MEPUtilsChooser mepuc = new MEPUtilsChooser(cData);
            mepuc.ShowDialog();
            //mepuc.Close();

            var methodDict = CreateMethodDict();
            Result result = methodDict[mepuc.MethodToExecute].Invoke(doc);

            return result;
        }
    }
}
