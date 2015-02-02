using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NuGet;

namespace SolutionCop.DefaultRules
{
    public class VerifyNuGetPackageVersionsRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify that NuGet package versions match rules"; }
        }

        public override string Id
        {
            get { return "VerifyNuGetPackageVersionsRule"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            var xmlPackageRules = xmlRuleConfigs.Elements().Where(x => x.Name.LocalName.ToLower() == "package");
            var pathToPackagesConfigFile = Path.Combine(Path.GetDirectoryName(projectFilePath), "packages.config");
            if (File.Exists(pathToPackagesConfigFile))
            {
                var xmlUsedPackages = XDocument.Load(pathToPackagesConfigFile).Element("packages").Elements("package");
                foreach (var xmlUsedPackage in xmlUsedPackages)
                {
                    var packageId = xmlUsedPackage.Attribute("id").Value;
                    var packageVersion = xmlUsedPackage.Attribute("version").Value;
                    var xmlPackageRule = xmlPackageRules.FirstOrDefault(x => x.Attribute("id").Value == packageId);
                    if (xmlPackageRule == null)
                    {
                        yield return string.Format("Unknown package {0} with version {1} in project {2}", packageId, packageVersion, Path.GetFileName(projectFilePath));
                    }
                    else
                    {
                        var packageVersionInRule = xmlPackageRule.Attribute("version").Value.Trim();
                        IVersionSpec versionSpec;
                        if (VersionUtility.TryParseVersionSpec(packageVersionInRule, out versionSpec))
                        {
                            if (!versionSpec.Satisfies(SemanticVersion.Parse(packageVersion)))
                            {
                                yield return string.Format("Version {0} for package {1} does not match rule {2} in project {3}", packageVersion, packageId, packageVersionInRule, Path.GetFileName(projectFilePath));
                            }
                        }
                        else
                        {
                            yield return string.Format("Cannot parse package version rule {0} is used in project {1}", packageVersionInRule, Path.GetFileName(projectFilePath));
                        }
                    }
                }
            }
        }
    }
}