using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class SameNuGetPackageVersionsRule : ProjectRule<Tuple<string, string>[]>
    {
        private readonly Dictionary<string, HashSet<string>> _usedIds = new Dictionary<string, HashSet<string>>();

        public override string DisplayName
        {
            get { return "Verify that all project within solution use same version of packages (exceptions supported)"; }
        }

        public override string Id
        {
            get { return "SameNuGetPackageVersions"; }
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

        protected override Tuple<string, string>[] ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
            var _exceptions = new List<Tuple<string, string>>();
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
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
            return _exceptions.ToArray();
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<string, string>[] exceptions)
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
                    if (exceptions.Contains(new Tuple<string, string>(projectFileName, null)) || exceptions.Contains(new Tuple<string, string>(null, packageId)) || exceptions.Contains(new Tuple<string, string>(projectFileName, packageId)))
                    {
                        Console.Out.WriteLine("DEBUG: Skipping package {0} as an exception in project {1}", packageId, projectFileName);
                    }
                    else
                    {
                        if (_usedIds.ContainsKey(packageId))
                        {
                            var otherUsedPackageIds = _usedIds[packageId].Where(x => x != packageVersion);
                            if (otherUsedPackageIds.Any())
                            {
                                yield return string.Format("Package {0} uses different versions {1} and {2}  in project {3}", packageId, otherUsedPackageIds.First(), packageVersion, projectFileName);
                            }
                        }
                        else
                        {
                            _usedIds[packageId] = new HashSet<string>();
                        }
                        _usedIds[packageId].Add(packageVersion);
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