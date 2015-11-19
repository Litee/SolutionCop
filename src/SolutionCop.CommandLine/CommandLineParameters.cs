using SolutionCop.Core;

namespace SolutionCop.CommandLine
{
    using global::CommandLine;
    using global::CommandLine.Text;



    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal sealed class CommandLineParameters
    {
        [Option('s', "solution", Required = true, HelpText = "Path to the Visual Studio solution file to analyze.")]
        public string PathToSolution { get; set; }

        [Option('c', "config", Required = false, HelpText = "Path to the configuration file with rule settings.")]
        public string PathToConfigFile { get; set; }

        [Option('b', "build-server", Required = false, HelpText = "Specify this parameter if you want additional information to be sent to CI server via console interaction. Supported value: TeamCity")]
        public BuildServer BuildServerType { get; set; }

        [Option("build-server-no-success-messages", DefaultValue = false, HelpText = "Hide success messages for build server output", Required = false)]
        public bool BuildServerNoSuccessMessages { get; set; }

        [HelpOption]
        // ReSharper disable once UnusedMember.Global
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
