using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class SuppressWarningsRule : StandardProjectRule
    {
        private readonly IDictionary<string, string[]> _exceptions = new Dictionary<string, string[]>();
        private IEnumerable<string> _warningsAllowedToSuppress;

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
                var xmlException = new XElement("Exception");
                xmlException.Add(new XElement("Project", "PUT PROJECT TO IGNORE HERE (e.g. FakeProject.csproj)"));
                element.Add(xmlException);
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            _warningsAllowedToSuppress = xmlRuleConfigs.Descendants("Warning").Select(x => x.Value.Trim());
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
                    var warnings = xmlException.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    _exceptions.Add(xmlProject.Value, warnings.ToArray());
                }
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            IEnumerable<string> warningsAllowedToSuppress;
            if (_exceptions.ContainsKey(projectFileName))
            {
                warningsAllowedToSuppress = _exceptions[projectFileName];
                if (!warningsAllowedToSuppress.Any())
                {
                    Console.Out.WriteLine("DEBUG: Project can suppress any warnings: {0}", projectFileName);
                    yield break;
                }
                Console.Out.WriteLine("DEBUG: Project has exceptional warnings {0}: {1}", string.Join(", ", warningsAllowedToSuppress), projectFileName);
            }
            else
            {
                warningsAllowedToSuppress = _warningsAllowedToSuppress;
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