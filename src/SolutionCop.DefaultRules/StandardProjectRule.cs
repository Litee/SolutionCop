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

        protected abstract IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs);

        public ValidationResult ValidateProject(string projectFilePath, XElement xmlRuleConfigs)
        {
            var isEnabled = false;
            var hasErrorsInConfiguration = false;
            var errors = new List<string>();
            // Check that config section is correct
            if (xmlRuleConfigs.Name.LocalName != Id)
            {
                throw new InvalidOperationException("Configuration section has invalid name");
            }

            // Is ruled enabled?
            var isEnabledString = (string)xmlRuleConfigs.Attribute("enabled");
            if (isEnabledString == null || isEnabledString.ToLower() == "true")
            {
                isEnabled = true;

                var configurationErrors = ParseConfigSectionCustomParameters(xmlRuleConfigs).ToArray();
                if (configurationErrors.Any())
                {
                    hasErrorsInConfiguration = true;
                    errors.AddRange(configurationErrors);
                }
                else
                {
                    if (File.Exists(projectFilePath))
                    {
                        var xmlProject = XDocument.Load(projectFilePath);
                        errors.AddRange(ValidateProjectPrimaryChecks(xmlProject, projectFilePath));
                    }
                    else
                    {
                        errors.Add(string.Format("Project file not found: {0}", Path.GetFileName(projectFilePath)));
                    }
                }
            }
            else if (isEnabledString.ToLower() != "false")
            {
                errors.Add(string.Format("Error in config for rule {0} - 'enabled' attribute has wrong value.", Id));
            }
            return new ValidationResult(Id, isEnabled, hasErrorsInConfiguration, errors.ToArray());
        }

        protected abstract IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath);
    }
}