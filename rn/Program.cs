using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDesk.Options;
using System.IO;
using System.Text.RegularExpressions;

namespace rn {
    class Program {
        public Int16 inc = 0;
        public Int16 incby = 1;
        public String replacement;
        public Boolean increment = false;
        static void Main(string[] args) {
            Boolean show_help = false;
            Boolean testmode = false;
            Boolean verbose = false;
            Boolean ignorecase = false;
            String dir = ".";
            SearchOption recursive = SearchOption.TopDirectoryOnly;
            Program prog = new Program();
            var p = new OptionSet() {
                { "t|test", "Test run, does not commit any changes", v => testmode = true},
                { "v|verbose", "Display verbose messages", v => verbose = true},
                { "i|ignore-case", "Ignore case", v => ignorecase = true},
                { "I|increment", "Match all files, fill first match with incrementing number", v => prog.increment = true},
                { "b|baseinc=", "Base value (requires -I)", v => prog.inc = Convert.ToInt16(v) },
                { "B|incby=", "Increment by x (requires -I)", v => prog.incby = Convert.ToInt16(v) },
                { "d|directory=", "Change default directory", v => dir = v },
                { "r|recursive", "Rename files in all subdirectories", v => recursive = SearchOption.AllDirectories },
                { "h|help", "Show this message and exit", v => show_help = v != null },
            };
            List<string> Regexes;
            try {
                Regexes = p.Parse(args);
            }
            catch (OptionException e) {
                Console.Write("rn: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `rn --help' for more information.");
                return;
            }
            if (show_help || (prog.increment && (Regexes.Count != 1)) || (!prog.increment && (Regexes.Count != 2))) {
                ShowHelp(p);
                return;
            }
            string[] files = Directory.GetFiles(dir, "*", recursive);
            String pattern;
            if (prog.increment)
                pattern = "(.*)";
            else
                pattern = Regexes[0];
            Regex regengine = new Regex(pattern, ignorecase ? RegexOptions.IgnoreCase : RegexOptions.None);
            MatchEvaluator eval = new MatchEvaluator(prog.incrementReplace);
            try {
                foreach (string file in files) {
                    if (prog.increment)
                        prog.replacement = Regexes[0];
                    else
                        prog.replacement = Regexes[1];
                    if (testmode || verbose)
                        Console.WriteLine("Renaming {0} to {1}\\{2}", file, System.IO.Path.GetDirectoryName(file), regengine.Replace(System.IO.Path.GetFileName(file), eval));
                    if (testmode)
                        continue;
                    System.IO.File.Move(file, System.IO.Path.GetDirectoryName(file) + "\\" + regengine.Replace(System.IO.Path.GetFileName(file), eval));
                }
            }
            catch (Exception e) {
                Console.Write("rn: ");
                Console.WriteLine(e.Message);
#if DEBUG
                Console.WriteLine(e.StackTrace);
#endif
                Console.WriteLine("Try `rn --help' for more information.");
                return;
            }
        }
        public string incrementReplace(Match input) {
            if (input.ToString() == "")
                return null;
            if (increment) {
                inc += incby;
                return Regex.Replace(String.Format("{0:D2}", inc-incby), @"(\d{2})", replacement);
            }
            else
                return input.Result(replacement);
        }
        static void ShowHelp(OptionSet p) {
            Console.WriteLine("Usage: rn [OPTIONS] [MATCH REGEX] [REPLACE REGEX]");
            Console.WriteLine("[MATCH REGEX] expects a Perl-compatible regular expression.");
            Console.WriteLine("[REPLACE REGEX] expects a Perl-compatible replacement regular expression.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
