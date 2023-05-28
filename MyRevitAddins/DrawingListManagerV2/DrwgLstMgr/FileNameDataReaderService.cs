using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    public static class FileNameDataReaderService
    {
        public static Dictionary<PropertiesEnum, string> ReadData(
            string fileName, DrawingNamingFormat namingFormat)
        {
            Dictionary<PropertiesEnum, string> dict =
                new Dictionary<PropertiesEnum, string>();
            Match match = namingFormat.Regex.Match(fileName);
            foreach (Group gr in match.Groups)
            {
                if (gr.Name.IsNotNoE())
                {
                    Field field = new Field.Fields()
                        .GetAllFields()
                        .Where(x => x.RegexName == gr.Name)
                        .FirstOrDefault();
                    if (field == default) continue;

                    dict.Add(field.PropertyName, gr.Value);
                }
            }
            return dict;
        }
    }
}
