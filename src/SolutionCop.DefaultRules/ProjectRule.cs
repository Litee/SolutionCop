namespace SolutionCop.DefaultRules
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;
    using Properties;

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
                            errors.Add(string.Format("Project file not found: {0}", Path.GetFileName(projectFilePath)));
                        }
                    }
                }
            }
            else if (isEnabledString.ToLower() != "false")
            {
                errors.Add(string.Format("Error in config for rule {0} - 'enabled' attribute has wrong value.", Id));
            }
            return new ValidationResult(Id, isEnabled, hasErrorsInConfiguration, errors.ToArray());
        }

        protected abstract T ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors);

        protected void ValidateConfigSectionForAllowedElements(XElement xmlElement, List<string> errors, params string[] allowedElementNames)
        {
            var unknownElements = xmlElement.Elements()
                .Select(x => x.Name.LocalName)
                .Where(x => allowedElementNames.All(y => y != x))
                .Select(x => string.Format("<{0}>", x))
                .ToArray();
            if (unknownElements.Any())
            {
                var entryOrEntries = unknownElements.Count() == 1 ? "entry" : "entries";
                var allowedElementsList = string.Join(", ", allowedElementNames.Select(x => string.Format("<{0}>", x)));
                var errorDetails = string.Format("Unknown {0} within <{1}>: {2}. Allowed entries: {3}.", entryOrEntries, xmlElement.Name.LocalName, string.Join(", ", unknownElements), allowedElementsList);
                errors.Add(string.Format(Resources.BadConfiguration, Id, errorDetails));
            }
        }

/*
        protected void ValidateConfigSectionForRequiredElements(XElement xmlElement, List<string> errors, params string[] requiredElementNames)
        {
            var missingElements = requiredElementNames
                .Where(x => xmlElement.Element(x) == null)
                .Select(x => string.Format("<{0}>", x))
                .ToArray();
            if (missingElements.Count() == 1)
            {
                errors.Add(string.Format(Resources.BadConfiguration, Id, string.Format("Missing configuration entry {0}.", string.Join(",", missingElements))));
            }
            else if (missingElements.Count() > 1)
            {
                errors.Add(string.Format(Resources.BadConfiguration, Id, string.Format("Missing configuration entries {0}.", string.Join(",", missingElements))));
            }
        }
*/

        protected abstract IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, T exceptions);
    }
}