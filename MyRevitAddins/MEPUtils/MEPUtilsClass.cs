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
        public static Result FormCaller(ExternalCommandData cData)
        {
            MEPUtilsChooser mepuc = new MEPUtilsChooser();
            mepuc.ShowDialog();
            //mepuc.Close();

            Result result = mepuc.MethodToExecute.Invoke(cData);

            return result;
        }
    }
}
