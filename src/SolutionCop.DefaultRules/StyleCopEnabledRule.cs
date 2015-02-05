using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class StyleCopEnabledRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptionProjectNames;

        public override string DisplayName
        {
            get { return "Verify that StyleCop is enabled in every project"; }
        }

        public override string Id
        {
            get { return "StyleCopEnabled"; }
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
            _exceptionProjectNames = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
            yield break;
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var importedProjectPaths = xmlProject.Descendants(Namespace + "Import").Select(x => (string)x.Attribute("Project"));
            if (!importedProjectPaths.Any(x => x.Contains("StyleCop.MSBuild.Targets") || x.Contains("Microsoft.SourceAnalysis.targets")))
            {
                if (_exceptionProjectNames.Contains(Path.GetFileName(projectFilePath)))
                {
                    Console.Out.WriteLine("DEBUG: Skipping project with disabled StyleCop as an exception: {0}", Path.GetFileName(projectFilePath));
                }
                else
                {
                    yield return string.Format("StyleCop is missing in project {0}", Path.GetFileName(projectFilePath));
                }
            }
        }
    }
}