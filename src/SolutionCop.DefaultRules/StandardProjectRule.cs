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
        private bool _isEnabled;

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

        public bool IsEnabled
        {
            get { return _isEnabled; }
        }

        // TODO Return a monade
        public IEnumerable<string> ParseConfig(XElement xmlRuleConfigs)
        {
            if (xmlRuleConfigs.Name.LocalName != Id)
            {
                throw new InvalidOperationException("Configuration section has invalid name");
            }
            var isEnabledString = (string)xmlRuleConfigs.Attribute("enabled");
            if (isEnabledString == null || isEnabledString.ToLower() == "true")
            {
                _isEnabled = true;
            }
            else if (isEnabledString.ToLower() != "false")
            {
                yield return string.Format("Error in config for rule {0} - 'enabled' attribute has wrong value.", Id);
            }
            var errors = ParseConfigSectionCustomParameters(xmlRuleConfigs);
            foreach (var error in errors)
            {
                _isEnabled = false;
                yield return error;
            }
        }

        protected abstract IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs);

        public IEnumerable<string> ValidateProject(string projectFilePath)
        {
            if (IsEnabled)
            {
                if (File.Exists(projectFilePath))
                {
                    var xmlProject = XDocument.Load(projectFilePath);
                    foreach (var error in ValidateProjectPrimaryChecks(xmlProject, projectFilePath))
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

        protected abstract IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath);
    }
}