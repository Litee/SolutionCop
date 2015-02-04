using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommandLine;
using SolutionCop.Core;

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

                var rules = RulesDirectoryCatalog.LoadRules();
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
                        Console.Out.WriteLine("DEBUG: Checking config for rule {0}", rule.Id);
                        var ruleConfigErrors = rule.ValidateConfig(xmlRuleConfig);
                        if (ruleConfigErrors.Any())
                        {
                            foreach (var error in ruleConfigErrors)
                            {
                                Console.Out.WriteLine("ERROR: {0}", error);
                                xmlRuleConfig.SetAttributeValue("enabled", false);
                                saveRequired = true;
                            }
                        }
                    }
                }
                if (saveRequired)
                {
                    Console.Out.WriteLine("DEBUG: Config file was updated. Saving...");
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
                    if (commandLineParameters.BuildServerType == BuildServer.TeamCity)
                    {
                        Console.WriteLine("##teamcity[buildStatus status='ERROR' text='{0}']", string.Join("|r|n", errors.Select(EscapeForTeamCity)));
                    }
                }
                else
                {
                    Console.Out.WriteLine("INFO: No errors found!");
                    if (commandLineParameters.BuildServerType == BuildServer.TeamCity)
                    {
                        // TODO
                        Console.WriteLine("##teamcity[buildStatus status='SUCCESS' text='{0}']", EscapeForTeamCity(Path.GetFileName(commandLineParameters.PathToConfigFile)));
                    }
                }
                Console.Out.WriteLine("INFO: Analysis finished!");
                Environment.Exit(errors.Any() ? -1 : 0);
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private static object EscapeForTeamCity(string originalString)
        {
            return originalString.Replace("|", "||").Replace("'", "|'").Replace("\r", "|r").Replace("\n", "|n").Replace("]", "|]");
        }
    }
}