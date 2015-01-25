using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TreatWarningsAsErrorsRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify that all warnings are treated as errors"; }
        }

        public override string Id
        {
            get { return "TreatWarningsAsErrors"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(string projectFilePath, XElement xmlRuleConfigs, XDocument xmlProject)
        {
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupsWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlTreatWarningsAsErrors = xmlPropertyGroupsWithCondition.Descendants(Namespace + "TreatWarningsAsErrors").FirstOrDefault();
                if (xmlTreatWarningsAsErrors == null || xmlTreatWarningsAsErrors.Value != "true")
                {
                    return Enumerable.Repeat(string.Format("'Treat warnings as errors' is not enabled in project: {0}", Path.GetFileName(projectFilePath)), 1);
                }
            }
            return Enumerable.Empty<string>();
        }
    }
}