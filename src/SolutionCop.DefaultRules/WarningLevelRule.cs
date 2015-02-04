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
                element.SetAttributeValue("minimalValue", 4);
                element.Add(new XElement("Exception", "FakeProject.csproj"));
                return element;
            }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            var exceptionProjectNames = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
            var projectFileName = Path.GetFileName(projectFilePath);
            if (exceptionProjectNames.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping warning level check as an exception for project {0}", projectFileName);
            }
            else
            {
                int requiredWarningLevel;
                if (!Int32.TryParse((string)xmlRuleConfigs.Attribute("minimalValue"), out requiredWarningLevel))
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
                        yield return string.Format("Warning level {0} is lower than required {1} in project {2}", warningLevelInProject, requiredWarningLevel, projectFileName);
                        yield break;
                    }
                }
            }
        }
    }
}