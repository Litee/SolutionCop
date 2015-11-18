﻿using System.Collections.Generic;
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
        private static readonly DefaultSolutionCopConsole SolutionCopConsole = new DefaultSolutionCopConsole();

        private static void Main(string[] args)
        {
            var commandLineParameters = new CommandLineParameters();
            if (Parser.Default.ParseArguments(args, commandLineParameters))
            {
                SolutionCopConsole.LogInfo("StyleCop version " + Assembly.GetEntryAssembly().GetName().Version);
                VerifySolution(commandLineParameters);
            }
            else
            {
                Environment.Exit(-1);
            }
        }

        private static void VerifySolution(CommandLineParameters commandLineParameters)
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


                var verificationResult = new ProjectsVerifier(SolutionCopConsole).VerifyProjects(pathToConfigFile, solutionInfo.ProjectFilePaths.ToArray(), (errors, validationResults) =>
                {
                    if (errors.Any())
                    {
                        SolutionCopConsole.LogError("***** Full list of errors: *****");
                        errors.ForEach(x => SolutionCopConsole.LogError(x));
                        if (commandLineParameters.BuildServerType == BuildServer.TeamCity)
                        {
                            // adding empty line for a better formatting in TC output
                            var extendedErrorsInfo = Enumerable.Repeat(string.Empty, 1).Concat(errors.Select((x, idx) => string.Format("ERROR ({0}/{1}): {2}", idx + 1, errors.Count, x))).Concat(Enumerable.Repeat(String.Empty, 1)).Concat(validationResults.Select(x => String.Format("INFO: Rule {0} is {1}", x.RuleId, x.IsEnabled ? "enabled" : "disabled")));
                            Console.Out.WriteLine("##teamcity[testFailed name='SolutionCop' message='FAILED - {0}' details='{1}']", EscapeForTeamCity(Path.GetFileName(pathToConfigFile)), string.Join("|r|n", extendedErrorsInfo.Select(EscapeForTeamCity)));
                            Console.Out.WriteLine("##teamcity[buildStatus text='FAILED - {0}']", EscapeForTeamCity(Path.GetFileName(pathToConfigFile)));
                        }
                    }
                    else
                    {
                        SolutionCopConsole.LogInfo("No errors found!");
                        if (commandLineParameters.BuildServerType == BuildServer.TeamCity)
                        {
                            Console.Out.WriteLine("##teamcity[buildStatus status='SUCCESS' text='PASSED - {0}']", EscapeForTeamCity(Path.GetFileName(pathToConfigFile)));
                        }
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

        private static string EscapeForTeamCity(string originalString)
        {
            return originalString.Replace("|", "||").Replace("'", "|'").Replace("\r", "|r").Replace("\n", "|n").Replace("]", "|]");
        }
    }
}