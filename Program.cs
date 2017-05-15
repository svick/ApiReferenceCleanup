using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.IO.SearchOption;

namespace ApiReferenceCleanup
{
    class Program
    {
        static void Main()
        {
            string path = @"E:\Users\Svick\git\core-docs\";

            var files = Directory.EnumerateFiles(path, "*.md", AllDirectories);

            foreach (var file in files)
            {
                bool changed = false;

                var inputLines = File.ReadAllLines(file);

                var outputLines = new List<string>();

                foreach (var line in inputLines)
                {
                    char nbsp = '\u00A0';

                    string newLine = line;

                    if (line.Contains(nbsp))
                    {
                        newLine = line.Replace(nbsp, ' ');

                        changed = true;
                    }

                    outputLines.Add(newLine);
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