using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using static System.IO.SearchOption;

namespace ApiReferenceCleanup
{
    class Program
    {
        static void Main()
        {
            string path = @"E:\Users\Svick\git\core-docs\xml\";

            var files = Directory.EnumerateFiles(path, "*.xml", AllDirectories);

            Regex allowedLetterStart = new Regex(@"^(?:s|ing)\b", RegexOptions.Compiled);

            foreach (var file in files)
            {
                bool changed = false;

                var doc = XDocument.Load(file);

                foreach (var element in doc.Descendants())
                {
                    if (element.PreviousNode is XText previousText && char.IsLetter(previousText.Value.Last()))
                    {
                        previousText.Value = previousText.Value + " ";

                        changed = true;
                    }

                    if (element.NextNode is XText nextText && char.IsLetter(nextText.Value.First()) && !allowedLetterStart.IsMatch(nextText.Value))
                    {
                        nextText.Value = " " + nextText.Value;

                        changed = true;
                    }

                    foreach (var attribute in element.Attributes())
                    {
                        var trimmed = attribute.Value.Trim();

                        if (trimmed != attribute.Value)
                        {
                            attribute.Value = trimmed;

                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    Console.WriteLine(file.Substring(path.Length));

                    var settings = new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Indent = true,
                        Encoding = new UTF8Encoding(false)
                    };

                    using (var fileStream = File.Open(file, FileMode.Create))
                    using (var xmlWriter = XmlWriter.Create(fileStream, settings))
                    {
                        doc.Save(xmlWriter);
                    }
                }
            }
        }
    }
}