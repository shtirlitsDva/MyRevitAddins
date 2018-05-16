using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.CreateInstrumentation
{
    static class ListsAndDicts
    {
        public static List<string> Directions()
        {
            return new List<string> { "Top", "Bottom", "Front", "Back", "Left", "Right" };
        }

        public static Dictionary<string, string> PipeTypeByOlet()
        {
            return new Dictionary<string, string>()
            {
                {"Weldolet", "Stålrør, sømløse weldolet" },
                {"Sockolet", "Stålrør, sømløse sockolet" },
                {"Termolomme", "Stålrør, sømløse termolomme" },
                {"Intet olet", "Stålrør, sømløse" }
            };
        }
    }
}
