using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class WarningLevelRule : StandardProjectRule
    {
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
                element.SetValue(4);
                return element;
            }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            int requiredWarningLevel;
            if (!Int32.TryParse(xmlRuleConfigs.Value.Trim(), out requiredWarningLevel))
            {
                yield return string.Format("Bad parameter format in config for rule {0}. Must be an integer.", Id);
                yield break;
            }
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupsWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlWarningLevel = xmlPropertyGroupsWithCondition.Descendants(Namespace + "WarningLevel").FirstOrDefault();
                var warningLevelInProject = xmlWarningLevel == null ? 0 : Int32.Parse(xmlWarningLevel.Value);
                if (warningLevelInProject < requiredWarningLevel)
                {
                    yield return string.Format("Warning level {0} is lower than required {1} in project {2}", warningLevelInProject, requiredWarningLevel, Path.GetFileName(projectFilePath));
                    yield break;
                }
            }
        }
    }
}