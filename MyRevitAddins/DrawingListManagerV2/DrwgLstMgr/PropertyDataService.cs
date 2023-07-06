using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEPUtils.DrawingListManagerV2
{
    internal class PropertyDataService
    {
        private static readonly int numberOfEnumValues =
            Enum.GetValues(typeof(DrawingInfoTypeEnum)).Length;
        public string this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }
        private string[] data =
            new string[numberOfEnumValues];
        public void SetData(string value, DrawingInfoTypeEnum drawingType)
        {
            data[(int)drawingType] = value;
        }
        private string getData(DrawingInfoTypeEnum drawingType) =>
            data[(int)drawingType];
        public bool HasExcel { get => getData(DrawingInfoTypeEnum.Excel).IsNotNoE(); }
        public bool HasReleased { get => getData(DrawingInfoTypeEnum.Released).IsNotNoE(); }
        public bool HasStaging { get => getData(DrawingInfoTypeEnum.Staging).IsNotNoE(); }
        public string Excel { get => getData(DrawingInfoTypeEnum.Excel); }
        public string Released { get => getData(DrawingInfoTypeEnum.Released); }
        public string Staging { get => getData(DrawingInfoTypeEnum.Staging); }
    }
}
