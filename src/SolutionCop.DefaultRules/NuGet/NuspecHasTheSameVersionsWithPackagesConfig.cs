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
        private const string PackageAttributeName = "packageName";

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

            ConfigValidation.ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, Id, NuspecTagName, ProjectExceptionTagName);

            if (errors.Any())
            {
                return new ValidationResult(Id, true, true, errors.ToArray());
            }

            var config = RuleConfiguration.Parse(xmlRuleConfigs);

            if (!config.IsEnabled)
            {
                return new ValidationResult(Id, false, false, new string[0]);
            }

            return new ValidationResult(Id, true, false, new string[0]);
        }

        private sealed class RuleConfiguration
        {
            private RuleConfiguration()
            {
                Specs = new List<NuspecTag>();
                ExcludedPackages = new List<PackageExclusion>();
                ExcludedProjects = new List<ProjectExclusion>();
            }

            public bool IsEnabled { get; private set; }

            public List<NuspecTag> Specs { get; }

            public List<ProjectExclusion> ExcludedProjects { get; }

            public List<PackageExclusion> ExcludedPackages { get; }

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
                        result.ExcludedProjects.Add(ProjectExclusion.Parse(mainElement));
                    }
                    else if (string.Equals(name, PackageExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ExcludedPackages.Add(PackageExclusion.Parse(mainElement));
                    }
                }

                return result;
            }
        }

        private sealed class PackageExclusion
        {
            private PackageExclusion(string packageName)
            {
                PackageName = packageName;
            }

            public string PackageName { get; }

            public static PackageExclusion Parse(XElement element)
            {
                var packageName = element.Attributes(PackageAttributeName).FirstOrDefault()?.Value ?? string.Empty;

                return new PackageExclusion(packageName);
            }
        }

        private sealed class ProjectExclusion
        {
            private ProjectExclusion(string projectName)
            {
                ProjectName = projectName;
            }

            public string ProjectName { get; }

            public static ProjectExclusion Parse(XElement element)
            {
                var packageName = element.Attributes(ProjectAttributeName).FirstOrDefault()?.Value ?? string.Empty;

                return new ProjectExclusion(packageName);
            }
        }

        private sealed class NuspecTag
        {
            private NuspecTag()
            {
                Pathes = new List<string>();
                ExcludedPackages = new List<PackageExclusion>();
                ExcludedProjects = new List<ProjectExclusion>();
            }

            public List<string> Pathes { get; }

            public List<ProjectExclusion> ExcludedProjects { get; }

            public List<PackageExclusion> ExcludedPackages { get; }

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
                        result.ExcludedProjects.Add(ProjectExclusion.Parse(nuspecSubtag));
                    }
                    else if (string.Equals(name, PackageExceptionTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        result.ExcludedPackages.Add(PackageExclusion.Parse(nuspecSubtag));
                    }
                }

                return result;
            }
        }
    }
}