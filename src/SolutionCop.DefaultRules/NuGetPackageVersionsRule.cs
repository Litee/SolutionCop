using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet;

namespace SolutionCop.DefaultRules
{
    public class NuGetPackageVersionsRule : StandardProjectRule
    {
        private readonly List<XElement> _xmlPackageRules = new List<XElement>();
        private IEnumerable<string> _exceptions = new List<string>();

        public override string DisplayName
        {
            get { return "Verify that NuGet package versions match rules"; }
        }

        public override string Id
        {
            get { return "NuGetPackageVersions"; }
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
            var xmlPackageRules = xmlRuleConfigs.Elements().Where(x => x.Name.LocalName.ToLower() == "package");
            foreach (var xmlPackageRule in xmlPackageRules)
            {
                var packageRuleVersion = xmlPackageRule.Attribute("version").Value.Trim();
                var packageRuleId = xmlPackageRule.Attribute("version").Value.Trim();
                IVersionSpec versionSpec;
                if (!VersionUtility.TryParseVersionSpec(packageRuleVersion, out versionSpec))
                {
                    yield return string.Format("Cannot parse package version rule {0} for package {1} in config {2}", packageRuleVersion, packageRuleId, Id);
                }
                else
                {
                    _xmlPackageRules.Add(xmlPackageRule);
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
                var pathToPackagesConfigFile = Path.Combine(Path.GetDirectoryName(projectFilePath), "packages.config");
                if (File.Exists(pathToPackagesConfigFile))
                {
                    var xmlUsedPackages = XDocument.Load(pathToPackagesConfigFile).Element("packages").Elements("package");
                    foreach (var xmlUsedPackage in xmlUsedPackages)
                    {
                        var packageId = xmlUsedPackage.Attribute("id").Value;
                        var packageVersion = xmlUsedPackage.Attribute("version").Value;
                        var xmlPackageRule = _xmlPackageRules.FirstOrDefault(x => x.Attribute("id").Value == packageId);
                        if (xmlPackageRule == null)
                        {
                            yield return string.Format("Unknown package {0} with version {1} in project {2}", packageId, packageVersion, projectFileName);
                        }
                        else
                        {
                            var packageRuleVersion = xmlPackageRule.Attribute("version").Value.Trim();
                            IVersionSpec versionSpec = VersionUtility.ParseVersionSpec(packageRuleVersion);
                            if (!versionSpec.Satisfies(SemanticVersion.Parse(packageVersion)))
                            {
                                yield return string.Format("Version {0} for package {1} does not match rule {2} in project {3}", packageVersion, packageId, packageRuleVersion, projectFileName);
                            }
                        }
                    }
                }
            }
        }
    }
}