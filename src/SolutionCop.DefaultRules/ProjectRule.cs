namespace SolutionCop.DefaultRules
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;

    public abstract class ProjectRule<T> : IProjectRule
    {
        private readonly XNamespace _namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public abstract string Id { get; }

        public virtual XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                return element;
            }
        }

        protected XNamespace Namespace
        {
            get
            {
                return _namespace;
            }
        }

        public ValidationResult ValidateAllProjects(XElement xmlRuleConfigs, params string[] projectFilePaths)
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

                var configurationErrors = new List<string>();
                var ruleConfiguration = ParseConfigurationSection(xmlRuleConfigs, configurationErrors);
                if (configurationErrors.Any())
                {
                    hasErrorsInConfiguration = true;
                    errors.AddRange(configurationErrors);
                }
                else
                {
                    foreach (var projectFilePath in projectFilePaths)
                    {
                        if (File.Exists(projectFilePath))
                        {
                            var xmlProject = XDocument.Load(projectFilePath);
                            errors.AddRange(ValidateSingleProject(xmlProject, projectFilePath, ruleConfiguration));
                        }
                        else
                        {
                            errors.Add($"Project file not found: {Path.GetFileName(projectFilePath)}");
                        }
                    }
                }
            }
            else if (isEnabledString.ToLower() != "false")
            {
                errors.Add($"Error in config for rule {Id} - 'enabled' attribute has wrong value.");
            }
            return new ValidationResult(Id, isEnabled, hasErrorsInConfiguration, errors.ToArray());
        }

        protected abstract T ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors);

        protected void ValidateConfigSectionForAllowedElements(XElement xmlElement, List<string> errors, params string[] allowedElementNames)
        {
            ConfigValidation.ValidateConfigSectionForAllowedElements(xmlElement, errors, Id, allowedElementNames);
        }

        protected abstract IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, T exceptions);
    }
}