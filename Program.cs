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

            Regex codeWithoutSpace = new Regex(@"`[^ ]*(?<!`\|)`(?=\w)", RegexOptions.Compiled);

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
                                cdata = !line.Contains("]]>"); // if CDATA starts and ends on the same line, we ignore the line

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

                    if (code)
                    {
                        outputLines.Add(line);

                        continue;
                    }

                    if (!codeWithoutSpace.IsMatch(line))
                    {
                        outputLines.Add(line);

                        continue;
                    }

                    string fixedLine = line;

                    do
                    {
                        fixedLine = codeWithoutSpace.Replace(fixedLine, "$0 ");
                    } while (codeWithoutSpace.IsMatch(fixedLine));

                    outputLines.Add(fixedLine);

                    changed = true;
                }

                if (cdata)
                    throw new Exception();

                if (changed)
                {
                    Console.WriteLine(file.Substring(path.Length));

                    File.WriteAllLines(file, outputLines);
                }
            }
        }
    }
}