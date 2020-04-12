using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using System.Globalization;
//using MoreLinq;
using System.Text;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.ApplicationServices;
using Shared;
using fi = Shared.Filter;
using op = Shared.Output;
using tr = Shared.Transformation;
using mp = Shared.MepUtils;
using NLog;

namespace MEPUtils.PED
{
    public class InitPED
    {
        public Result PopulateParameters(ExternalCommandData commandData, Logger log)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            HashSet<Element> elements = fi.GetElements<Pipe, BuiltInCategory>(doc, BuiltInCategory.INVALID).Cast<Element>().ToHashSet();
            SetWallThicknessPipes(elements, log);
            return Result.Succeeded;
        }

        public static void SetWallThicknessPipes(HashSet<Element> elements, Logger log)
        {
            //bool ctrl = false;
            //bool shft = false;
            //if ((int)Keyboard.Modifiers == 2) ctrl = true;
            //if ((int)Keyboard.Modifiers == 4) shft = true;

            foreach (Element element in elements)
            {
                //if ctrl is pressed, overwrite, else append
                //See if the parameter already has value and skip element if it has
                //if (!ctrl) if (element.get_Parameter(wallThkDef.Guid).HasValue) continue;

                //Retrieve the correct wallthickness from dictionary and set it on the element
                Parameter wallThkParameter = element.LookupParameter("PED_PIPE_WTHK");
                if (wallThkParameter == null) throw new Exception("Parameter missing! Add PED_PIPE_WTHK parameter to project! Datatype TEXT.");

                int id = element.Id.IntegerValue;

                //Retrieve parameter for outside diameter
                Parameter oDiaPar = element.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);

                //Retrieve parameter for inside diameter
                Parameter iDiaPar = element.get_Parameter(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM);

                //Calculate the wall thickness
                double oDia = oDiaPar.AsDouble();
                double iDia = iDiaPar.AsDouble();
                double wallThk = ((oDia - iDia) / 2).FtToMm().Round(1);
                log.Info($"Pipe element {element.Id.IntegerValue}: oDia: {oDia.FtToMm()}, iDia: {iDia.FtToMm()}, Wthk: {string.Format("{0:N1}", wallThk)}");

                wallThkParameter.Set(string.Format("{0:N1}", wallThk));
            }
        }

        public void processOlets(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";

            //Collect and filter Olets
            ElementParameterFilter epf = fi.ParameterValueGenericFilter(doc, "Olet", new Guid("e0baa750-22ba-4e60-9466-803137a0cba8"));
            FilteredElementCollector col = new FilteredElementCollector(doc);
            var olets = col.OfCategory(BuiltInCategory.OST_PipeFitting).OfClass(typeof(FamilyInstance)).WherePasses(epf).ToHashSet();

            foreach (Element olet in olets)
            {
                Cons cons = mp.GetConnectors(olet);
                Connector prim = cons.Primary;
                //Test if Connector is connected
                if (prim.IsConnected)
                {
                    ConnectorSet refConSet = prim.AllRefs;
                    var refCons = mp.GetAllConnectorsFromConnectorSet(refConSet);
                    Connector refCon = refCons.Where(x => isPipe(x.Owner)).SingleOrDefault();
                    if (refCon == null) refCon = refCons.Where(x => x.Owner.IsType<FamilyInstance>()).FirstOrDefault();
                    if (refCon == null) throw new Exception($"Element {olet.Id.IntegerValue} refCon Owner cannot find a Pipe of FamilyInstance!");

                    double dia = 0;

                    switch (refCon.Owner)
                    {
                        case Pipe pipe:
                            dia = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble().FtToMm().Round(1);
                            break;
                        case FamilyInstance fis:
                            Element element = (Element)fis;
                            Cons consFis = mp.GetConnectors(element);
                            dia = (consFis.Primary.Radius * 2).FtToMm().Round(1);
                            break;
                        default:
                            break;
                    }

                    //Pipe pipe = (Pipe)refCon.Owner;

                    Parameter weldsToPar = olet.get_Parameter(new Guid("c3401bb0-2e6c-4831-9917-73d6784a4a6f"));
                    weldsToPar.Set("ø" + dia.ToString(nfi));
                }
            }

            bool isPipe(Element elem)
            {
                switch (elem)
                {
                    case Pipe pipe:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}