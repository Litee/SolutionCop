using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SolutionCop.API;

namespace SolutionCop
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintAppHelp();
                Environment.Exit(-1);
            }
            var pathToSolution = args[0];
            var pathToRuleParameters = args[1];
            if (!new FileInfo(pathToSolution).Exists || !new FileInfo(pathToRuleParameters).Exists)
            {
                PrintAppHelp();
                Environment.Exit(-1);
            }

            var rules = FindRules();

            var solutionFileLines = File.ReadAllLines(pathToSolution).ToArray();
            var projectReferenceRegEx = new Regex("Project\\(\"([\\{\\}0-9A-Z\\-]+)\"\\) = \"(.*)\", \"(.*.csproj)\", \"([\\{\\}0-9A-Z\\-]+)\"\\s*");
            var projectRelativePaths = solutionFileLines.Select(line => projectReferenceRegEx.Match(line)).Where(x => x.Success).Select(x => x.Groups[3].ToString());
            var projectPaths = projectRelativePaths.Select(x => new FileInfo(pathToSolution).Directory.FullName + "\\" + x);

            var xmlAllRuleParameters = XDocument.Load(pathToRuleParameters);

            Console.Out.WriteLine("Analyzing...");
            foreach (var projectPath in projectPaths)
            {
                foreach (var rule in rules)
                {
                    var xmlRuleParameters = xmlAllRuleParameters.Element(rule.Id);
                    rule.ValidateProject(projectPath, xmlRuleParameters);
                }

            }
            Console.Out.WriteLine("Finished!");
        }

        private static List<IRule> FindRules()
        {
            Console.Out.WriteLine("Scanning for rules...");
            var rules = new List<IRule>();
            var assemblies = new DirectoryInfo(".").GetFiles("*.DLL");
            foreach (var assemblyFile in assemblies)
            {
                var ruleTypes = Assembly.LoadFrom(assemblyFile.FullName).GetExportedTypes().Where(t => t.IsClass && typeof (IRule).IsAssignableFrom(t));
                foreach (var ruleType in ruleTypes)
                {
                    var rule = (IRule) Activator.CreateInstance(ruleType);
                    Console.Out.WriteLine("Rule found: '{0}'", rule.DisplayName);
                    rules.Add(rule);
                }
            }
            Console.Out.WriteLine("{0} rules found!", rules.Count);
            return rules;
        }

        private static void PrintAppHelp()
        {
            // TODO Output app usage
            Console.WriteLine("Here will be some command-line help");
        }
    }
}
