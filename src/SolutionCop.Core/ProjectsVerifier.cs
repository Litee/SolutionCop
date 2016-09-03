namespace SolutionCop.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum VerificationResult
    {
        NoErrors,
        ErrorsFound
    }

    public class ProjectsVerifier
    {
        private readonly ISolutionCopConsole _logger;
        private readonly IBuildServerReporter _buildServerReporter;
        private readonly RulesDirectoryCatalog _rulesDirectoryCatalog;
        private readonly ConfigurationFileParser _configurationFileParser;

        public ProjectsVerifier(ISolutionCopConsole logger, IBuildServerReporter buildServerReporter)
        {
            _logger = logger;
            _buildServerReporter = buildServerReporter;
            _configurationFileParser = new ConfigurationFileParser(logger);
            _rulesDirectoryCatalog = new RulesDirectoryCatalog(logger);
        }

        public VerificationResult VerifyProjects(string pathToConfigFile, string[] projectFilePaths, Action<List<string>, List<ValidationResult>> dumpResultsAction)
        {
            const string SuiteName = "SolutionCop";
            var errors = new List<string>();
            var validationResults = new List<ValidationResult>();

            var rules = _rulesDirectoryCatalog.LoadRules();

            var ruleConfigsMap = _configurationFileParser.ParseConfigFile(pathToConfigFile, rules, errors);

            _logger.LogInfo("Starting analysis...");
            _buildServerReporter.TestSuiteStarted(SuiteName);

            foreach (var rule in rules)
            {
                var xmlRuleConfigs = ruleConfigsMap[rule.Id];
                var testName = string.Concat(SuiteName, ".", rule.Id);

                if (xmlRuleConfigs == null)
                {
                    var error = string.Format("Configuration section is not found for rule {0}", rule.Id);
                    errors.Add(error);
                    _buildServerReporter.TestStarted(testName);
                    _buildServerReporter.TestFailed(testName, error, error);
                    _buildServerReporter.TestFinished(testName);
                    continue;
                }

                _logger.LogInfo("Analyzing projects using rule {0}", rule.Id);
                var validationResult = rule.ValidateAllProjects(xmlRuleConfigs, projectFilePaths);

                if (validationResult.IsEnabled)
                {
                    _buildServerReporter.TestStarted(testName);

                    if (validationResult.Errors.Any())
                    {
                        _buildServerReporter.TestFailed(testName, string.Format("Validation failed for rule {0}",  rule.Id), string.Join("\n", validationResult.Errors));
                    }

                    _buildServerReporter.TestFinished(testName);
                }
                else
                {
                    _buildServerReporter.TestIgnored(testName);
                }

                errors.AddRange(validationResult.Errors);
                validationResults.Add(validationResult);
            }

            _buildServerReporter.TestSuiteFinished(SuiteName);
            _logger.LogInfo("Analysis finished!");

            dumpResultsAction(errors, validationResults);
            return errors.Any() ? VerificationResult.ErrorsFound : VerificationResult.NoErrors;
        }
    }
}
