namespace SolutionCop.CommandLine
{
    using global::CommandLine;
    using global::CommandLine.Text;
    using SolutionCop.Core;

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    internal sealed class CommandLineParameters : ICommandLineParameters
    {
        [Option('s', "solution", Required = true, HelpText = "Path to the Visual Studio solution file to analyze.")]
        public string PathToSolution { get; set; }

        [Option('c', "config", Required = false, HelpText = "Path to the configuration file with rule settings.")]
        public string PathToConfigFile { get; set; }

        [Option('b', "build-server", Required = false, HelpText = "Specify this parameter if you want additional information to be sent to CI server via console interaction. Supported value: TeamCity. LEGACY - use `--report-format TeamCityVerbose` option instead.")]
        public BuildServer BuildServerType { get; set; }

        [Option('f', "report-format", Required = false, HelpText = "Use this parameter to define how to output information - e.g. send to CI server via console interaction. Supported values: DefaultConsole, TeamCityVerbose, TeamCityNoSuccessMessage")]
        public ReportFormatType ReportFormat { get; set; }

        [Option("build-server-no-success-messages", Required = false, DefaultValue = false, HelpText = "Hide success messages for build server output. LEGACY - use '--report-format TeamCityNoSuccessMessage' instead'.")]
        public bool BuildServerNoSuccessMessages { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }
}
