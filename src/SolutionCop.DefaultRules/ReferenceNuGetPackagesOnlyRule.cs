using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class ReferenceNuGetPackagesOnlyRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptions = new List<string>();

        public override string DisplayName
        {
            get { return "Verify that all referenced binaries come from NuGet packages"; }
        }

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