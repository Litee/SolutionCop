using System.IO;
using System.Reflection;

namespace SolutionCop.CommandLine
{
    using System;
    using System.Linq;

    using global::CommandLine;
    using Core;

    internal static class Program
    {

        private static void Main(string[] args)
        {
            var commandLineParameters = new CommandLineParameters();
            if (Parser.Default.ParseArguments(args, commandLineParameters))
            {
                Console.Out.WriteLine("INFO: StyleCop version " + Assembly.GetEntryAssembly().GetName().Version);
                VerifySolution(commandLineParameters);
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private static void VerifySolution(CommandLineParameters commandLineParameters)
        {
            var solutionInfo = SolutionParser.LoadFromFile(commandLineParameters.PathToSolution);

            if (!solutionInfo.IsParsed)
            {
                Console.Out.WriteLine("FATAL: Cannot parse solution file.");
                Environment.Exit(-1);
            }

            var pathToConfigFile = commandLineParameters.PathToConfigFile;
            if (string.IsNullOrEmpty(pathToConfigFile))
            {
                pathToConfigFile = Path.Combine(Path.GetDirectoryName(commandLineParameters.PathToSolution), "SolutionCop.xml");
                Console.Out.WriteLine("INFO: Custom path to config file is not specified, using default one: {0}", pathToConfigFile);
            }


            var errors = new ProjectsVerifier(new DefaultAnalysisLogger()).VerifyProjects(pathToConfigFile, solutionInfo.ProjectFilePaths.ToArray(), commandLineParameters.BuildServerType);
            Environment.Exit(errors.Any() ? -1 : 0);
        }
    }
}