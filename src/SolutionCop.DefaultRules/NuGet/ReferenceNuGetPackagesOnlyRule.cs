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
    public class ReferenceNuGetPackagesOnlyRule : ProjectRule<List<Tuple<string, string[]>>>
    {
        public override string Id
        {
            get { return "ReferenceNuGetPackagesOnly"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj"), new XElement("Assembly", "my.dll")));
                return element;
            }
        }

        protected override List<Tuple<string, string[]>> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception");
            var exceptions = new List<Tuple<string, string[]>>();
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");

                // <File> is legacy
                var xmlFiles = xmlException.Elements("File").Concat(xmlException.Elements("Assembly")).ToArray();
                if (xmlProject == null && !xmlFiles.Any())
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> or <Assembly> elements are missing in exceptions list.", Id));
                }
                exceptions.Add(new Tuple<string, string[]>(xmlProject == null ? null : xmlProject.Value.Trim(), xmlFiles.Select(x => x.Value.Trim()).ToArray()));
            }
            return exceptions;
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, List<Tuple<string, string[]>> exceptions)
        {
            var hintPaths = xmlProject.Descendants(Namespace + "HintPath").Where(x => !x.Value.Contains(@"\packages\")).Select(x => x.Value).ToArray();
            var projectFileName = Path.GetFileName(projectFilePath);
            var hintPathsNotMatchingAnyException = new HashSet<string>(hintPaths);
            foreach (var exception in exceptions.Where(x => x.Item1 == null || x.Item1.Contains(projectFileName)))
            {
                var exceptionFiles = exception.Item2;
                if (exception.Item1 == null || exceptionFiles.Any())
                {
                    var hintPathsThatMatchCurrentException = hintPaths.Where(hintPath => exceptionFiles.Any(exceptionFile => hintPath.ToLower().EndsWith(exceptionFile.ToLower()))).ToArray();
                    foreach (var hintPath in hintPathsThatMatchCurrentException)
                    {
                        hintPathsNotMatchingAnyException.Remove(hintPath);
                    }
                    if (hintPathsThatMatchCurrentException.Count() == hintPaths.Count())
                    {
                        break;
                    }
                }
                else
                {
                    // Exception matched the project and there are no file exceptions
                    hintPathsNotMatchingAnyException.Clear();
                    break;
                }
            }
            return hintPathsNotMatchingAnyException.Select(badReference => string.Format("Reference '{0}' is not pointing to NuGet package in project {1}", badReference, projectFileName));
        }
    }
}