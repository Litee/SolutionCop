using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SolutionCop.Core;

namespace SolutionCop.DefaultRules.NuGet
{
    [Export(typeof(IProjectRule))]
    public class ReferenceNuGetPackagesOnlyRule : ProjectRule<string[]>
    {
        public override string Id
        {
            get { return "ReferenceNuGetPackagesOnly"; }
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
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
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

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, string[] _exceptions)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (_exceptions.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping project with disabled StyleCop as an exception: {0}", Path.GetFileName(projectFilePath));
            }
            else
            {
                var xmlHintPaths = xmlProject.Descendants(Namespace + "HintPath").Where(x => !x.Value.Contains(@"\packages\"));
                foreach (var xmlHintPath in xmlHintPaths)
                {
                    yield return string.Format("Reference '{0}' is not pointing to NuGet package in project {1}", xmlHintPath.Value, projectFileName);
                }
            }
        }
    }
}