using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class StyleCopEnabledRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify that StyleCop is enabled in every project"; }
        }

        public override string Id
        {
            get { return "StyleCopEnabled"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            if (!xmlProject.Descendants(Namespace + "Import").Any(x => x.Attribute("Project") != null && x.Attribute("Project").Value.Contains("StyleCop.MSBuild.Targets")))
            {
                yield return string.Format("StyleCop is missing in project {0}",Path.GetFileName(projectFilePath));
            }
        }
    }
}