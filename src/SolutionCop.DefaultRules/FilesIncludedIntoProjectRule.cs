using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class FilesIncludedIntoProjectRule : ProjectRule<string[]>
    {
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

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, string[] exceptions)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (exceptions.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping project as an exception: {0}", projectFileName);
            }
            else
            {
                // TODO Support other extensions
                var filePaths = Directory.EnumerateFiles(Path.GetDirectoryName(projectFilePath), "*.cs", SearchOption.AllDirectories);
                foreach (var filePath in filePaths.Where(x => !x.ToLower().Contains(@"\obj\") && !x.ToLower().Contains(@"\bin\")))
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