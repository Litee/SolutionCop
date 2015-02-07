using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TreatWarningsAsErrorsRule : StandardProjectRule
    {
        private IEnumerable<string> _warningsThatMustBeTreatedAsErrors;
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
                var xmlException = new XElement("Exception");
                xmlException.Add(new XComment("As exception you can specify project name or warnings or both (AND logic)"));
                xmlException.Add(new XElement("Project", "PUT PROJECT TO IGNORE HERE (e.g. FakeProject.csproj)"));
                xmlException.Add(new XElement("Warning", "PUT SPECIFIC WARNING YOU WANT TO IGNORE HERE (e.g. 0123)"));
                element.Add(xmlException);
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            _warningsThatMustBeTreatedAsErrors = xmlRuleConfigs.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
            _allWarningsMustBeTreatedAsErrors = !_warningsThatMustBeTreatedAsErrors.Any() && xmlRuleConfigs.Element("AllWarnings") != null;
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

        private bool TreatsSpecificWarningAsAnError(XElement xmlPropertyGroup, IEnumerable<string> warningsThatMustBeTreatedAsErrors)
        {
            var xmlWarningsAsErrors = xmlPropertyGroup.Descendants(Namespace + "WarningsAsErrors").FirstOrDefault();
            var warningsTreatedAsErrorsInProject = xmlWarningsAsErrors == null ? new string[0] : xmlWarningsAsErrors.Value.Split(',').Select(x => x.Trim());
            Console.Out.WriteLine("{0} vs {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), string.Join(", ", warningsTreatedAsErrorsInProject));
            return !warningsThatMustBeTreatedAsErrors.Except(warningsTreatedAsErrorsInProject).Any();
        }

        private bool TreatsAllWarningsAsErrors(XElement xmlPropertyGroup)
        {
            var xmlTreatWarningsAsErrors = xmlPropertyGroup.Descendants(Namespace + "TreatWarningsAsErrors").FirstOrDefault();
            // Not all warnings are treated as errors within the project
            if (xmlTreatWarningsAsErrors != null)
            {
                if (xmlTreatWarningsAsErrors.Value == "true")
                {
                    return true;
                }
            }
            return false;
        }
    }
}