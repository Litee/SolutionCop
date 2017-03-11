namespace SolutionCop.DefaultRules.NuGet
{
    internal sealed class NuspecDependencyInfo
    {
        public NuspecDependencyInfo(string packageId, string packageVersion)
        {
            PackageId = packageId;
            PackageVersion = packageVersion;
        }

        public string PackageId { get; }

        public string PackageVersion { get; }
    }
}