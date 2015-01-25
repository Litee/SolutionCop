using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using SolutionCop.API;

namespace SolutionCop.DefaultRules
{
    public abstract class StandardProjectRule : IRule
    {
        protected readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public abstract string Id { get; }

        public abstract string DisplayName { get; }

        public IEnumerable<string> ValidateProject(string projectFilePath, XElement xmlRuleConfigs)
        {
            var xmlEnabled = xmlRuleConfigs.Attribute("enabled");
            if (xmlEnabled == null || xmlEnabled.Value.ToLower() != "false")
            {
            var xmlProject = XDocument.Load(projectFilePath);
                return ValidateProjectWithEnabledRule(projectFilePath, xmlRuleConfigs, xmlProject);
            }
            return Enumerable.Empty<string>();
        }

        protected abstract IEnumerable<string> ValidateProjectWithEnabledRule(string projectFilePath, XElement xmlRuleConfigs, XDocument xmlProject);
    }
}