using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TreatStyleCopWarningsAsErrorsRule : StandardProjectRule
    {
        private List<string> _exceptions = new List<string>();

        public override string DisplayName
        {
            get { return "Verify that StyleCop warnings are treated as errors"; }
        }

        public override string Id
        {
            get { return "TreatStyleCopWarningsAsErrors"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception").ToArray();
            if (unknownElements.Any())
            {
                yield return string.Format("Bad configuration for rule {0}: Unknown elements {1} in configuration.", Id, string.Join(",", unknownElements));
                yield break;
            }
            foreach (var xmlException in xmlRuleConfigs.Descendants("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    yield return string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id);
                }
                else
                {
                    _exceptions = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim()).ToList();
                }
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (_exceptions.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping project with disabled StyleCop warnings as an exception: {0}", projectFileName);
            }
            else
            {
                var xmlPropertyGlobalGroups = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") == null);
                var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
                foreach (var xmlPropertyGroupWithCondition in xmlPropertyGroupsWithConditions)
                {
                    var xmlTreatWarningsAsErrors = xmlPropertyGroupWithCondition.Descendants(Namespace + "StyleCopTreatErrorsAsWarnings").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "StyleCopTreatErrorsAsWarnings")).FirstOrDefault();
                    if (xmlTreatWarningsAsErrors != null)
                    {
                        if (xmlTreatWarningsAsErrors.Value == "true")
                        {
                            continue;
                        }
                    }
                    yield return string.Format("StyleCop warnings are not treated as errors in project {0}", projectFileName);
                }
            }
        }
    }
}