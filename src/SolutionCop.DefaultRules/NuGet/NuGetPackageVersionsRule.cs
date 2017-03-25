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
                element.Add(new XElement("Package", new XAttribute("id", "first-package-id"), new XAttribute("version", "1.0.0"), new XAttribute("prerelease", "false")));
                element.Add(new XElement("Package", new XAttribute("id", "second-package-id"), new XAttribute("version", "[2.0.3]")));
                element.Add(new XElement("Package", new XAttribute("id", "third-package-id"), new XAttribute("version", "[1.5.0, 2.0.0)")));
                element.Add(new XElement("Package", new XAttribute("id", "fourth-package-id"), new XAttribute("version", "[1.5.0]|[1.6.0]")));
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
                    errors.Add($"Bad configuration for rule {Id}: <Project> element is missing in exceptions list.");
                }
            }

            var exceptions = xmlRuleConfigs.Elements("Exception").Select(x => x.Value.Trim()).ToArray();
            var xmlPackageRules = xmlRuleConfigs.Elements().Where(x => x.Name.LocalName == "Package");
            var packageRules = new List<XElement>();
            foreach (var xmlPackageRule in xmlPackageRules)
            {
                var packageRuleId = xmlPackageRule.Attribute("id").Value.Trim();
                var packageRuleVersions = xmlPackageRule.Attribute("version").Value.Trim();
                IVersionSpec versionSpec;
                if (packageRuleVersions.Split('|').Select(x => x.Trim()).Any(x => !VersionUtility.TryParseVersionSpec(x, out versionSpec)))
                {
                    errors.Add($"Cannot parse package version rule {packageRuleVersions} for package {packageRuleId} in config {Id}");
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
                            yield return $"Unknown package '{packageId}' with version {packageVersion} in project {projectFileName}";
                        }
                        else
                        {
                            var noPrereleaseVersions = ((string)xmlPackageRule.Attribute("prerelease") ?? "true").Trim() == "false";
                            var packageRuleVersions = xmlPackageRule.Attribute("version").Value.Trim();
                            var versionSpecs = packageRuleVersions.Split('|').Select(x => x.Trim()).Select(VersionUtility.ParseVersionSpec);
                            var usedSemanticVersion = SemanticVersion.Parse(packageVersion);
                            if (!versionSpecs.Any(x => x.Satisfies(usedSemanticVersion)))
                            {
                                yield return $"Version {packageVersion} for package '{packageId}' does not match rule {packageRuleVersions} in project {projectFileName}";
                            }
                            else if (noPrereleaseVersions && !string.IsNullOrEmpty(usedSemanticVersion.SpecialVersion))
                            {
                                yield return $"Version {packageVersion} for package '{packageId}' must be stable in project {projectFileName}";
                            }
                        }
                    }
                }
            }
        }
    }
}