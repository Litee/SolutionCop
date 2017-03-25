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
    public class NuGetMandatoryPackageRule : ProjectRule<Tuple<List<XElement>, string[]>>
    {
        public override string Id
        {
            get { return "NuGetMandatoryPackage"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XComment("This package must be referenced in all projects"));
                element.Add(new XElement("Package", new XAttribute("id", "mandatory-package-id")));
                element.Add(new XComment("SomeProject.csproj does not have to reference mandatory packages"));
                element.Add(new XElement("Exception", new XElement("Project", "SomeProject.csproj")));
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

            var exceptionProjects = xmlRuleConfigs.Elements("Exception").Select(x => x.Value.Trim()).ToArray();
            var xmlPackageRules = xmlRuleConfigs.Elements().Where(x => x.Name.LocalName == "Package");
            var packageRules = new List<XElement>();
            foreach (var xmlPackageRule in xmlPackageRules)
            {
                packageRules.Add(xmlPackageRule);
            }

            return Tuple.Create(packageRules, exceptionProjects);
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
                    foreach (var xmlPackageRule in xmlPackageRules)
                    {
                        var mandatoryPackageId = xmlPackageRule.Attribute("id").Value;
                        var matchingPackageInProject = xmlUsedPackages.FirstOrDefault(x => x.Attribute("id").Value == mandatoryPackageId);
                        if (matchingPackageInProject == null)
                        { // mandatory package was not found
                            yield return $"Missing mandatory package '{mandatoryPackageId}' in project {projectFileName}";
                        }
                    }
                }
            }
        }
    }
}