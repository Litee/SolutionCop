using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SolutionCop.Core;

namespace SolutionCop.DefaultRules
{
    public abstract class StandardProjectRule : IProjectRule
    {
        protected readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public abstract string Id { get; }

        public abstract string DisplayName { get; }

        public virtual XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                return element;
            }
        }

        public virtual IEnumerable<string> ValidateConfig(XElement xmlRuleConfigs)
        {
            yield break;
        }

        public IEnumerable<string> ValidateProject(string projectFilePath, XElement xmlRuleConfig)
        {
            if (!ValidateConfig(xmlRuleConfig).Any())
            {
                var xmlEnabled = xmlRuleConfig.Attribute("enabled");
                if (xmlEnabled == null || xmlEnabled.Value.ToLower() != "false")
                {
                    if (File.Exists(projectFilePath))
                    {
                        var xmlProject = XDocument.Load(projectFilePath);
                        foreach (var error in ValidateProjectWithEnabledRule(xmlProject, projectFilePath, xmlRuleConfig))
                        {
                            yield return error;
                        }
                    }
                    else
                    {
                        yield return string.Format("Project file not found: {0}", projectFilePath);
                    }
                }
            }
        }

        protected abstract IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs);
    }
}