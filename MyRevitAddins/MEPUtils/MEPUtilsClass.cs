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
        private static Dictionary<int, Func<ExternalCommandData, Result>> CreateMethodDict()
        {
            var dictionary = new Dictionary<int, Func<ExternalCommandData, Result>>
            {
                {1, InsulationHandler.CreateInsulationForPipes },
                {2, InsulationHandler.DeleteAllPipeInsulation }
            };
            return dictionary;
        }
        
        public static Result FormCaller(ExternalCommandData cData)
        {
            MEPUtilsChooser mepuc = new MEPUtilsChooser(cData);
            mepuc.ShowDialog();
            //mepuc.Close();

            var methodDict = CreateMethodDict();
            Result result = methodDict[mepuc.MethodToExecute].Invoke(cData);

            return result;
        }
    }
}
