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
        private IEnumerable<string> _exceptionProjectNames;
        private bool _allWarningsMustBeTreatedAsErrors;

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
                element.Add(new XElement("Exception", "FakeProject.csproj"));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            _warningsThatMustBeTreatedAsErrors = xmlRuleConfigs.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
            _allWarningsMustBeTreatedAsErrors = !_warningsThatMustBeTreatedAsErrors.Any() && xmlRuleConfigs.Element("AllWarnings") != null;
            _exceptionProjectNames = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
            yield break;
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (_exceptionProjectNames.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping warning level check as an exception for project {0}", projectFileName);
            }
            else
            {
                var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
                foreach (var xmlPropertyGroupsWithCondition in xmlPropertyGroupsWithConditions)
                {
                    var xmlTreatWarningsAsErrors = xmlPropertyGroupsWithCondition.Descendants(Namespace + "TreatWarningsAsErrors").FirstOrDefault();
                    var xmlWarningsAsErrors = xmlPropertyGroupsWithCondition.Descendants(Namespace + "WarningsAsErrors").FirstOrDefault();
                    // Not all warnings are treated as errors within the project
                    if (xmlTreatWarningsAsErrors == null || xmlTreatWarningsAsErrors.Value != "true")
                    {
                        if (_allWarningsMustBeTreatedAsErrors)
                        {
                            yield return string.Format("Not all warnings are treated as an error in project {0}", Path.GetFileName(projectFilePath));
                            yield break;
                        }
                        var warningsTreatedAsErrorsInProject = xmlWarningsAsErrors == null ? new string[0] : xmlWarningsAsErrors.Value.Split(',').Select(x => x.Trim());
                        var warningsThatHasNotBeenTreatedAsErrorsInProject = _warningsThatMustBeTreatedAsErrors.Except(warningsTreatedAsErrorsInProject);
                        if (warningsThatHasNotBeenTreatedAsErrorsInProject.Count() == 1)
                        {
                            yield return string.Format("Warning {0} is not treated as an error in project {1}", warningsThatHasNotBeenTreatedAsErrorsInProject.First(), Path.GetFileName(projectFilePath));
                            yield break;
                        }
                        if (warningsThatHasNotBeenTreatedAsErrorsInProject.Count() > 1)
                        {
                            yield return string.Format("Warnings {0} are not treated as errors in project {1}", string.Join(", ", warningsThatHasNotBeenTreatedAsErrorsInProject), Path.GetFileName(projectFilePath));
                            yield break;
                        }
                    }
                }
            }
        }
    }
}