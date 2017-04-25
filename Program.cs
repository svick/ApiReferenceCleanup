using System;
using System.Collections.Generic;
using System.IO;
using static System.IO.SearchOption;

namespace ApiReferenceCleanup
{
    class Program
    {
        static void Main()
        {
            string path = @"E:\Users\Svick\git\core-docs\xml";

            var files = Directory.EnumerateFiles(path, "*.xml", AllDirectories);

            foreach (var file in files)
            {
                bool changed = false;

                var inputLines = File.ReadAllLines(file);

                var outputLines = new List<string>();

                bool cdata = false;
                bool code = false;

                foreach (var line in inputLines)
                {
                    if (!code)
                    {
                        if (!cdata)
                        {
                            outputLines.Add(line);

                            if (line.Contains("<![CDATA["))
                                cdata = true;

                            continue;
                        }

                        if (line.Contains("]]>"))
                        {
                            cdata = false;

                            outputLines.Add(line);

                            continue;
                        }
                    }

                    if (line.StartsWith("```"))
                        code = !code;

                    if (code || !line.StartsWith(" "))
                    {
                        outputLines.Add(line);

                        continue;
                    }

                    var fixedLine = line.Substring(1);

                    outputLines.Add(fixedLine);

                    changed = true;
                }

                if (changed)
                    File.WriteAllLines(file, outputLines);
            }
        }
    }
}