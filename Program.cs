using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static System.IO.SearchOption;

namespace ApiReferenceCleanup
{
    class Program
    {
        static void Main()
        {
            string path = @"E:\Users\Svick\git\core-docs\xml\";

            var files = Directory.EnumerateFiles(path, "*.xml", AllDirectories);

            Regex tagWithoutSpace = new Regex(@"(?:<[^>]+/>|<(\w+)(?:| [^>]+)>[^<]*</\1>)(?=[a-zA-Z])(?!(?:s|ing)\b)", RegexOptions.Compiled);

            foreach (var file in files)
            {
                bool changed = false;

                var inputLines = File.ReadAllLines(file);

                var outputLines = new List<string>();

                foreach (var line in inputLines)
                {
                    if (!tagWithoutSpace.IsMatch(line))
                    {
                        outputLines.Add(line);

                        continue;
                    }

                    var changedLine = tagWithoutSpace.Replace(line, "$0 ");

                    outputLines.Add(changedLine);

                    changed = true;
                }

                if (changed)
                {
                    Console.WriteLine(file.Substring(path.Length));

                    File.WriteAllLines(file, outputLines);
                }
            }
        }
    }
}