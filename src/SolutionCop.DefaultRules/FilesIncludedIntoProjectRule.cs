using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class FilesIncludedIntoProjectRule : StandardProjectRule
    {
        private List<string> _exceptions = new List<string>();

        public override string DisplayName
        {
            get { return "Verify that all packages specified in packages.config are used in *.csproj (exceptions supported)"; }
        }

        public override string Id
        {
            get { return "FilesIncludedIntoProject"; }
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
                    _exceptions = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim()).ToList();
                }
            }
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (_exceptions.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping project as an exception: {0}", projectFileName);
            }
            else
            {
                // TODO Support other extensions
                var filePaths = Directory.EnumerateFiles(Path.GetDirectoryName(projectFilePath), "*.cs", SearchOption.AllDirectories);
                foreach (var filePath in filePaths.Where(x => !x.ToLower().Contains(@"\obj\")))
                {
                    var fileName = Path.GetFileName(filePath);
                    // TODO Make more precise
                    var xmlHintPaths = xmlProject.Descendants(Namespace + "Compile").Where(x =>
                    {
                        return x.Attribute("Include").Value.Contains(fileName);
                    });
                    if (!xmlHintPaths.Any())
                    {
                        yield return string.Format("File is not referenced in project: {0}", fileName);
                    }
                }
            }
        }
    }
}