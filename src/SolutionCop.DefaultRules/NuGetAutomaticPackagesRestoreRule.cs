using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class NuGetAutomaticPackagesRestoreRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptions = new List<string>();

        public override string DisplayName
        {
            get { return "Verify that correct automatic packages restore mode is used (see https://docs.nuget.org/Consume/Package-Restore/Migrating-to-Automatic-Package-Restore)"; }
        }

        public override string Id
        {
            get { return "NuGetAutomaticPackagesRestore"; }
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
                yield return string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements));
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
                    _exceptions = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
                }
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (_exceptions.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping warning level check as an exception for project {0}", projectFileName);
            }
            else
            {
                var importedProjectPaths = xmlProject.Descendants(Namespace + "Import").Select(x => (string)x.Attribute("Project"));
                if (importedProjectPaths.Any(x => x.ToLower().Contains(".nuget\\nuget.targets")))
                {
                    yield return string.Format("Obsolete NuGet restore mode is used in project {0}", Path.GetFileName(projectFilePath));
                }
            }
        }
    }
}