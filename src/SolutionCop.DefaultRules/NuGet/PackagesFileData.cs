namespace SolutionCop.DefaultRules.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal sealed class PackagesFileData
    {
        private PackagesFileData()
        {
            Packages = new List<PackageInfo>();
        }

        public List<PackageInfo> Packages { get; }

        public static PackagesFileData ReadFile(string packagesFilePath)
        {
            var packagesDocument = XDocument.Load(packagesFilePath);

            var result = new PackagesFileData();

            var xmlUsedPackages = packagesDocument.Element("packages").Elements("package");

            foreach (var xmlUsedPackage in xmlUsedPackages)
            {
                var packageId = xmlUsedPackage.Attribute("id")?.Value ?? string.Empty;
                var packageVersion = xmlUsedPackage.Attribute("version")?.Value ?? string.Empty;

                result.Packages.Add(new PackageInfo(packageId, packageVersion));
            }

            return result;
        }
    }
}