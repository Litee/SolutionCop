using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class StyleCopEnabledRule : StandardProjectRule
    {
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

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            var importedProjectPaths = xmlProject.Descendants(Namespace + "Import").Select(x => (string)x.Attribute("Project"));
            if (!importedProjectPaths.Any(x => x.Contains("StyleCop.MSBuild.Targets") || x.Contains("Microsoft.SourceAnalysis.targets")))
            {
                var exceptionIds = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
                if (exceptionIds.Contains(Path.GetFileName(projectFilePath)))
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