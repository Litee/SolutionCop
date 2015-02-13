namespace SolutionCop.CommandLine
{
    using global::CommandLine;
    using global::CommandLine.Text;

    internal enum BuildServer
    {
        None,
        TeamCity
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal class CommandLineParameters
    {
        [Option('s', "solution", Required = true, HelpText = "Path to the Visual Studio solution file to analyze.")]
        public string PathToSolution { get; set; }

        [Option('c', "config", Required = false, HelpText = "Path to the configuration file with rule settings.")]
        public string PathToConfigFile { get; set; }

        [Option('b', "build-server", Required = false, HelpText = "Specify this parameter if you want additional information to be sent to CI server via console interaction. Supported value: TeamCity")]
        public BuildServer BuildServerType { get; set; }

        [HelpOption]
        // ReSharper disable once UnusedMember.Global
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
