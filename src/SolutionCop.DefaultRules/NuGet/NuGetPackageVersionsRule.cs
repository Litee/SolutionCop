namespace SolutionCop.DefaultRules.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;
    using global::NuGet;

    [Export(typeof(IProjectRule))]
    public class NuGetPackageVersionsRule : ProjectRule<Tuple<List<XElement>, string[]>>
    {
        public override string Id
        {
            get { return "NuGetPackageVersions"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Package", new XAttribute("id", "first-package-id"), new XAttribute("version", "1.0.0")));
                element.Add(new XElement("Package", new XAttribute("id", "second-package-id"), new XAttribute("version", "[2.0.3]")));
                element.Add(new XElement("Package", new XAttribute("id", "third-package-id"), new XAttribute("version", "[1.5.0, 2.0.0)"), new XAttribute("prerelease", "false")));
                return element;
            }
        }

        protected override Tuple<List<XElement>, string[]> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception", "Package");
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
            }

            var exceptions = xmlRuleConfigs.Elements("Exception").Select(x => x.Value.Trim()).ToArray();
            var xmlPackageRules = xmlRuleConfigs.Elements().Where(x => x.Name.LocalName == "Package");
            var packageRules = new List<XElement>();
            foreach (var xmlPackageRule in xmlPackageRules)
            {
                var packageRuleId = xmlPackageRule.Attribute("id").Value.Trim();
                var packageRuleVersion = xmlPackageRule.Attribute("version").Value.Trim();
                IVersionSpec versionSpec;
                if (!VersionUtility.TryParseVersionSpec(packageRuleVersion, out versionSpec))
                {
                    errors.Add(string.Format("Cannot parse package version rule {0} for package {1} in config {2}", packageRuleVersion, packageRuleId, Id));
                }
                else
                {
                    packageRules.Add(xmlPackageRule);
                }
            }

            return Tuple.Create(packageRules, exceptions);
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<List<XElement>, string[]> ruleConfiguration)
        {
            var xmlPackageRules = ruleConfiguration.Item1;
            var exceptions = ruleConfiguration.Item2;
            var projectFileName = Path.GetFileName(projectFilePath);
            if (exceptions.Contains(projectFileName))
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
                        var xmlPackageRule = xmlPackageRules.FirstOrDefault(x => x.Attribute("id").Value == packageId);
                        if (xmlPackageRule == null)
                        {
                            yield return string.Format("Unknown package '{0}' with version {1} in project {2}", packageId, packageVersion, projectFileName);
                        }
                        else
                        {
                            var packageRuleVersion = xmlPackageRule.Attribute("version").Value.Trim();
                            var noPrereleaseVersions = ((string)xmlPackageRule.Attribute("prerelease") ?? "true").Trim() == "false";
                            IVersionSpec versionSpec = VersionUtility.ParseVersionSpec(packageRuleVersion);
                            var usedSemanticVersion = SemanticVersion.Parse(packageVersion);
                            if (!versionSpec.Satisfies(usedSemanticVersion))
                            {
                                yield return string.Format("Version {0} for package '{1}' does not match rule {2} in project {3}", packageVersion, packageId, packageRuleVersion, projectFileName);
                            }
                            else if (noPrereleaseVersions && !string.IsNullOrEmpty(usedSemanticVersion.SpecialVersion))
                            {
                                yield return string.Format("Version {0} for package '{1}' must be stable in project {2}", packageVersion, packageId, projectFileName);
                            }
                        }
                    }
                }
            }
        }
    }
}