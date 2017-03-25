namespace SolutionCop.DefaultRules.Basic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;

    [Export(typeof(IProjectRule))]
    public class SuppressWarningsRule : ProjectRule<Tuple<string[], IDictionary<string, string[]>>>
    {
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
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception", "Warning");
            var warningsAllowedToSuppress = xmlRuleConfigs.Elements("Warning").Select(x => x.Value.Trim()).ToArray();
            var exceptions = new Dictionary<string, string[]>();
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add($"Bad configuration for rule {Id}: <Project> element is missing in exceptions list.");
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
            string[] warningsAllowedToSuppress;
            if (exceptions.TryGetValue(projectFileName, out warningsAllowedToSuppress))
            {
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
            var xmlPropertyGlobalGroups = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") == null).ToArray();
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlNoWarn = xmlPropertyGroupWithCondition.Descendants(Namespace + "NoWarn").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "NoWarn")).FirstOrDefault();
                if (xmlNoWarn != null)
                {
                    var suppressedWarnings = xmlNoWarn.Value.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    if (suppressedWarnings.Any())
                    {
                        var warningsNotAllowedToSuppress = suppressedWarnings.Except(warningsAllowedToSuppress).ToArray();
                        if (warningsNotAllowedToSuppress.Length == 1)
                        {
                            yield return $"Unapproved warning {warningsNotAllowedToSuppress.First()} is suppressed in project {Path.GetFileName(projectFilePath)}. Please make sure that setting is active for ALL configurations.";
                            yield break;
                        }
                        if (warningsNotAllowedToSuppress.Length > 1)
                        {
                            yield return $"Unapproved warnings {string.Join(", ", warningsNotAllowedToSuppress)} are suppressed in project {Path.GetFileName(projectFilePath)}. Please make sure that setting is active for ALL configurations.";
                            yield break;
                        }
                    }
                }
            }
        }
    }
}