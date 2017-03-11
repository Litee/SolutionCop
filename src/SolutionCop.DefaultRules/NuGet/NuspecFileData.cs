namespace SolutionCop.DefaultRules.NuGet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal sealed class NuspecFileData
    {
        private NuspecFileData()
        {
            Dependencies = new List<NuspecDependencyInfo>();
        }

        public List<NuspecDependencyInfo> Dependencies { get; }

        public static NuspecFileData ReadFile(string packagesFilePath)
        {
            var packagesDocument = XDocument.Load(packagesFilePath);

            var result = new NuspecFileData();

            var dependencies = packagesDocument?.Element("package")?.Element("metadata")?.Element("dependencies")?.Elements("dependency") ?? Enumerable.Empty<XElement>();

            foreach (var xmlUsedPackage in dependencies)
            {
                var packageId = xmlUsedPackage.Attribute("id")?.Value ?? string.Empty;
                var packageVersion = xmlUsedPackage.Attribute("version")?.Value ?? string.Empty;

                result.Dependencies.Add(new NuspecDependencyInfo(packageId, packageVersion));
            }

            return result;
        }
    }
}