using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class ReferenceNuGetPackagesOnlyRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify that all referenced binaries come from NuGet packages"; }
        }

        public override string Id
        {
            get { return "ReferenceNuGetPackagesOnly"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            var xmlHintPaths = xmlProject.Descendants(Namespace + "HintPath").Where(x => !x.Value.Contains(@"\packages\"));
            foreach (var xmlHintPath in xmlHintPaths)
            {
                yield return string.Format("Reference '{0}' is not pointing to NuGet package in project {1}", xmlHintPath.Value, Path.GetFileName(projectFilePath));
            }
        }
    }
}