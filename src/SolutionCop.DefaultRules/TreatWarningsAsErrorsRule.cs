using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TreatWarningsAsErrorsRule : ProjectRule
    {
        private string[] _warningsThatMustBeTreatedAsErrors;
        private bool _allWarningsMustBeTreatedAsErrors;
        private readonly IDictionary<string, string[]> _exceptions = new Dictionary<string, string[]>();

        public override string DisplayName
        {
            get { return "Verify warnings treatment as errors"; }
        }

        public override string Id
        {
            get { return "TreatWarningsAsErrors"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("AllWarnings"));
                element.Add(new XComment("You may also define specific warnings"));
                element.Add(new XComment(new XElement("Warning", "0123").ToString()));
                element.Add(new XComment(new XElement("Warning", "0124").ToString()));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectIsAllowedNotToTreatSomeWarningsAsErrors.csproj"), new XElement("Warning", "0125")));
                return element;
            }
        }

        protected override void ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception" && x != "Warning" && x != "AllWarnings").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
            _warningsThatMustBeTreatedAsErrors = xmlRuleConfigs.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            _allWarningsMustBeTreatedAsErrors = !_warningsThatMustBeTreatedAsErrors.Any() && xmlRuleConfigs.Element("AllWarnings") != null;
            // Clear is required for cases when errors are enumerated twice
            _exceptions.Clear();
            foreach (var xmlException in xmlRuleConfigs.Descendants("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
                else
                {
                    var warnings = xmlException.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    _exceptions.Add(xmlProject.Value, warnings.ToArray());
                }
            }
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            IEnumerable<string> warningsThatMustBeTreatedAsErrors;
            bool allWarningsMustBeTreatedAsErrors;
            if (_exceptions.ContainsKey(projectFileName))
            {
                allWarningsMustBeTreatedAsErrors = !_exceptions.Any();
                warningsThatMustBeTreatedAsErrors = _exceptions[projectFileName];
                Console.Out.WriteLine("DEBUG: Project has exceptional warnings {0}: {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), projectFileName);
            }
            else
            {
                allWarningsMustBeTreatedAsErrors = _allWarningsMustBeTreatedAsErrors;
                warningsThatMustBeTreatedAsErrors = _warningsThatMustBeTreatedAsErrors;
                Console.Out.WriteLine("DEBUG: Project has standard warnings {0}: {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), projectFileName);
            }
            var xmlPropertyGlobalGroups = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") == null);
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlTreatWarningsAsErrors = xmlPropertyGroupWithCondition.Descendants(Namespace + "TreatWarningsAsErrors").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "TreatWarningsAsErrors")).FirstOrDefault();
                if (xmlTreatWarningsAsErrors != null)
                {
                    if (xmlTreatWarningsAsErrors.Value == "true")
                    {
                        continue;
                    }
                }
                if (allWarningsMustBeTreatedAsErrors)
                {
                    yield return string.Format("Not all warnings are treated as an error in project {0}", projectFileName);
                }
                else
                {
                    var xmlWarningsAsErrors = xmlPropertyGroupWithCondition.Descendants(Namespace + "WarningsAsErrors").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "WarningsAsErrors")).FirstOrDefault();
                    var warningsTreatedAsErrorsInProject = xmlWarningsAsErrors == null ? new string[0] : xmlWarningsAsErrors.Value.Split(',').Select(x => x.Trim());

                    Console.Out.WriteLine("{0} vs {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), string.Join(", ", warningsTreatedAsErrorsInProject));

                    var warningsNotTreatedAsErrorsInPropertyGroup = warningsThatMustBeTreatedAsErrors.Except(warningsTreatedAsErrorsInProject);
                    foreach (var warningId in warningsNotTreatedAsErrorsInPropertyGroup)
                    {
                        yield return string.Format("Warning {0} is not treated as an error in project {1}", warningId, Path.GetFileName(projectFilePath));
                    }
                }
            }
        }
    }
}