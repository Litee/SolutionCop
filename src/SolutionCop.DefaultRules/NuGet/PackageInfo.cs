namespace SolutionCop.DefaultRules.NuGet
{
    internal sealed class PackageInfo
    {
        public PackageInfo(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }

        public string Version { get; }
    }
}