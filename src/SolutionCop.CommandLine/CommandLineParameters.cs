using CommandLine;
using CommandLine.Text;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SolutionCop.CommandLine
{
    class CommandLineParameters
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

    internal enum BuildServer
    {
        None,
        TeamCity
    }
}
