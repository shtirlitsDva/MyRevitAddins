using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrawingAnalysisResult
    {
        public DrawingAttributeAnalysisResult DrawingNumber { get; set; }
        public DrawingAttributeAnalysisResult Title { get; set; }
        public DrawingAttributeAnalysisResult Scale { get; set; }
        public DrawingAttributeAnalysisResult ReleaseDate { get; set; }
        public DrawingAttributeAnalysisResult RevisionLetter { get; set; }
        public DrawingAttributeAnalysisResult RevisionDate { get; set; }

        // Additional properties to keep track of the status
        public bool IsConsistentWithDrawingList { get; set; }
        public bool HasPendingRevision { get; set; }
        public bool HasMetadata { get; set; }
    }
}
