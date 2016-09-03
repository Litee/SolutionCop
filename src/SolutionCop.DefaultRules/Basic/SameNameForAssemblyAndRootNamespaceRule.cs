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
    public class SameNameForAssemblyAndRootNamespaceRule : ProjectRule<string[]>
    {
        public override string Id
        {
            get { return "SameNameForAssemblyAndRootNamespace"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                return element;
            }
        }

        protected override string[] ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception");
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
            }

            return xmlRuleConfigs.Elements("Exception").Select(x => x.Value.Trim()).ToArray();
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, string[] exceptions)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            if (exceptions.Contains(projectFileName))
            {
                Console.Out.WriteLine("DEBUG: Skipping warning level check as an exception for project {0}", projectFileName);
            }
            else
            {
                var assemblyName = xmlProject.Descendants(Namespace + "AssemblyName").FirstOrDefault();
                var rootNamespace = xmlProject.Descendants(Namespace + "RootNamespace").First().Value;
                if (assemblyName == null)
                {
                    yield return string.Format("Assembly name is missing in project {0}", Path.GetFileName(projectFilePath));
                }
                else if (assemblyName.Value != rootNamespace)
                {
                    yield return string.Format("Assembly name '{0}' and root namespace '{1}' are different in project {2}", assemblyName.Value, rootNamespace, Path.GetFileName(projectFilePath));
                }
            }
        }
    }
}