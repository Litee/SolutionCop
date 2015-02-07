using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TreatStyleCopWarningsAsErrorsRule : StandardProjectRule
    {
        private readonly List<string> _exceptions = new List<string>();

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
                var xmlException = new XElement("Exception");
                xmlException.Add(new XComment("As exception you can specify project name"));
                xmlException.Add(new XElement("Project", "PUT PROJECT TO IGNORE HERE (e.g. FakeProject.csproj)"));
                element.Add(xmlException);
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            yield break;
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