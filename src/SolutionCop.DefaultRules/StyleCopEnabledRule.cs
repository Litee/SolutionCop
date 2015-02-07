using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class StyleCopEnabledRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptions = new List<string>();

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
                var xmlException = new XElement("Exception");
                xmlException.Add(new XElement("Project", "PUT PROJECT TO IGNORE HERE (e.g. FakeProject.csproj)"));
                element.Add(xmlException);
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
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
                Console.Out.WriteLine("DEBUG: Skipping project with disabled StyleCop as an exception: {0}", projectFileName);
            }
            else
            {
                var importedProjectPaths = xmlProject.Descendants(Namespace + "Import").Select(x => (string)x.Attribute("Project"));
                if (!importedProjectPaths.Any(x => x.Contains("StyleCop.MSBuild.Targets") || x.Contains("Microsoft.SourceAnalysis.targets")))
                {
                    yield return string.Format("StyleCop is missing in project {0}", projectFileName);
                }
            }
        }
    }
}