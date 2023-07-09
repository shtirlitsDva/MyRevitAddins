using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class DrawingAnalysisResult
    {
        public DrawingAnalysisResult(IGrouping<string, DrawingInfo> data)
        {
            DrawingNumber = new DrawingAttributeAnalysisResult(
                PropertiesEnum.Number, data);
            Title = new DrawingAttributeAnalysisResult(
                PropertiesEnum.Title, data);
            Scale = new DrawingAttributeAnalysisResult(
                PropertiesEnum.Scale, data);
            ReleaseDate = new DrawingAttributeAnalysisResult(
                PropertiesEnum.Date, data);
            RevisionLetter = new DrawingAttributeAnalysisResult(
                PropertiesEnum.Revision, data);
            RevisionDate = new DrawingAttributeAnalysisResult(
                PropertiesEnum.RevisionDate, data);
        }
        public DrawingAttributeAnalysisResult DrawingNumber { get; }
        public DrawingAttributeAnalysisResult Title { get; }
        public DrawingAttributeAnalysisResult Scale { get; }
        public DrawingAttributeAnalysisResult ReleaseDate { get; }
        public DrawingAttributeAnalysisResult RevisionLetter { get; }
        public DrawingAttributeAnalysisResult RevisionDate { get; }
        public bool IsValid() => DrawingNumber.IsValid();
    }
}
