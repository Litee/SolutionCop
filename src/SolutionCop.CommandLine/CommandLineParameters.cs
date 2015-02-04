using CommandLine;
using CommandLine.Text;

namespace SolutionCop.CommandLine
{
    class CommandLineParameters
    {
        [Option('s', "solution", Required = true)]
        public string PathToSolution { get; set; }

        [Option('c', "config")]
        public string PathToConfigFile { get; set; }

        [Option('b', "build-server", Required = false, DefaultValue = BuildServer.None)]
        public BuildServer BuildServerType { get; set; }

        [HelpOption]
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
