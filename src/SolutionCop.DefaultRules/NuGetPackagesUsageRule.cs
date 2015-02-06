using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class NuGetPackagesUsageRule : StandardProjectRule
    {
        private IEnumerable<string> _exceptionPackageIds;

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
                element.Add(new XElement("Exception", "Rx-Main"));
                return element;
            }
        }

        protected override IEnumerable<string> ParseConfigSectionCustomParameters(XElement xmlRuleConfigs)
        {
            _exceptionPackageIds = xmlRuleConfigs.Descendants("Exception").Select(x => x.Value.Trim());
            yield break;
        }

        protected override IEnumerable<string> ValidateProjectPrimaryChecks(XDocument xmlProject, string projectFilePath)
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
                    if (_exceptionPackageIds.Contains(packageId))
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