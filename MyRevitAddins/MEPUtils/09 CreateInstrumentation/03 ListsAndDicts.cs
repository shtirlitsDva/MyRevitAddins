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

        /// <summary>
        /// Returns a list of valid general Pipe DNs
        /// </summary>
        /// <returns></returns>
        public static List<string> SizeList()
        {
            return new List<string>()
            {
                {"15"},
                {"20"},
                {"25"},
                {"32"},
                {"40"},
                {"50"},
                {"65"},
                {"80"},
                {"100"},
                {"125"},
                {"150" },
                {"200"},
                {"250"},
                {"300"},
                {"350" },
                {"400"}
            };
        }

        /// <summary>
        /// Returns a list of valid Weldolet NDs
        /// </summary>
        public static List<string> WList()
        {
            return new List<string>()
            {
                {"15" },
                {"20" },
                {"25" }
            };
        }

        /// <summary>
        /// Returns a list of valid Sockolet NDs
        /// </summary>
        public static List<string> SList()
        {
            return new List<string>()
            {
                {"32" },
                {"40" },
                {"50" },
                {"65" },
                {"80" }
            };
        }
    }
}
