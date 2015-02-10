using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class WarningLevelRule : ProjectRule<Tuple<int, IDictionary<string, int>>>
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
                element.Add(new XElement("MinimalValue", "4"));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectThatIsAllowedToHaveWarningLevel_2.csproj"), new XElement("MinimalValue", "2")));
                element.Add(new XElement("Exception", new XElement("Project", "AnotherProjectToFullyExcludeFromChecks.csproj")));
                return element;
            }
        }

        protected override Tuple<int, IDictionary<string, int>> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception" && x != "MinimalValue").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
            var xmlMinimalValue = xmlRuleConfigs.Element("MinimalValue");
            int requiredWarningLevel = 4;
            if (xmlMinimalValue == null)
            {
                errors.Add(string.Format("Bad configuration for rule {0}: <MinimalValue> element is missing.", Id));
            }
            else if (!Int32.TryParse((string)xmlMinimalValue, out requiredWarningLevel))
            {
                errors.Add(string.Format("Bad configuration for rule {0}: <MinimalValue> element must contain an integer.", Id));
            }
            var exceptions = new Dictionary<string, int>();
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
                else
                {
                    xmlMinimalValue = xmlException.Element("MinimalValue");
                    var minimalValue = xmlMinimalValue == null ? 0 : Convert.ToInt32(xmlMinimalValue.Value.Trim());
                    exceptions.Add(xmlProject.Value, minimalValue);
                }
            }
            return Tuple.Create<int, IDictionary<string, int>>(requiredWarningLevel, exceptions);
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<int, IDictionary<string, int>> ruleConfiguration)
        {
            var exceptions = ruleConfiguration.Item2;
            var projectFileName = Path.GetFileName(projectFilePath);
            int requiredWarningLevel;
            if (exceptions.ContainsKey(projectFileName))
            {
                requiredWarningLevel = exceptions[projectFileName];
                Console.Out.WriteLine("DEBUG: Project has exceptional warning level {0}: {1}", requiredWarningLevel, projectFileName);
            }
            else
            {
                requiredWarningLevel = ruleConfiguration.Item1;
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