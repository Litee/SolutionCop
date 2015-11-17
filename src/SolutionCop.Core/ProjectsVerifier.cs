using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SolutionCop.Core
{
    public class ProjectsVerifier
    {
        private readonly IAnalysisLogger _logger;
        private readonly RulesDirectoryCatalog _rulesDirectoryCatalog;
        private readonly ConfigurationFileParser _configurationFileParser;

        public ProjectsVerifier(IAnalysisLogger logger)
        {
            _logger = logger;
            _configurationFileParser = new ConfigurationFileParser(logger);
            _rulesDirectoryCatalog = new RulesDirectoryCatalog(logger);
        }

        public List<string> VerifyProjects(string pathToConfigFile, string[] projectFilePaths, BuildServer buildServerType = BuildServer.None)
        {
            var errors = new List<string>();
            var validationResults = new List<ValidationResult>();

            var rules = _rulesDirectoryCatalog.LoadRules();

            var ruleConfigsMap = _configurationFileParser.ParseConfigFile(pathToConfigFile, rules, errors);

            _logger.LogInfo("INFO: Starting analysis...");
            foreach (var rule in rules)
            {
                var xmlRuleConfigs = ruleConfigsMap[rule.Id];
                if (xmlRuleConfigs == null)
                {
                    errors.Add(String.Format("Configuration section is not found for rule {0}", rule.Id));
                    continue;
                }
                _logger.LogInfo("INFO: Analyzing projects using rule {0}", rule.Id);
                var validationResult = rule.ValidateAllProjects(xmlRuleConfigs, projectFilePaths);
                errors.AddRange(validationResult.Errors);
                validationResults.Add(validationResult);
            }
            _logger.LogInfo("INFO: Analysis finished!");

            if (errors.Any())
            {
                _logger.LogError("ERROR: ***** Full list of errors: *****");
                errors.ForEach(x => _logger.LogError("ERROR: {0}", x));
                if (buildServerType == BuildServer.TeamCity)
                {
                    // adding empty line for a better formatting in TC output
                    var extendedErrorsInfo = Enumerable.Repeat(String.Empty, 1).Concat(errors.Select((x, idx) => String.Format("ERROR ({0}/{1}): {2}", idx + 1, errors.Count, x))).Concat(Enumerable.Repeat(String.Empty, 1)).Concat(validationResults.Select(x => String.Format("INFO: Rule {0} is {1}", x.RuleId, x.IsEnabled ? "enabled" : "disabled")));
                    Console.Out.WriteLine("##teamcity[testFailed name='SolutionCop' message='FAILED - {0}' details='{1}']", EscapeForTeamCity(Path.GetFileName(pathToConfigFile)), string.Join("|r|n", extendedErrorsInfo.Select(EscapeForTeamCity)));
                    Console.Out.WriteLine("##teamcity[buildStatus text='FAILED - {0}']", EscapeForTeamCity(Path.GetFileName(pathToConfigFile)));
                }
            }
            else
            {
                _logger.LogInfo("INFO: No errors found!");
                if (buildServerType == BuildServer.TeamCity)
                {
                    Console.Out.WriteLine("##teamcity[buildStatus status='SUCCESS' text='PASSED - {0}']", EscapeForTeamCity(Path.GetFileName(pathToConfigFile)));
                }
            }
            return errors;
        }

        private static string EscapeForTeamCity(string originalString)
        {
            return originalString.Replace("|", "||").Replace("'", "|'").Replace("\r", "|r").Replace("\n", "|n").Replace("]", "|]");
        }
    }
}
