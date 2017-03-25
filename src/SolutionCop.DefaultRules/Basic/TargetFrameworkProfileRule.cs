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
    public class TargetFrameworkProfileRule : ProjectRule<Tuple<string[], IDictionary<string, string[]>>>
    {
        public override string Id
        {
            get { return "TargetFrameworkProfile"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Profile", "Client"));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectCanHaveTargetVersion3_5.csproj"), new XElement("Profile", string.Empty)));
                return element;
            }
        }

        protected override Tuple<string[], IDictionary<string, string[]>> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception", "Profile");
            var targetFrameworkProfiles = xmlRuleConfigs.Elements("Profile").Select(x => x.Value.Trim()).ToArray();
            if (!targetFrameworkProfiles.Any())
            {
                errors.Add($"No target profile specified for rule {Id}");
            }
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
                    var profiles = xmlException.Elements("Profile").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    exceptions.Add(xmlProject.Value, profiles.ToArray());
                }
            }
            return Tuple.Create<string[], IDictionary<string, string[]>>(targetFrameworkProfiles, exceptions);
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<string[], IDictionary<string, string[]>> ruleConfiguration)
        {
            var targetFrameworkProfiles = ruleConfiguration.Item1;
            var exceptions = ruleConfiguration.Item2;
            var projectFileName = Path.GetFileName(projectFilePath);
            string[] value;
            if (exceptions.TryGetValue(projectFileName, out value))
            {
                if (value == null || !value.Any())
                {
                    Console.Out.WriteLine("DEBUG: Project can target any framework profile: {0}", projectFileName);
                    yield break;
                }
                else
                {
                    targetFrameworkProfiles = targetFrameworkProfiles.Concat(value).ToArray();
                }
            }
            var xmlTargetFrameworkProfiles = xmlProject.Descendants(Namespace + "TargetFrameworkProfile").ToArray();
            var invalidFrameworkProfiles = xmlTargetFrameworkProfiles.Select(x => x.Value.Trim()).Where(x => targetFrameworkProfiles.All(y => y != x)).ToArray();
            if (invalidFrameworkProfiles.Any())
            {
                yield return $"Invalid target .NET framework profile '{invalidFrameworkProfiles.First()}' in project {Path.GetFileName(projectFilePath)}";
            }
            else if (targetFrameworkProfiles.Any(x => x != string.Empty) && !xmlTargetFrameworkProfiles.Any())
            {
                yield return $"Invalid target .NET framework profile '' in project {Path.GetFileName(projectFilePath)}";
            }
        }
    }
}