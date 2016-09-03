﻿namespace SolutionCop.DefaultRules.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;

    [Export(typeof(IProjectRule))]
    public class SameNuGetPackageVersionsRule : ProjectRule<Tuple<string, string>[]>
    {
        // Hash table would be faster, but List is more readable
        private readonly List<Tuple<string, string, string>> _projectsPackageIdsAndVersions = new List<Tuple<string, string, string>>();

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
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception");
            var exceptions = new List<Tuple<string, string>>();
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
                    exceptions.Add(new Tuple<string, string>(xmlProject == null ? null : xmlProject.Value.Trim(), xmlPackage == null ? null : xmlPackage.Value.Trim()));
                }
            }

            return exceptions.ToArray();
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
                        var differentVersionsOfTheSamePackage = _projectsPackageIdsAndVersions.Where(x => x.Item2 == packageId && x.Item3 != packageVersion).ToArray();
                        if (differentVersionsOfTheSamePackage.Any())
                        {
                            yield return string.Format(
                                "Package {0} uses different versions {1} in projects {2}",
                                packageId,
                                string.Join(" and ", Enumerable.Repeat(packageVersion, 1).Concat(differentVersionsOfTheSamePackage.Select(x => x.Item3))),
                                string.Join(", ", Enumerable.Repeat(projectFileName, 1).Concat(differentVersionsOfTheSamePackage.Select(x => x.Item1)).Take(5))); // Take(5) protects from too many items in message
                        }
                        _projectsPackageIdsAndVersions.Add(new Tuple<string, string, string>(projectFileName, packageId, packageVersion));
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