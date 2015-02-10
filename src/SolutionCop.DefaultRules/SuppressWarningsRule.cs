using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class SuppressWarningsRule : ProjectRule<Tuple<string[], IDictionary<string, string[]>>>
    {
        public override string DisplayName
        {
            get { return "Verify suppressed warnings"; }
        }

        public override string Id
        {
            get { return "SuppressWarnings"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Warning", "0123"));
                element.Add(new XElement("Warning", "0124"));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectIsAllowedToSuppressAnExtraWarning.csproj"), new XElement("Warning", "0125")));
                return element;
            }
        }

        protected override Tuple<string[], IDictionary<string, string[]>> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception" && x != "Warning").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
            var warningsAllowedToSuppress = xmlRuleConfigs.Elements("Warning").Select(x => x.Value.Trim()).ToArray();
            var exceptions = new Dictionary<string, string[]>();
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
                else
                {
                    var warnings = xmlException.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    exceptions.Add(xmlProject.Value, warnings.ToArray());
                }
            }
            return Tuple.Create<string[], IDictionary<string, string[]>>(warningsAllowedToSuppress, exceptions);
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<string[], IDictionary<string, string[]>> ruleConfiguration)
        {
            var exceptions = ruleConfiguration.Item2;
            var projectFileName = Path.GetFileName(projectFilePath);
            IEnumerable<string> warningsAllowedToSuppress;
            if (exceptions.ContainsKey(projectFileName))
            {
                warningsAllowedToSuppress = exceptions[projectFileName];
                if (!warningsAllowedToSuppress.Any())
                {
                    Console.Out.WriteLine("DEBUG: Project can suppress any warnings: {0}", projectFileName);
                    yield break;
                }
                Console.Out.WriteLine("DEBUG: Project has exceptional warnings {0}: {1}", string.Join(", ", warningsAllowedToSuppress), projectFileName);
            }
            else
            {
                warningsAllowedToSuppress = ruleConfiguration.Item1;
                Console.Out.WriteLine("DEBUG: Project has standard warnings {0}: {1}", string.Join(", ", warningsAllowedToSuppress), projectFileName);
            }
            var xmlPropertyGlobalGroups = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") == null);
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlNoWarn = xmlPropertyGroupWithCondition.Descendants(Namespace + "NoWarn").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "NoWarn")).FirstOrDefault();
                if (xmlNoWarn != null)
                {
                    var suppressedWarnings = xmlNoWarn.Value.Split(',').Select(x => x.Trim());
                    var warningsNotAllowedToSuppress = suppressedWarnings.Except(warningsAllowedToSuppress);
                    if (warningsNotAllowedToSuppress.Count() == 1)
                    {
                        yield return string.Format("Unapproved warning {0} is suppressed in project {1}", warningsNotAllowedToSuppress.First(), Path.GetFileName(projectFilePath));
                        yield break;
                    }
                    if (warningsNotAllowedToSuppress.Count() > 1)
                    {
                        yield return string.Format("Unapproved warnings {0} are suppressed in project {1}", string.Join(", ", warningsNotAllowedToSuppress), Path.GetFileName(projectFilePath));
                        yield break;
                    }
                }
            }
        }
    }
}