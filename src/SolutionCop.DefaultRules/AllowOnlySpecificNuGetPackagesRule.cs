using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.DefaultRules
{
    public class AllowOnlySpecificNuGetPackagesRule : StandardProjectRule
    {
        public override string DisplayName
        {
            get { return "Verify that only packages from white list are used"; }
        }

        public override string Id
        {
            get { return "AllowOnlySpecificNuGetPackages"; }
        }

        protected override IEnumerable<string> ValidateProjectWithEnabledRule(XDocument xmlProject, string projectFilePath, XElement xmlRuleConfigs)
        {
            var allowedPackages = xmlRuleConfigs.Elements("Package").Select(x => x.Value.Trim());
            var pathToPackagesConfigFile = Path.Combine(Path.GetDirectoryName(projectFilePath), "packages.config");
            if (File.Exists(pathToPackagesConfigFile))
            {
                var packageIds = XDocument.Load(pathToPackagesConfigFile).Element("packages").Elements("package").Select(x => x.Attribute("id").Value);
                foreach (var packageId in packageIds)
                {
                    if (!allowedPackages.Contains(packageId))
                    {
                        yield return string.Format("Unapproved package {0} is used in project {1}", packageId, Path.GetFileName(projectFilePath));
                    }
                }
            }
        }
    }
}