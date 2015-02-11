using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SolutionCop.Core;
using SolutionCop.DefaultRules.Properties;

namespace SolutionCop.DefaultRules
{
    public abstract class ProjectRule<T> : IProjectRule
    {
        protected readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

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

        protected abstract T ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors);

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

/*
        protected void ValidateConfigSectionStructure(XElement xmlConfig, IDictionary<string, string[]> allowedItems, List<string> errors)
        {
            var unknownFirstLevelElements = xmlConfig.Elements().Select(x => x.Name.LocalName).Where(x => allowedItems.Keys.All(y => y != x)).ToArray();
            errors.Add(string.Format(Resources.BadConfiguration, Id, string.Format("Unknown element(s) {0} in configuration.", string.Join(",", unknownFirstLevelElements))));

            foreach (var xmlFirstLevel in xmlConfig.Elements())
            {
                var firstLevelElementName = xmlFirstLevel.Name.LocalName;
                string[] allowedSecondLevelItems;
                if (allowedItems.TryGetValue(firstLevelElementName, out allowedSecondLevelItems))
                {
                    var unknownSecondLevelElements = xmlFirstLevel.Elements().Select(x => x.Name.LocalName).Where(x => allowedSecondLevelItems.All(y => y != x)).ToArray();
                    errors.Add(string.Format(Resources.BadConfiguration, Id, string.Format("Unknown element(s) {0} in configuration.", string.Join(",", unknownSecondLevelElements))));
                }
            }
        }
*/

        protected abstract IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, T exceptions);
    }
}