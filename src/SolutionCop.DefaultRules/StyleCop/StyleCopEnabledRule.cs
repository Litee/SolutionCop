using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SolutionCop.Core;

namespace SolutionCop.DefaultRules.StyleCop
{
    [Export(typeof(IProjectRule))]
    public class StyleCopEnabledRule : ProjectRule<string[]>
    {
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
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                return element;
            }
        }

        protected override string[] ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionElements(xmlRuleConfigs, errors, "Exception");
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
            }
            return xmlRuleConfigs.Elements("Exception").Select(x => x.Value.Trim()).ToArray();
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, string[] exceptions)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (exceptions.Contains(projectFileName))
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