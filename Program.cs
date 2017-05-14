using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using static System.IO.SearchOption;
using static System.StringComparison;

namespace ApiReferenceCleanup
{
    class Program
    {
        static void Main()
        {
            string path = @"E:\Users\Svick\git\core-docs\docs\framework\configure-apps\file-schema\wcf\";

            var files = Directory.EnumerateFiles(path, "*.md", AllDirectories);

            foreach (var file in files)
            {
                bool changed = false;

                var inputLines = File.ReadAllLines(file);

                var outputLines = new List<string>();

                var codeLines = new List<string>();

                bool code = false;

                foreach (var line in inputLines)
                {
                    if (line.TrimStart(' ', '-').StartsWith("```"))
                    {
                        if (code)
                        {
                            ProcessCodeBlock();
                        }
                        else
                        {
                            codeLines.Add(line);
                        }

                        code = !code;

                        continue;
                    }

                    var linesCollection = code ? codeLines : outputLines;

                    linesCollection.Add(line);
                }

                if (code)
                    throw new Exception(file);

                if (changed)
                {
                    Console.WriteLine(file.Substring(path.Length));

                    File.WriteAllLines(file, outputLines);
                }

                void ProcessCodeBlock()
                {
                    string blockCode = string.Join("\n", codeLines.Skip(1));

                    // TODO: preserve spaces in attributes
                    // TODO: newlines between attributes when an element has several attributes

                    if (TryParseElement(blockCode, out var element))
                    {
                        changed = true;

                        var codeBlockStart = codeLines[0];
                        outputLines.Add(codeBlockStart.Substring(0, codeBlockStart.IndexOf("```", Ordinal)) + "```xml");
                        outputLines.Add(element.ToString());
                    }
                    else
                    {
                        outputLines.AddRange(codeLines);
                    }
                    outputLines.Add("```");

                    codeLines.Clear();
                }
            }
        }

        static bool TryParseElement(string text, out XElement result)
        {
            try
            {
                result = XElement.Parse(text);
                return true;
            }
            catch (XmlException)
            {
                result = null;
                return false;
            }
        }
    }
}