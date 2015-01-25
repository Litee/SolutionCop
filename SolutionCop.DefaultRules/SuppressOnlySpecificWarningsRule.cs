using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class SuppressOnlySpecificWarningsRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify that only approved warnings are suppressed"; }
        }

        public override string Id
        {
            get { return "SuppressOnlySpecificWarnings"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            var warningsAllowedToSuppress = xmlRuleConfigs.Value.Split(',').Select(x => x.Trim());
            var xmlPropertyGroupsWithConditions = xmlProject.Descendants(Namespace + "PropertyGroup").Where(x => x.Attribute("Condition") != null);
            foreach (var xmlPropertyGroupsWithCondition in xmlPropertyGroupsWithConditions)
            {
                var xmlNoWarn = xmlPropertyGroupsWithCondition.Descendants(Namespace + "NoWarn").FirstOrDefault();
                if (xmlNoWarn != null)
                {
                    var suppressedWarnings = xmlNoWarn.Value.Split(',').Select(x => x.Trim());
                    var warningsNotAllowedToSuppress = suppressedWarnings.Except(warningsAllowedToSuppress);
                    if (warningsNotAllowedToSuppress.Count() == 1)
                    {
                        return Enumerable.Repeat(string.Format("Warning {0} is suppressed in project: {1}", warningsNotAllowedToSuppress.First(), Path.GetFileName(projectFilePath)), 1);
                    }
                    else if (warningsNotAllowedToSuppress.Count() > 1)
                    {
                        return Enumerable.Repeat(string.Format("Warnings {0} are suppressed in project: {1}", string.Join(", ", warningsNotAllowedToSuppress), Path.GetFileName(projectFilePath)), 1);
                    }
                }
            }
            return Enumerable.Empty<string>();
        }
    }
}