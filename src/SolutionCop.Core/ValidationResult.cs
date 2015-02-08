namespace SolutionCop.Core
{
    public class ValidationResult
    {
        public string RuleId { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool HasErrorsInConfiguration { get; private set; }
        public string[] Errors { get; private set; }

        public ValidationResult(string ruleId, bool isEnabled, bool hasErrorsInConfiguration, string[] errors)
        {
            RuleId = ruleId;
            IsEnabled = isEnabled;
            HasErrorsInConfiguration = hasErrorsInConfiguration;
            Errors = errors;
        }
    }
}