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
        private const string EnabledAttributeName = "enabled";
        private const string NuspecTagName = "Nupspec";
        private const string NupsecPathTagName = "Path";

        private static readonly StringComparer WordComparer = StringComparer.OrdinalIgnoreCase;

        public string Id => "NuspecHasTheSameVersionsWithPackagesConfig";

        public XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue(EnabledAttributeName, "false");
                element.Add(new XComment("See also here: https://github.com/Litee/SolutionCop/wiki/NuspecHasTheSameVersionsWithPackagesConfig"));

                var simpleNuspecElement = new XElement(NuspecTagName);
                simpleNuspecElement.Add(new XComment("One simple nuspec file. Relative and absolute pathes are supported."));
                simpleNuspecElement.Add(new XElement(NupsecPathTagName, "NuspecFiles/MyPackage.nuspec"));

                var folderNuspecElement = new XElement(NuspecTagName);
                folderNuspecElement.Add(new XComment("Mask-based nuspec file pattern. SolutionCop will resolve folder '../../NuspecFiles/' and then will try to find all files *.nuspec in this directory and in all subdirectories (see .Net option 'SearchOption.AllDirectories')"));
                folderNuspecElement.Add(new XElement(NupsecPathTagName, "../../NuspecFiles/*.nuspec"));

                var nuspecForExclusion = new XElement(NuspecTagName, new XElement(NupsecPathTagName, "MyPackage.nuspec"));

                element.Add(simpleNuspecElement);
                element.Add(folderNuspecElement);

                var projectExclusion = PackageProjectException.Generate("ProjectWithAnotherReferences.csproj", null);

                var packageExclusion = PackageProjectException.Generate(null, "package-id");

                nuspecForExclusion.Add(new XComment("Target package will be ignored for current nuspec pattern only"));
                nuspecForExclusion.Add(packageExclusion);

                nuspecForExclusion.Add(new XComment("This project will be ignored for current nuspec pattern only. All these declarations are the same: 'myProject.csproj', 'myProject', MYPROJECT"));
                nuspecForExclusion.Add(projectExclusion);

                element.Add(nuspecForExclusion);

                var globalProjectExclusion = PackageProjectException.Generate("ProjectWithAnotherReferences.csproj", null);
                var globalPackageExclusion = PackageProjectException.Generate(null, "package-id");

                element.Add(new XComment("Target package will be ignored for all nuspec files"));
                element.Add(globalPackageExclusion);

                element.Add(new XComment("This project will be ignored by all nuspec files. All these declarations are the same: 'myProject.csproj', 'myProject', MYPROJECT"));
                element.Add(globalProjectExclusion);

                var unionedGlobalExclusion = PackageProjectException.Generate("ProjectWithAnotherReferences.csproj", "package-id");

                element.Add(new XComment($"To exclude project and package (e.g. exclude with AND mark) union both exclusions in the same {PackageProjectException.ExceptionTagName} tag"));
                element.Add(unionedGlobalExclusion);

                return element;
            }
        }

        public ValidationResult ValidateAllProjects(XElement xmlRuleConfigs, params string[] projectFilePaths)
        {
            var errors = new List<string>();

            ConfigValidation.ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, Id, NuspecTagName, PackageProjectException.ExceptionTagName);

            if (errors.Any())
            {
                return new ValidationResult(Id, true, true, errors.ToArray());
            }

            var config = RuleConfiguration.Parse(xmlRuleConfigs);

            if (!config.IsEnabled)
            {
                return new ValidationResult(Id, false, false, new string[0]);
            }

            var projectAndPackages = GetProjectToPackagesMap(projectFilePaths.Where(File.Exists));

            foreach (var nuspecTag in config.Specs)
            {
                var exceptions = config.Exceptions.Concat(nuspecTag.Exceptions).ToArray();

                var packageFiles = projectAndPackages
                    .Select(kv => kv.Value.WithoutExceptions(exceptions, kv.Key))
                    .ToList();

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

        private static Dictionary<string, PackagesFileData> GetProjectToPackagesMap(IEnumerable<string> projectFilePathes)
        {
            var projectsAndPackages = from projectPath in projectFilePathes
                                      let fileName = Path.GetFileName(projectPath)
                                      let projectFolder = Path.GetDirectoryName(projectPath)
                                      let packagesFilePath = Path.Combine(projectFolder, "packages.config")
                                      where File.Exists(projectPath) && File.Exists(packagesFilePath)
                                      select new { ProjectPath = projectPath, PackageFile = PackagesFileData.ReadFile(packagesFilePath) };

            return projectsAndPackages.ToDictionary(pp => pp.ProjectPath, pp => pp.PackageFile);
        }

        private sealed class RuleConfiguration
        {
            private RuleConfiguration()
            {
                Specs = new List<NuspecTag>();
                Exceptions = new HashSet<PackageProjectException>();
            }

            public bool IsEnabled { get; private set; }

            public List<NuspecTag> Specs { get; }

            public HashSet<PackageProjectException> Exceptions { get; }

            public static RuleConfiguration Parse(XElement ruleConfig)
            {
                var result = new RuleConfiguration();

                var enabledValue = ruleConfig.Attribute(EnabledAttributeName)?.Value;
                result.IsEnabled = string.Equals(enabledValue, "true", StringComparison.OrdinalIgnoreCase);

                foreach (var mainElement in ruleConfig.Elements())
                {
                    var name = mainElement.Name.LocalName;

                    if (string.Equals(name, NuspecTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Specs.Add(NuspecTag.Parse(mainElement));
                    }
                    else if (string.Equals(name, PackageProjectException.ExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Exceptions.Add(PackageProjectException.Parse(mainElement));
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
                Exceptions = new HashSet<PackageProjectException>();
            }

            public List<string> Pathes { get; }

            public HashSet<PackageProjectException> Exceptions { get; }

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
                    else if (string.Equals(name, PackageProjectException.ExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Exceptions.Add(PackageProjectException.Parse(nuspecSubtag));
                    }
                }

                return result;
            }
        }
    }
}