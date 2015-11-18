using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SolutionCop.Core
{
    public class ProjectsVerifier
    {
        private readonly ISolutionCopConsole _logger;
        private readonly RulesDirectoryCatalog _rulesDirectoryCatalog;
        private readonly ConfigurationFileParser _configurationFileParser;

        public ProjectsVerifier(ISolutionCopConsole logger)
        {
            _logger = logger;
            _configurationFileParser = new ConfigurationFileParser(logger);
            _rulesDirectoryCatalog = new RulesDirectoryCatalog(logger);
        }

        public VerificationResult VerifyProjects(string pathToConfigFile, string[] projectFilePaths, Action<List<string>, List<ValidationResult>> dumpResultsAction)
        {
            var errors = new List<string>();
            var validationResults = new List<ValidationResult>();

            var rules = _rulesDirectoryCatalog.LoadRules();

            var ruleConfigsMap = _configurationFileParser.ParseConfigFile(pathToConfigFile, rules, errors);

            _logger.LogInfo("Starting analysis...");
            foreach (var rule in rules)
            {
                var xmlRuleConfigs = ruleConfigsMap[rule.Id];
                if (xmlRuleConfigs == null)
                {
                    errors.Add(String.Format("Configuration section is not found for rule {0}", rule.Id));
                    continue;
                }
                _logger.LogInfo("Analyzing projects using rule {0}", rule.Id);
                var validationResult = rule.ValidateAllProjects(xmlRuleConfigs, projectFilePaths);
                errors.AddRange(validationResult.Errors);
                validationResults.Add(validationResult);
            }
            _logger.LogInfo("Analysis finished!");

            dumpResultsAction(errors, validationResults);
            return errors.Any() ? VerificationResult.ErrorsFound : VerificationResult.NoErrors;
        }
    }

    public enum VerificationResult
    {
        NoErrors,
        ErrorsFound
    }
}
