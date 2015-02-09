using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TargetFrameworkVersionRule : StandardProjectRule
    {
        private IEnumerable<string> _targetFrameworkVersions;
        private readonly IDictionary<string, string[]> _exceptions = new Dictionary<string, string[]>();

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
                element.Add(new XElement("FrameworkVersion", "4.0"));
                element.Add(new XElement("FrameworkVersion", "4.5"));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectCanHaveTargetVersion3_5.csproj"), new XElement("FrameworkVersion", "3.5")));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception" && x != "FrameworkVersion").ToArray();
            if (unknownElements.Any())
            {
                yield return string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements));
                yield break;
            }
            _targetFrameworkVersions = xmlRuleConfigs.Elements("FrameworkVersion").Select(x => x.Value.Trim());
            if (!_targetFrameworkVersions.Any())
            {
                yield return string.Format("No target version specified for rule {0}", Id);
            }
            // Clear is required for cases when errors are enumerated twice
            _exceptions.Clear();
            foreach (var xmlException in xmlRuleConfigs.Descendants("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    yield return string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id);
                }
                else
                {
                    var warnings = xmlException.Elements("FrameworkVersion").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    _exceptions.Add(xmlProject.Value, warnings.ToArray());
                }
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            var targetFrameworkVersions = _targetFrameworkVersions;
            if (_exceptions.ContainsKey(projectFileName))
            {
                if (_exceptions[projectFileName] == null || !_exceptions[projectFileName].Any())
                {
                    Console.Out.WriteLine("DEBUG: Project can target any framework version: {0}", projectFileName);
                    yield break;
                }
                else
                {
                    targetFrameworkVersions = targetFrameworkVersions.Concat(_exceptions[projectFileName]);
                }
            }
            var invalidFrameworkVersions = xmlProject.Descendants(Namespace + "TargetFrameworkVersion").Select(x => x.Value.Substring(1)).Where(x => targetFrameworkVersions.All(y => y != x));
            if (invalidFrameworkVersions.Any())
            {
                yield return string.Format("Invalid target .NET framework version '{0}' in project {1}", invalidFrameworkVersions.First(), Path.GetFileName(projectFilePath));
            }
        }
    }
}
