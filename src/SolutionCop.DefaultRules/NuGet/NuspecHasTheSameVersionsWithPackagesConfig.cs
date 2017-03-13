namespace SolutionCop.DefaultRules.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;

    [Export(typeof(IProjectRule))]
    public sealed class NuspecHasTheSameVersionsWithPackagesConfig : IProjectRule
    {
        private const string PackageExceptionTagName = "ExcludePackageId";
        private const string EnabledAttributeName = "enabled";
        private const string NuspecTagName = "Nupspec";
        private const string ProjectExceptionTagName = "ExcludePackagesOfProject";
        private const string NupsecPathTagName = "Path";
        private const string ProjectAttributeName = "projectName";
        private const string PackageAttributeName = "packageId";

        private static readonly StringComparer WordComparer = StringComparer.OrdinalIgnoreCase;

        public string Id => "NuspecHasTheSameVersionsWithPackagesConfig";

        public XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue(EnabledAttributeName, "false");

                var simpleNuspecElement = new XElement(NuspecTagName, new XElement(NupsecPathTagName, "NuspecFiles/MyPackage.nuspec"));
                var folderNuspecElement = new XElement(NuspecTagName, new XElement(NupsecPathTagName, "../../NuspecFiles/*.nuspec"));
                var nuspecForExclusion = new XElement(NuspecTagName, new XElement(NupsecPathTagName, "MyPackage.nuspec"));

                element.Add(simpleNuspecElement);
                element.Add(folderNuspecElement);

                var projectExclusion = new XElement(ProjectExceptionTagName);
                projectExclusion.SetAttributeValue(ProjectAttributeName, "ProjectWithAnotherReferences.csproj");

                var packageExclusion = new XElement(PackageExceptionTagName);
                packageExclusion.SetAttributeValue(PackageAttributeName, "package-id");

                nuspecForExclusion.Add(packageExclusion);
                nuspecForExclusion.Add(projectExclusion);

                element.Add(nuspecForExclusion);

                var globalProjectExclusion = new XElement(ProjectExceptionTagName);
                globalProjectExclusion.SetAttributeValue(ProjectAttributeName, "ProjectWithAnotherReferences.csproj");

                var globalPackageExclusion = new XElement(ProjectExceptionTagName);
                globalPackageExclusion.SetAttributeValue(PackageAttributeName, "package-id");

                element.Add(globalProjectExclusion);
                element.Add(globalPackageExclusion);

                return element;
            }
        }

        public ValidationResult ValidateAllProjects(XElement xmlRuleConfigs, params string[] projectFilePaths)
        {
            var errors = new List<string>();

            ConfigValidation.ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, Id, NuspecTagName, ProjectExceptionTagName, PackageExceptionTagName);

            if (errors.Any())
            {
                return new ValidationResult(Id, true, true, errors.ToArray());
            }

            var config = RuleConfiguration.Parse(xmlRuleConfigs);

            if (!config.IsEnabled)
            {
                return new ValidationResult(Id, false, false, new string[0]);
            }

            var projectAndPackages = GetProjectToPackagesMap(projectFilePaths.Where(File.Exists), config.ExcludedProjects);

            foreach (var nuspecTag in config.Specs)
            {
                var excludedProjects = new HashSet<string>(config.ExcludedProjects.Union(nuspecTag.ExcludedProjects, WordComparer));
                var excludedPackages = new HashSet<string>(config.ExcludedPackages.Union(nuspecTag.ExcludedPackages, WordComparer));

                var packageFiles = projectAndPackages.Where(kv => !excludedProjects.Contains(Path.GetFileName(kv.Key))).Select(kv => kv.Value);

                var packageToVersions = GetPackageToAllowedVersion(packageFiles);

                foreach (var specPathesPattern in nuspecTag.Pathes)
                {
                    var resolvedNuspecPathes = GetNuspecFiles(specPathesPattern);

                    if (ReferenceEquals(resolvedNuspecPathes, null))
                    {
                        errors.Add($"Unable to find nuspec files by using pattern {specPathesPattern}");

                        continue;
                    }

                    foreach (var nuspecFilePath in resolvedNuspecPathes)
                    {
                        var nuspecFile = NuspecFileData.ReadFile(nuspecFilePath);

                        foreach (var dependency in nuspecFile.Dependencies)
                        {
                            var packageId = dependency.PackageId;

                            if (excludedPackages.Contains(packageId))
                            {
                                continue;
                            }

                            if (!packageToVersions.ContainsKey(packageId))
                            {
                                continue;
                            }

                            var allowedVersions = packageToVersions[packageId];

                            var version = dependency.PackageVersion;

                            if (allowedVersions.Contains(version))
                            {
                                continue;
                            }

                            errors.Add($"There is inconsistent between Nuspec file {nuspecFilePath} and solution. Nuspec has dependency '{packageId}' with version '{version}'. Solution has the same packages, but with versions: {string.Join(" ,", allowedVersions)}");
                        }
                    }
                }
            }

            return new ValidationResult(Id, true, false, errors.ToArray());
        }

        private static Dictionary<string, HashSet<string>> GetPackageToAllowedVersion(IEnumerable<PackagesFileData> packagesFileDatas)
        {
            return packagesFileDatas.SelectMany(d => d.Packages)
                .ToLookup(p => p.Name, p => p.Version)
                .ToDictionary(kv => kv.Key, kv => new HashSet<string>(kv, WordComparer));
        }

        private static string[] GetNuspecFiles(string pathToFiles)
        {
            if (string.IsNullOrWhiteSpace(pathToFiles))
            {
                return null;
            }

            if (File.Exists(pathToFiles))
            {
                return new[] { pathToFiles };
            }

            var directoryName = Path.GetDirectoryName(pathToFiles);
            var pattern = Path.GetFileName(pathToFiles);

            if (string.IsNullOrWhiteSpace(directoryName) || !Directory.Exists(directoryName))
            {
                return null;
            }

            var files = Directory.GetFiles(directoryName, pattern, SearchOption.AllDirectories);

            return files.Any() ? files : null;
        }

        private static Dictionary<string, PackagesFileData> GetProjectToPackagesMap(IEnumerable<string> projectFilePathes, HashSet<string> excludedProjects)
        {
            var projectsAndPackages = from projectPath in projectFilePathes
                let fileName = Path.GetFileName(projectPath)
                let projectFolder = Path.GetDirectoryName(projectPath)
                let packagesFilePath = Path.Combine(projectFolder, "packages.config")
                where !excludedProjects.Contains(fileName) && File.Exists(projectPath) && File.Exists(packagesFilePath)
                select new { ProjectPath = projectPath, PackageFile = PackagesFileData.ReadFile(packagesFilePath) };

            return projectsAndPackages.ToDictionary(pp => pp.ProjectPath, pp => pp.PackageFile);
        }

        private static string ParseProjectExclusion(XElement element)
        {
            var packageName = element.Attributes(ProjectAttributeName).FirstOrDefault()?.Value ?? string.Empty;

            return packageName.Trim();
        }

        private static string ParsePackageExclusion(XElement element)
        {
            var packageName = element.Attributes(PackageAttributeName).FirstOrDefault()?.Value ?? string.Empty;

            return packageName.Trim();
        }

        private sealed class RuleConfiguration
        {
            private RuleConfiguration()
            {
                Specs = new List<NuspecTag>();
                ExcludedPackages = new HashSet<string>(WordComparer);
                ExcludedProjects = new HashSet<string>(WordComparer);
            }

            public bool IsEnabled { get; private set; }

            public List<NuspecTag> Specs { get; }

            public HashSet<string> ExcludedProjects { get; }

            public HashSet<string> ExcludedPackages { get; }

            public static RuleConfiguration Parse(XElement ruleConfig)
            {
                var result = new RuleConfiguration();

                var enabledValue = ruleConfig.Attribute(EnabledAttributeName)?.Value;
                result.IsEnabled = string.IsNullOrEmpty(enabledValue) || string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase);

                foreach (var mainElement in ruleConfig.Elements())
                {
                    var name = mainElement.Name.LocalName;

                    if (string.Equals(name, NuspecTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Specs.Add(NuspecTag.Parse(mainElement));
                    }
                    else if (string.Equals(name, ProjectExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ExcludedProjects.Add(ParseProjectExclusion(mainElement));
                    }
                    else if (string.Equals(name, PackageExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ExcludedPackages.Add(ParsePackageExclusion(mainElement));
                    }
                }

                return result;
            }
        }

        private sealed class NuspecTag
        {
            private NuspecTag()
            {
                Pathes = new List<string>();
                ExcludedPackages = new HashSet<string>(WordComparer);
                ExcludedProjects = new HashSet<string>(WordComparer);
            }

            public List<string> Pathes { get; }

            public HashSet<string> ExcludedProjects { get; }

            public HashSet<string> ExcludedPackages { get; }

            public static NuspecTag Parse(XElement element)
            {
                var result = new NuspecTag();

                foreach (var nuspecSubtag in element.Elements())
                {
                    var name = nuspecSubtag.Name.LocalName;

                    if (string.Equals(name, NupsecPathTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Pathes.Add(nuspecSubtag.Value.Trim());
                    }
                    else if (string.Equals(name, ProjectExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ExcludedProjects.Add(ParseProjectExclusion(nuspecSubtag));
                    }
                    else if (string.Equals(name, PackageExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ExcludedPackages.Add(ParsePackageExclusion(nuspecSubtag));
                    }
                }

                return result;
            }
        }
    }
}