using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class WarningLevelRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptionProjectNames;
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
                element.SetAttributeValue("minimalValue", 4);
                element.Add(new XElement("Exception", "FakeProject.csproj"));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            if (IsEnabled)
            {
                if (!Int32.TryParse((string) xmlRuleConfigs.Attribute("minimalValue"), out _requiredWarningLevel))
                {
                    yield return string.Format("Bad config for rule {0}. 'minimalValue' attribute must be an integer.", Id);
                }
            }
            _exceptionProjectNames = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (_exceptionProjectNames.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping warning level check as an exception for project {0}", projectFileName);
            }
            else
            {
                var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
                foreach (var xmlPropertyGroupsWithCondition in xmlPropertyGroupsWithConditions)
                {
                    var xmlWarningLevel = xmlPropertyGroupsWithCondition.Descendants(Namespace + "WarningLevel").FirstOrDefault();
                    var warningLevelInProject = xmlWarningLevel == null ? 0 : Int32.Parse(xmlWarningLevel.Value);
                    if (warningLevelInProject < _requiredWarningLevel)
                    {
                        yield return string.Format("Warning level {0} is lower than required {1} in project {2}", warningLevelInProject, _requiredWarningLevel, projectFileName);
                        yield break;
                    }
                }
            }
        }
    }
}