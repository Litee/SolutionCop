using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SolutionCop
{
    class Program
    {
        private static readonly RulesDirectoryCatalog RulesDirectoryCatalog = new RulesDirectoryCatalog();
        private static readonly SolutionParser SolutionParser = new SolutionParser();

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintAppHelp();
                Environment.Exit(-1);
            }
            var pathToSolution = args[0];
            var pathToRuleConfigs = args[1];
            var solutionInfo = SolutionParser.LoadFromFile(pathToSolution);

            if (!new FileInfo(pathToRuleConfigs).Exists)
            {
                Console.Out.WriteLine("FATAL: Cannot find configuration file {0}", pathToSolution);
                Environment.Exit(-1);
            }

            var rules = RulesDirectoryCatalog.LoadRules();
            if (!solutionInfo.IsParsed)
            {
                Console.Out.WriteLine("FATAL: Cannot parse solution file.");
                Environment.Exit(-1);
            }
            var xmlAllRuleConfigs = XDocument.Load(pathToRuleConfigs);

            Console.Out.WriteLine("Analyzing...");
            foreach (var projectPath in solutionInfo.ProjectFilePaths)
            {
                foreach (var rule in rules)
                {
                    var xmlRuleConfig = xmlAllRuleConfigs.Element(rule.Id);
                    rule.ValidateProject(projectPath, xmlRuleConfig);
                }

            }
            Console.Out.WriteLine("Finished!");
        }

        private static void PrintAppHelp()
        {
            // TODO Output app usage
            Console.WriteLine("Here will be some command-line help");
        }
    }
}
