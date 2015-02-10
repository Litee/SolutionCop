using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class NuGetPackagesUsageRule : ProjectRule
    {
        private readonly List<Tuple<string, string>> _exceptions = new List<Tuple<string, string>>();

        public override string DisplayName
        {
            get { return "Verify that all packages specified in packages.config are used in *.csproj (exceptions supported)"; }
        }

        public override string Id
        {
            get { return "NuGetPackagesUsage"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Exception", new XElement("Project", "ProjectIsAllowedNotToReferenceSpecificPackage.csproj"), new XElement("Package", "package-id")));
                element.Add(new XElement("Exception", new XElement("Package", "second-package-id")));
                element.Add(new XElement("Exception", new XElement("Package", "third-package-id")));
                return element;
            }
        }

        protected override void ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
            foreach (var xmlException in xmlRuleConfigs.Descendants("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                var xmlPackage = xmlException.Element("Package");
                if (xmlProject == null && xmlPackage == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> or <Package> elements are missing in exceptions list.", Id));
                }
                else
                {
                    _exceptions.Add(new Tuple<string, string>(xmlProject == null ? null : xmlProject.Value.Trim(), xmlPackage == null ? null : xmlPackage.Value.Trim()));
                }
            }
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath)
        {
            var pathToPackagesConfigFile = Path.Combine(Path.GetDirectoryName(projectFilePath), "packages.config");
            var projectFileName = Path.GetFileName(projectFilePath);
            if (File.Exists(pathToPackagesConfigFile))
            {
                var xmlUsedPackages = XDocument.Load(pathToPackagesConfigFile).Element("packages").Elements("package");
                foreach (var xmlUsedPackage in xmlUsedPackages)
                {
                    var packageId = xmlUsedPackage.Attribute("id").Value;
                    var packageVersion = xmlUsedPackage.Attribute("version").Value;
                    // TODO Simplify
                    if (_exceptions.Contains(new Tuple<string, string>(projectFileName, null)) || _exceptions.Contains(new Tuple<string, string>(null, packageId)) || _exceptions.Contains(new Tuple<string, string>(projectFileName, packageId)))
                    {
                        Console.Out.WriteLine("DEBUG: Skipping package {0} as an exception in project {1}", packageId, projectFileName);
                    }
                    else
                    {
                        var hintPathSubstring = "\\packages\\" + packageId + "." + packageVersion + "\\";
                        var xmlHintPaths = xmlProject.Descendants(Namespace + "HintPath").Where(x => x.Value.Contains(hintPathSubstring));
                        if (!xmlHintPaths.Any())
                        {
                            yield return string.Format("Package {0} with version {1} is declared in projects.config, but not referenced in project {2}", packageId, packageVersion, projectFileName);
                        }
                    }
                }
            }
            else
            {
                Console.Out.WriteLine("DEBUG: Skipping project without packages.config: {0}", projectFileName);
            }
        }
    }
}