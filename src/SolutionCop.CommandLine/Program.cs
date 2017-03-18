namespace SolutionCop.CommandLine
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Core;
    using global::CommandLine;

    internal static class Program
    {
        private static readonly DefaultSolutionCopConsole SolutionCopConsole = new DefaultSolutionCopConsole();

        private static void Main(string[] args)
        {
            var commandLineParameters = new CommandLineParameters();

            if (Parser.Default.ParseArguments(args, commandLineParameters))
            {
                // Process legacy parameters
                if (commandLineParameters.BuildServerType == BuildServer.TeamCity)
                {
                    commandLineParameters.ReportFormat = ReportFormatType.TeamCityVerbose;
                }
                if (commandLineParameters.BuildServerNoSuccessMessages && commandLineParameters.ReportFormat == ReportFormatType.TeamCityVerbose)
                {
                    commandLineParameters.ReportFormat = ReportFormatType.TeamCityNoSuccessMessage;
                }
                SolutionCopConsole.LogInfo("StyleCop version " + Assembly.GetEntryAssembly().GetName().Version);
                VerifySolution(commandLineParameters);
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private static void VerifySolution(ICommandLineParameters commandLineParameters)
        {
            var solutionInfo = SolutionParser.LoadFromFile(SolutionCopConsole, commandLineParameters.PathToSolution);

            if (solutionInfo.IsParsed)
            {
                var pathToConfigFile = commandLineParameters.PathToConfigFile;
                if (string.IsNullOrEmpty(pathToConfigFile))
                {
                    pathToConfigFile = Path.Combine(Path.GetDirectoryName(commandLineParameters.PathToSolution), "SolutionCop.xml");
                    SolutionCopConsole.LogInfo("Custom path to config file is not specified, using default one: {0}", pathToConfigFile);
                }

                var reporter = CreateBuildServerReporter(commandLineParameters.ReportFormat);

                var verificationResult = new ProjectsVerifier(SolutionCopConsole, reporter).VerifyProjects(
                    pathToConfigFile,
                    solutionInfo.ProjectFilePaths.ToArray(),
                    (errors, validationResults) =>
                        {
                            if (errors.Any())
                            {
                                SolutionCopConsole.LogError("***** Full list of errors: *****");
                                errors.ForEach(x => SolutionCopConsole.LogError(x));
                                reporter.SolutionVerificationFailed(string.Concat("FAILED - ", Path.GetFileName(pathToConfigFile)));
                            }
                            else
                            {
                                SolutionCopConsole.LogInfo("No errors found!");
                                reporter.SolutionVerificationPassed(string.Concat("PASSED - ", Path.GetFileName(pathToConfigFile)));
                            }
                        });
                Environment.Exit(verificationResult == VerificationResult.ErrorsFound ? -1 : 0);
            }
            else
            {
                SolutionCopConsole.LogError("Cannot parse solution file.");
                Environment.Exit(-1);
            }
        }

        private static IBuildServerReporter CreateBuildServerReporter(ReportFormatType reportFormat)
        {
            IBuildServerReporter reporter;
            switch (reportFormat)
            {
                case ReportFormatType.TeamCityVerbose:
                    reporter = new TeamCityReporter(showSuccessMessage: true);
                    break;
                case ReportFormatType.TeamCityNoSuccessMessage:
                    reporter = new TeamCityReporter(showSuccessMessage: false);
                    break;
                default:
                    reporter = new NullBuildServerReporter();
                    break;
            }
            return reporter;
        }
    }
}