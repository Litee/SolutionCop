using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CommandLine;

namespace SolutionCop
{
    internal class Program
    {
        private static readonly RulesDirectoryCatalog RulesDirectoryCatalog = new RulesDirectoryCatalog();
        private static readonly SolutionParser SolutionParser = new SolutionParser();

        private static void Main(string[] args)
        {
            var commandLineParameters = new CommandLineParameters();
            if (Parser.Default.ParseArguments(args, commandLineParameters))
            {
                var solutionInfo = SolutionParser.LoadFromFile(commandLineParameters.PathToSolution);

                if (string.IsNullOrEmpty(commandLineParameters.PathToConfigFile))
                {
                    commandLineParameters.PathToConfigFile = Path.Combine(Path.GetDirectoryName(commandLineParameters.PathToSolution), "SolutionCop.xml");
                    Console.Out.WriteLine("INFO: Custom path to config file is not specified, using default one: {0}", commandLineParameters.PathToConfigFile);
                }
                if (File.Exists(commandLineParameters.PathToConfigFile))
                {
                    Console.Out.WriteLine("INFO: Existing config file found: {0}", commandLineParameters.PathToConfigFile);
                }
                else
                {
                    Console.Out.WriteLine("WARN: Config file does not exist. Saving default one to {0}", commandLineParameters.PathToConfigFile);
                    using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("SolutionCop.DefaultSolutionCop.xml"))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            File.WriteAllText(commandLineParameters.PathToConfigFile, reader.ReadToEnd());
                        }
                    }
                }
                Console.Out.WriteLine("INFO: Loading config file {0}", commandLineParameters.PathToConfigFile);
                var xmlAllRuleConfigs = XDocument.Load(commandLineParameters.PathToConfigFile);

                var rules = RulesDirectoryCatalog.LoadRules();
                if (!solutionInfo.IsParsed)
                {
                    Console.Out.WriteLine("FATAL: Cannot parse solution file.");
                    Environment.Exit(-1);
                }

                Console.Out.WriteLine("INFO: Starting analyzing...");
                var errors = new List<string>();
                foreach (var projectPath in solutionInfo.ProjectFilePaths)
                {
                    Console.Out.WriteLine("INFO: Analyzing project {0}", projectPath);
                    foreach (var rule in rules)
                    {
                        var xmlRuleConfig = xmlAllRuleConfigs.Elements().First().Element(rule.Id);
                        if (xmlRuleConfig == null)
                        {
                            // TODO Insert XML element into config
                            Console.Out.WriteLine("WARNING: No config specified for rule {0} - skipping", rule.Id);
                        }
                        else
                        {
                            errors.AddRange(rule.ValidateProject(projectPath, xmlRuleConfig));
                        }
                    }
                }
                if (errors.Any())
                {
                    errors.ForEach(x => Console.WriteLine("ERROR: {0}", x));
                }
                else
                {
                    Console.Out.WriteLine("INFO: No errors found!");
                }
                Console.Out.WriteLine("INFO: Analysis finished!");
                Environment.Exit(errors.Any() ? -1 : 0);
            }
            else
            {
                Environment.Exit(-1);
            }
        }
    }
}