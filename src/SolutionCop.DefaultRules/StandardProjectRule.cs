using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public abstract class StandardProjectRule : IProjectRule
    {
        protected readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public abstract string Id { get; }

        public abstract string DisplayName { get; }

        public XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                return element;
            }
        }

        public IEnumerable<string> Validate(string projectFilePath, XElement xmlRuleConfigs)
        {
            var xmlEnabled = xmlRuleConfigs.Attribute("enabled");
            if (xmlEnabled == null || xmlEnabled.Value.ToLower() != "false")
            {
                var xmlProject = XDocument.Load(projectFilePath);
                return ValidateProjectWithEnabledRule(xmlProject, projectFilePath, xmlRuleConfigs);
            }
            return Enumerable.Empty<string>();
        }

        protected abstract IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs);
    }
}