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
    public class TargetFrameworkVersionRule : ProjectRule<Tuple<string[], IDictionary<string, string[]>>>
    {
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

        protected override Tuple<string[], IDictionary<string, string[]>> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception", "FrameworkVersion");
            var targetFrameworkVersions = xmlRuleConfigs.Elements("FrameworkVersion").Select(x => x.Value.Trim()).ToArray();
            if (!targetFrameworkVersions.Any())
            {
                errors.Add(string.Format("No target version specified for rule {0}", Id));
            }
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
                    var warnings = xmlException.Elements("FrameworkVersion").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    exceptions.Add(xmlProject.Value, warnings.ToArray());
                }
            }
            return Tuple.Create<string[], IDictionary<string, string[]>>(targetFrameworkVersions, exceptions);
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<string[], IDictionary<string, string[]>> ruleConfiguration)
        {
            var targetFrameworkVersions = ruleConfiguration.Item1;
            var exceptions = ruleConfiguration.Item2;
            var projectFileName = Path.GetFileName(projectFilePath);
            string[] value;
            if (exceptions.TryGetValue(projectFileName, out value))
            {
                if (value == null || !value.Any())
                {
                    Console.Out.WriteLine("DEBUG: Project can target any framework version: {0}", projectFileName);
                    yield break;
                }
                else
                {
                    targetFrameworkVersions = targetFrameworkVersions.Concat(value).ToArray();
                }
            }
            var invalidFrameworkVersions = xmlProject.Descendants(Namespace + "TargetFrameworkVersion").Select(x => x.Value.Substring(1)).Where(x => targetFrameworkVersions.All(y => y != x)).ToArray();
            if (invalidFrameworkVersions.Any())
            {
                yield return string.Format("Invalid target .NET framework version '{0}' in project {1}", invalidFrameworkVersions.First(), Path.GetFileName(projectFilePath));
            }
        }
    }
}
