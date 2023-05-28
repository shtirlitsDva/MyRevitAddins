using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Pdf;

namespace MEPUtils.DrawingListManagerV2
{
    internal static class MetadataReaderService
    {
        public static Dictionary<PropertiesEnum, string> ReadData(string fileNameWithPath)
        {
            Dictionary<PropertiesEnum, string> dict =
                new Dictionary<PropertiesEnum, string>();

            PdfDocument document = null;
            bool documentOpened = false;
            try
            {
                if (0 != PdfReader.TestPdfFile(fileNameWithPath))
                {
                    document = PdfReader.Open(fileNameWithPath);
                    documentOpened = true;
                }
                else documentOpened = false;
            }
            catch (Exception) { documentOpened = false; }

            if (document == null) return dict;

            if (documentOpened)
            {
                var props = document.Info.Elements;
                var fields = new Field.Fields().GetAllFields();

                foreach (var field in fields.Where(x => props.ContainsKey("/" + x.MetadataName)))
                {
                    string s = props["/"+field.MetadataName].ToString();

                }

                if (fields.Any(x => props.ContainsKey("/" + x.MetadataName)))
                {
                    foreach (Field field in fields)
                    {
                        if (props.ContainsKey("/" + field.MetadataName))
                        {
                            string s = props["/" + field.MetadataName].ToString();
                            if (s.IsNoE()) continue;
                            //Substring removes leading and closing parantheses
                            dict.Add(field.PropertyName, s.Substring(1, s.Length - 2));
                        }
                    }
                }
                document.Close();
            }
            else
            {
                return dict;
            }
            return dict;
        }
    }
}
