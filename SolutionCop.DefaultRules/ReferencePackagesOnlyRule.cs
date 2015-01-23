using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class ReferencePackagesOnlyRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Should reference binaries only in NuGet packages"; }
        }

        public override string Id
        {
            get { return "ReferencePackagesOnly"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(string projectFilePath, XElement xmlRuleParameters, XDocument xmlProject)
        {
            var xmlHintPaths = xmlProject.Descendants(Namespace + "HintPath").Where(x => !x.Value.Contains(@"\packages\"));
            foreach (var xmlHintPath in xmlHintPaths)
            {
                return Enumerable.Repeat(string.Format("Reference '{0}' is not pointing to NuGet package in project: {1}", xmlHintPath.Value, Path.GetFileName(projectFilePath)), 1);
            }
            return Enumerable.Empty<string>();
        }
    }
}