using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.IO.SearchOption;
using static System.Text.RegularExpressions.RegexOptions;

namespace ApiReferenceCleanup
{
    class Program
    {
        private static readonly Regex XmlLine = new Regex("^((?:  )+)(<[^>]+>)(.*)(<[^>]+>)$", Compiled);
        private static readonly Regex XmlWord = new Regex(@"<([^ ]+)[^>]*?>[^<]*?</\1>[\S]*|<[^<]*?/>|[\S]+", Compiled);
        private static readonly Regex MdWord = new Regex(@"\[[^]]*?\]\([^)]*?\)[\S]*|[\S]+", Compiled);
        private static readonly Regex Prefix = new Regex(@"^ *(?:[-*>]|\d+\.|) *", Compiled);

        static void Main()
        {
            string path = @"E:\Users\Svick\git\core-docs\xml";
            int lineLengthLimit = 130;

            Console.WindowWidth = lineLengthLimit + 1;

            var files = Directory.EnumerateFiles(path, "*.xml", AllDirectories);

            foreach (var file in files.Skip(100).Take(30))
            {
                //Console.WriteLine(file);

                bool changed = false;

                var inputLines = File.ReadAllLines(file);

                var outputLines = new List<string>();

                bool docs = false;

                foreach (var line in inputLines)
                {
                    if (!docs)
                    {
                        outputLines.Add(line);

                        if (line.Contains("<Docs>"))
                            docs = true;

                        continue;
                    }

                    if (line.Contains("</Docs>"))
                    {
                        docs = false;

                        outputLines.Add(line);

                        continue;
                    }

                    if (line.Length <= lineLengthLimit)
                    {
                        outputLines.Add(line);

                        continue;
                    }

                    changed = true;

                    Console.WriteLine(line);

                    void Add(string part)
                    {
                        Console.WriteLine(part);
                        outputLines.Add(part);
                    }

                    var xmlLineMatch = XmlLine.Match(line);

                    // TODO: XmlLine → XML; CDATA → MD; else → report

                    if (xmlLineMatch.Success)
                    {
                        string indent1 = xmlLineMatch.Groups[1].Value;
                        string indent2 = indent1 + "  ";
                        string opening = xmlLineMatch.Groups[2].Value;
                        string text = xmlLineMatch.Groups[3].Value;
                        string closing = xmlLineMatch.Groups[4].Value;

                        Add(indent1 + opening);

                        foreach (var part in Split(text, lineLengthLimit - indent2.Length, XmlWord))
                        {
                            Add(indent2 + part);
                        }

                        Add(indent1 + closing);
                    }
                    else
                    {
                        string prefix1 = Prefix.Match(line).Value;

                        string prefix2;

                        if (prefix1.TrimStart().StartsWith(">"))
                            prefix2 = prefix1;
                        else
                            prefix2 = new string(' ', prefix1.Length);

                        string text = line.Substring(prefix1.Length);
                        bool first = true;

                        foreach (var part in Split(text, lineLengthLimit - prefix1.Length, MdWord))
                        {
                            string prefix = first ? prefix1 : prefix2;
                            first = false;

                            Add(prefix + part);
                        }
                    }

                    Console.WriteLine();
                }

                /*if (changed)
                    File.WriteAllLines(file, outputLines);*/
            }
        }

        static IEnumerable<string> Split(string s, int lengthLimit, Regex wordRegex)
        {
            var words = wordRegex.Matches(s).Cast<Match>().Select(m => m.Value);

            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (result.Length != 0)
                {
                    if (result.Length + 1 + word.Length > lengthLimit)
                    {
                        yield return result.ToString();
                        result.Clear();
                    }
                    else
                    {
                        result.Append(' ');
                    }
                }

                result.Append(word);
            }

            yield return result.ToString();
        }
    }
}