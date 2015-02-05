using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TargetFrameworkVersionRule : StandardProjectRule
    {
        private IEnumerable<string> _targetFrameworkVersions;

        public override string DisplayName
        {
            get { return "Verify target .NET framework version"; }
        }

        public override string Id
        {
            get { return "TargetFrameworkVersion"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("AllowedValue", "4.0"));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            _targetFrameworkVersions = xmlRuleConfigs.Elements("AllowedValue").Select(x => x.Value.Trim());
            if (IsEnabled && !_targetFrameworkVersions.Any())
            {
                yield return string.Format("No allowed values specified for rule {0}", Id);
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var invalidFrameworkVersions = xmlProject.Descendants(Namespace + "TargetFrameworkVersion").Select(x => x.Value.Substring(1)).Where(x => _targetFrameworkVersions.All(y => y != x));
            if (invalidFrameworkVersions.Any())
            {
                yield return string.Format("Invalid target .NET framework version '{0}' in project {1}", invalidFrameworkVersions.First(), Path.GetFileName(projectFilePath));
            }
        }
    }
}
