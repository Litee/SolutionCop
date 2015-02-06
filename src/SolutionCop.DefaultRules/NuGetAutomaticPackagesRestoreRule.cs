using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class NuGetAutomaticPackagesRestoreRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptionProjectNames;

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
                element.Add(new XElement("Exception", "FakeProject.csproj"));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            if (IsEnabled)
            {
                _exceptionProjectNames = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
            }
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
                var importedProjectPaths = xmlProject.Descendants(Namespace + "Import").Select(x => (string)x.Attribute("Project"));
                if (importedProjectPaths.Any(x => x.ToLower().Contains(".nuget\\nuget.targets")))
                {
                    yield return string.Format("Obsolete NuGet restore mode is used in project {0}", Path.GetFileName(projectFilePath));
                }
            }
        }
    }
}