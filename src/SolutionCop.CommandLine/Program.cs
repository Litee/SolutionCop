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
                var errors = new List<string>();
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
                bool saveConfigFileOnExit = false;
                foreach (var rule in rules)
                {
                    var xmlRuleConfig = xmlAllRuleConfigs.Elements().First().Element(rule.Id);
                    if (xmlRuleConfig == null)
                    {
                        xmlAllRuleConfigs.Elements().First().Add(rule.DefaultConfig);
                        Console.Out.WriteLine("WARNING: No config specified for rule {0} - adding default one", rule.Id);
                        saveConfigFileOnExit = true;
                    }
                    else
                    {
                        Console.Out.WriteLine("DEBUG: Parsing config for rule {0}...", rule.Id);
                        var ruleConfigErrors = rule.ParseConfig(xmlRuleConfig);
                        if (ruleConfigErrors.Any())
                        {
                            errors.AddRange(ruleConfigErrors);
                            foreach (var error in ruleConfigErrors)
                            {
                                Console.Out.WriteLine("ERROR: {0}", error);
                                Console.Out.WriteLine("ERROR: Rule {0} disabled", rule.Id);
                                saveConfigFileOnExit = true;
                            }
                        }
                    }
                }
                if (saveConfigFileOnExit)
                {
                    Console.Out.WriteLine("DEBUG: Config file was updated. Saving...");
                    xmlAllRuleConfigs.Save(commandLineParameters.PathToConfigFile);
                }


                Console.Out.WriteLine("INFO: Loading config file {0}", commandLineParameters.PathToConfigFile);

                Console.Out.WriteLine("INFO: Starting analyzing...");
                foreach (var projectPath in solutionInfo.ProjectFilePaths)
                {
                    Console.Out.WriteLine("INFO: Analyzing project {0}", projectPath);
                    foreach (var rule in rules)
                    {
                        errors.AddRange(rule.ValidateProject(projectPath));
                    }
                }
                Console.Out.WriteLine("INFO: Analysis finished!");

                if (errors.Any())
                {
                    Console.WriteLine("ERROR: ***** Full list of errors: *****");
                    errors.ForEach(x => Console.WriteLine("ERROR: {0}", x));
                    if (commandLineParameters.BuildServerType == BuildServer.TeamCity)
                    {
                        // adding empty line for a better formatting in TC output
                        var extendedErrorsInfo = Enumerable.Repeat("", 1)
                            .Concat(errors.Select((x, idx) => string.Format("ERROR: ({0}/{1}): {2}", idx, errors.Count, x)))
                            .Concat(Enumerable.Repeat("", 1))
                            .Concat(rules.Select(x => string.Format("Rule: {0} is {1}", x.Id, x.IsEnabled ? "enabled" : "disabled")));
                        Console.WriteLine("##teamcity[testFailed message='FAILED ({0})' details='{1}']", EscapeForTeamCity(Path.GetFileName(commandLineParameters.PathToConfigFile)), string.Join("|r|n", extendedErrorsInfo.Select(EscapeForTeamCity)));
                        Console.WriteLine("##teamcity[buildStatus text='FAILED ({0})']", EscapeForTeamCity(Path.GetFileName(commandLineParameters.PathToConfigFile)));
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
                Environment.Exit(errors.Any() ? -1 : 0);
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private static string EscapeForTeamCity(string originalString)
        {
            return originalString.Replace("|", "||").Replace("'", "|'").Replace("\r", "|r").Replace("\n", "|n").Replace("]", "|]");
        }
    }
}