using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class WarningLevelRule : StandardProjectRule
    {
        private readonly IDictionary<string, int> _exceptions = new Dictionary<string, int>();
        private int _requiredWarningLevel = 4;

        public override string DisplayName
        {
            get { return "Verify warning level"; }
        }

        public override string Id
        {
            get { return "WarningLevel"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("MinimalValue", "4"));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectThatIsAllowedToHaveWarningLevel_2.csproj"), new XElement("MinimalValue", "2")));
                element.Add(new XElement("Exception", new XElement("Project", "AnotherProjectToFullyExcludeFromChecks.csproj")));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            var xmlMinimalValue = xmlRuleConfigs.Element("MinimalValue");
            if (xmlMinimalValue == null)
            {
                yield return string.Format("Bad configuration for rule {0}: <MinimalValue> element is missing.", Id);
            }
            else if (!Int32.TryParse((string)xmlMinimalValue, out _requiredWarningLevel))
            {
                yield return string.Format("Bad configuration for rule {0}: <MinimalValue> element must contain an integer.", Id);
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
                    xmlMinimalValue = xmlException.Element("MinimalValue");
                    var minimalValue = xmlMinimalValue == null ? 0 : Convert.ToInt32(xmlMinimalValue.Value.Trim());
                    _exceptions.Add(xmlProject.Value, minimalValue);
                }
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            var requiredWarningLevel = _requiredWarningLevel;
            if (_exceptions.ContainsKey(projectFileName))
            {
                requiredWarningLevel = _exceptions[projectFileName];
                Console.Out.WriteLine("DEBUG: Project has exceptional warning level {0}: {1}", requiredWarningLevel, projectFileName);
            }
            else
            {
                Console.Out.WriteLine("DEBUG: Project has standard warning level {0}: {1}", requiredWarningLevel, projectFileName);
            }
            var xmlGlobalPropertyGroups = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") == null);
            foreach (var xmlPropertyGroup in xmlGlobalPropertyGroups)
            {
                var xmlWarningLevel = xmlPropertyGroup.Descendants(Namespace + "WarningLevel").FirstOrDefault();
                var warningLevelInProject = xmlWarningLevel == null ? 0 : Int32.Parse(xmlWarningLevel.Value);
                if (warningLevelInProject >= requiredWarningLevel)
                {
                    Console.Out.WriteLine("DEBUG: Project has acceptable warning level in global section {0}: {1}", warningLevelInProject, projectFileName);
                    yield break;
                }
            }
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupsWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlWarningLevel = xmlPropertyGroupsWithCondition.Descendants(Namespace + "WarningLevel").FirstOrDefault();
                var warningLevelInProject = xmlWarningLevel == null ? 0 : Int32.Parse(xmlWarningLevel.Value);
                if (warningLevelInProject < requiredWarningLevel)
                {
                    yield return string.Format("Warning level {0} is lower than required {1} in project {2}", warningLevelInProject, requiredWarningLevel, projectFileName);
                    yield break;
                }
            }
        }
    }
}