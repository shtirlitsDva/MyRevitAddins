using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Collections.Generic;
using static Shared.Filter;

namespace MEPUtils.NumberStuff
{
    public class NumberStuff
    {
        //1. Ask what families to number
        //  a. Use XML type of window from insulation
        //2. Present a windows:
        //  a. Shows choosen families 1. column
        //  b. Column 2: TAG 1 -> fixed text
        //  c. Column 3: TAG 2 -> Start number
        //  d. Column 4: How many digits number to be formatted (default: 2)

        public Result NumberStuffMethod(ExternalCommandData cData)
        {
            bool ctrl = false;
            if ((int)Keyboard.Modifiers == 2) ctrl = true;

            DataTable settings;

            if (!ctrl)
            {
                try
                {
                    settings = GetNumberStuffSettings(cData.Application.ActiveUIDocument.Document);
                }
                catch (Exception)
                {
                    ExecuteNumberStuffSettings(cData);
                }
            }
            else ExecuteNumberStuffSettings(cData);

            settings = GetNumberStuffSettings(cData.Application.ActiveUIDocument.Document);

            NumberStuffForm nsf = new NumberStuffForm(cData.Application.ActiveUIDocument.Document, settings);
            nsf.ShowDialog();

            //If Number button was pushed -- execute numbering
            if (nsf.Result == Result.Succeeded)
            {
                var rowsToNumber = nsf.Settings.AsEnumerable()
                    .Where(row => bool.Parse(row.Field<string>("Number")) == true);

                var groupByPrefix = rowsToNumber.GroupBy(row => row["Prefix"]);

                foreach (var group in groupByPrefix)
                {
                    int startNumber = 1;

                    HashSet<Element> elements = new HashSet<Element>();

                    foreach (var item in group)
                    {
                        elements.Union(GetElements<string, BuiltInParameter>())
                    }
                }
            }

            return Result.Succeeded;
        }

        public Result ExecuteNumberStuffSettings(ExternalCommandData cData)
        {
            NumberStuffFamilyChooser isw = new NumberStuffFamilyChooser(cData);
            isw.ShowDialog();
            isw.Close();
            using (Stream stream = new FileStream(isw.PathToSettingsXml, FileMode.Create, FileAccess.Write))
            {
                isw.Settings.WriteXml(stream);
            }

            return Result.Succeeded;
        }

        private static DataTable GetNumberStuffSettings(Document doc)
        {
            //Manage Insulation creation settings
            //Test if settings file exist
            string pn = doc.ProjectInformation.Name;
            string pathToSettingsXml =
                Environment.ExpandEnvironmentVariables(
                    $"%AppData%\\MyRevitAddins\\MEPUtils\\Settings.{pn}.NumberStuff.xml"); //Magic text?
            bool settingsExist = File.Exists(pathToSettingsXml);

            //Initialize an empty datatable
            DataTable settings = new DataTable("NumberStuff");

            if (settingsExist) //Read file if exists
            {
                using (Stream stream = new FileStream(pathToSettingsXml, FileMode.Open, FileAccess.Read))
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(stream);
                    settings = ds.Tables[0];
                }
            }
            else
                throw new Exception(
                    "Number Stuff numbering settings file does not exist! Run configuration routine first!");
            return settings;
        }
    }
}
