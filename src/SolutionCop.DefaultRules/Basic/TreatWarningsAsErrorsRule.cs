namespace SolutionCop.DefaultRules.Basic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Core;

    [Export(typeof(IProjectRule))]
    public class TreatWarningsAsErrorsRule : ProjectRule<Tuple<string[], bool, IDictionary<string, string[]>>>
    {
        public override string Id
        {
            get { return "TreatWarningsAsErrors"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("AllWarnings"));
                element.Add(new XComment("You may also define specific warnings"));
                element.Add(new XComment(new XElement("Warning", "0123").ToString()));
                element.Add(new XComment(new XElement("Warning", "0124").ToString()));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectIsAllowedNotToTreatSomeWarningsAsErrors.csproj"), new XElement("Warning", "0125")));
                return element;
            }
        }

        protected override Tuple<string[], bool, IDictionary<string, string[]>> ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception", "Warning", "AllWarnings");
            var exceptions = new Dictionary<string, string[]>();
            var warningsThatMustBeTreatedAsErrors = xmlRuleConfigs.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var allWarningsMustBeTreatedAsErrors = !warningsThatMustBeTreatedAsErrors.Any() && xmlRuleConfigs.Element("AllWarnings") != null;
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                if (xmlProject == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> element is missing in exceptions list.", Id));
                }
                else
                {
                    var warnings = xmlException.Elements("Warning").Select(x => x.Value.Trim()).Where(x => !string.IsNullOrEmpty(x));
                    exceptions.Add(xmlProject.Value, warnings.ToArray());
                }
            }
            return Tuple.Create<string[], bool, IDictionary<string, string[]>>(warningsThatMustBeTreatedAsErrors, allWarningsMustBeTreatedAsErrors, exceptions);
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, Tuple<string[], bool, IDictionary<string, string[]>> ruleConfiguration)
        {
            var exceptions = ruleConfiguration.Item3;
            var projectFileName = Path.GetFileName(projectFilePath);
            bool allWarningsMustBeTreatedAsErrors;
            var warningsThatMustBeTreatedAsErrors = ruleConfiguration.Item1;
            string[] value;
            if (exceptions.TryGetValue(projectFileName, out value))
            {
                allWarningsMustBeTreatedAsErrors = !exceptions.Any();
                warningsThatMustBeTreatedAsErrors = warningsThatMustBeTreatedAsErrors.Except(value).ToArray();
                Console.Out.WriteLine("DEBUG: Project has exceptional warnings {0}: {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), projectFileName);
            }
            else
            {
                allWarningsMustBeTreatedAsErrors = ruleConfiguration.Item2;
                Console.Out.WriteLine("DEBUG: Project has standard warnings {0}: {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), projectFileName);
            }
            var xmlPropertyGlobalGroups = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") == null).ToArray();
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlTreatWarningsAsErrors = xmlPropertyGroupWithCondition.Descendants(Namespace + "TreatWarningsAsErrors").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "TreatWarningsAsErrors")).FirstOrDefault();
                if (xmlTreatWarningsAsErrors != null)
                {
                    if (xmlTreatWarningsAsErrors.Value == "true")
                    {
                        continue;
                    }
                }
                if (allWarningsMustBeTreatedAsErrors)
                {
                    yield return string.Format("Not all warnings are treated as an error in project {0}", projectFileName);
                }
                else
                {
                    var xmlWarningsAsErrors = xmlPropertyGroupWithCondition.Descendants(Namespace + "WarningsAsErrors").Concat(xmlPropertyGlobalGroups.Descendants(Namespace + "WarningsAsErrors")).FirstOrDefault();
                    var warningsTreatedAsErrorsInProject = xmlWarningsAsErrors == null ? new string[0] : xmlWarningsAsErrors.Value.Split(',').Select(x => x.Trim()).ToArray();

                    Console.Out.WriteLine("{0} vs {1}", string.Join(", ", warningsThatMustBeTreatedAsErrors), string.Join(", ", warningsTreatedAsErrorsInProject));

                    var warningsNotTreatedAsErrorsInPropertyGroup = warningsThatMustBeTreatedAsErrors.Except(warningsTreatedAsErrorsInProject);
                    foreach (var warningId in warningsNotTreatedAsErrorsInPropertyGroup)
                    {
                        yield return string.Format("Warning {0} is not treated as an error in project {1}. Please make sure that setting is active for ALL configurations.", warningId, Path.GetFileName(projectFilePath));
                    }
                }
            }
        }
    }
}