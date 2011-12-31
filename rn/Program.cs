using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using System.Text.RegularExpressions;

namespace rn
{
    class Program
    {
        static void Main(string[] args)
        {
            Boolean show_help = false;
            Boolean testmode = false;
            Boolean verbose = false;
            Boolean ignorecase = false;
            String dir = ".";
            Match curMatch;
            StringWrapper[] matchvalues;
            SearchOption recursive = SearchOption.TopDirectoryOnly;
            var p = new OptionSet()
            {
                { "t|test", "Don't actually rename", v => testmode = true},
                { "v|verbose", "Display verbose messages", v => verbose = true},
                { "i|ignore-case", "Ignore case", v => ignorecase = true},
                { "d|directory=", "Change default directory", v => dir = v },
                { "r|recursive", "Rename files in all subdirectories", v => recursive = SearchOption.AllDirectories },
                { "h|help", "Show this message and exit", v => show_help = v != null },
            };
            List<string> Regexes;
            try
            {
                Regexes = p.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("rn: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `rn --help' for more information.");
                return;
            }
            if (show_help || (Regexes.Count != 2))
            {
                ShowHelp(p);
                return;
            }
            string[] files = Directory.GetFiles(dir, "*", recursive);
            Regex matcher;
            if (ignorecase)
                matcher = new Regex(Regexes[0],RegexOptions.IgnoreCase);
            else
                matcher = new Regex(Regexes[0]);
            try
            {
                foreach (string file in files)
                {
                    curMatch = matcher.Match(System.IO.Path.GetFileName(file));
                    matchvalues = new StringWrapper[curMatch.Groups.Count-1];
                    for (int i = 1; i < curMatch.Groups.Count; i++)
                        matchvalues[i-1] = new StringWrapper(curMatch.Groups[i].Value);
                    if (curMatch.Success)
                    {
                        if (testmode || verbose)
                            Console.WriteLine("Renaming {0} to {1}\\{2}", file, System.IO.Path.GetDirectoryName(file),String.Format(Regexes[1], matchvalues));
                        if (testmode)
                            continue;
                        System.IO.File.Move(file, System.IO.Path.GetDirectoryName(file) + "\\" + String.Format(Regexes[1], matchvalues));
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("rn: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `rn --help' for more information.");
                return;
            }
        }
        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: rn [OPTIONS] [MATCH REGEX] [REPLACE FORMAT]");
            Console.WriteLine("[MATCH REGEX] expects a Perl-compatible regular expression.");
            Console.WriteLine("[REPLACE FORMAT] expects a .NET-compatible formatting string.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
