using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class TargetFrameworkVersionRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify target .NET framework version"; }
        }

        public override string Id
        {
            get { return "TargetFrameworkVersion"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(string projectFilePath, XElement xmlRuleConfigs, XDocument xmlProject)
        {
            var targetFrameworkVersion = xmlRuleConfigs.Value;
            var invalidFrameworkVersions = xmlProject.Descendants(Namespace + "TargetFrameworkVersion").Select(x => x.Value.Substring(1)).Where(x => x != targetFrameworkVersion);
            if (invalidFrameworkVersions.Any())
            {
                return Enumerable.Repeat(string.Format("Invalid target .NET framework version '{0}' in project: {1}", invalidFrameworkVersions.First(), Path.GetFileName(projectFilePath)), 1);
            }
            return Enumerable.Empty<string>();
        }
    }
}
