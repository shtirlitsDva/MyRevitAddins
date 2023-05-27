using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MEPUtils.DrawingListManagerV2
{
    internal class FileService
    {
        internal IEnumerable<DrawingInfo> GetDrawingInfosFromDirectory(string pathToFolder, DrawingInfoTypeEnum drawingType)
        {
            if (Directory.Exists(pathToFolder))
                throw new System.Exception(
                    $"Drawing folder type {drawingType} does not exist!");

            var fileList = Directory.EnumerateFiles(
                    pathToFolder, "*.pdf", SearchOption.TopDirectoryOnly);
            if (fileList == null || fileList.Count() < 1) yield return null;

            foreach (var file in fileList)
            {
                DrawingInfo drawingInfo = new DrawingInfo(file, drawingType);
                yield return drawingInfo;
            }
        }
    }
}
