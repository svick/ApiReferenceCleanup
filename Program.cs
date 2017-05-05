using System;
using System.IO;
using System.Linq;
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

            foreach (var file in files)
            {
                bool changed = false;

                var doc = XDocument.Load(file);

                foreach (var element in doc.Descendants())
                {
                    if (element.PreviousNode is XText previousText && char.IsLetterOrDigit(previousText.Value.Last()))
                    {
                        previousText.Value = previousText.Value + " ";

                        changed = true;
                    }

                    if (element.NextNode is XText nextText && char.IsLetterOrDigit(nextText.Value.First()))
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

                    using (var fileStream = File.OpenWrite(file))
                    {
                        doc.Save(fileStream);
                    }
                }
            }
        }
    }
}