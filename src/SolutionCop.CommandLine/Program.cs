using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommandLine;

namespace SolutionCop.CommandLine
{
    internal static class Program
    {
        private static readonly RulesDirectoryCatalog RulesDirectoryCatalog = new RulesDirectoryCatalog();

        private static void Main(string[] args)
        {
            var commandLineParameters = new CommandLineParameters();
            if (Parser.Default.ParseArguments(args, commandLineParameters))
            {
                var solutionInfo = SolutionParser.LoadFromFile(commandLineParameters.PathToSolution);

                var rules = RulesDirectoryCatalog.LoadRules();
                if (!solutionInfo.IsParsed)
                {
                    Console.Out.WriteLine("FATAL: Cannot parse solution file.");
                    Environment.Exit(-1);
                }

                XDocument xmlAllRuleConfigs;
                if (string.IsNullOrEmpty(commandLineParameters.PathToConfigFile))
                {
                    commandLineParameters.PathToConfigFile = Path.Combine(Path.GetDirectoryName(commandLineParameters.PathToSolution), "SolutionCop.xml");
                    Console.Out.WriteLine("INFO: Custom path to config file is not specified, using default one: {0}", commandLineParameters.PathToConfigFile);
                }
                if (File.Exists(commandLineParameters.PathToConfigFile))
                {
                    Console.Out.WriteLine("INFO: Existing config file found: {0}", commandLineParameters.PathToConfigFile);
                    xmlAllRuleConfigs = XDocument.Load(commandLineParameters.PathToConfigFile);
                }
                else
                {
                    Console.Out.WriteLine("WARN: Config file does not exist. Creating a new one {0}", commandLineParameters.PathToConfigFile);
                    xmlAllRuleConfigs = new XDocument();
                    xmlAllRuleConfigs.Add(new XElement("Rules"));
                }
                Console.Out.WriteLine("INFO: Checking rule sections {0}", commandLineParameters.PathToConfigFile);
                bool saveRequired = false;
                foreach (var rule in rules)
                {
                    var xmlRuleConfig = xmlAllRuleConfigs.Elements().First().Element(rule.Id);
                    if (xmlRuleConfig == null)
                    {
                        xmlAllRuleConfigs.Elements().First().Add(rule.DefaultConfig);
                        Console.Out.WriteLine("WARNING: No config specified for rule {0} - adding default one", rule.Id);
                        saveRequired = true;
                    }
                    else
                    {
                        Console.Out.WriteLine("DEBUG: Validating config for rule {0}", rule.Id);
                        foreach (var error in rule.ValidateConfig(xmlRuleConfig))
                        {
                            Console.Out.WriteLine("ERROR: {0}", error);
                            xmlRuleConfig.SetAttributeValue("enabled", false);
                            saveRequired = true;
                        }
                    }
                }
                if (saveRequired)
                {
                    xmlAllRuleConfigs.Save(commandLineParameters.PathToConfigFile);
                }


                Console.Out.WriteLine("INFO: Loading config file {0}", commandLineParameters.PathToConfigFile);

                Console.Out.WriteLine("INFO: Starting analyzing...");

                var errors = new List<string>();
                foreach (var projectPath in solutionInfo.ProjectFilePaths)
                {
                    Console.Out.WriteLine("INFO: Analyzing project {0}", projectPath);
                    foreach (var rule in rules)
                    {
                        var xmlRuleConfig = xmlAllRuleConfigs.Elements().First().Element(rule.Id);
                        errors.AddRange(rule.ValidateProject(projectPath, xmlRuleConfig));
                    }
                }
                if (errors.Any())
                {
                    Console.WriteLine("ERROR: ***** Full list of errors: *****");
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