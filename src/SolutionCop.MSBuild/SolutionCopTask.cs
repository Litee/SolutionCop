using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using SolutionCop.Core;

namespace SolutionCop.MSBuild
{

    public class SolutionCopTask : Task
    {
        static SolutionCopTask()
        {
#if DEBUG
            // Add the Default Listener back as ReSharper removes it
            Trace.Listeners.Add(new DefaultTraceListener());
#endif
        }

        public string ProjectFullPath { get; set; }

        public override bool Execute()
        {
            try
            {
                Log.LogMessage("SolutionCop: Starting analysis for project {0}", ProjectFullPath);
                Log.LogMessage("SolutionCop: Working folder = {0}", Path.GetFullPath("."));
                DirectoryInfo dir = Directory.GetParent(BuildEngine.ProjectFileOfTaskNode);
                string pathToCofigFile;
                while (true)
                {
                    if (dir == null)
                    {
                        Log.LogError("SolutionCop: Cannot find SolutionCop.xml config file in parent folders.");
                        return false;
                    }
                    pathToCofigFile = Path.Combine(dir.FullName, "SolutionCop.xml");
                    if (File.Exists(pathToCofigFile))
                    {
                        Log.LogMessage("SolutionCop: Found SolutionCop.xml config file: {0}", pathToCofigFile);
                        break;
                    }
                    dir = dir.Parent;
                }

                var msBuileAnalysisLogger = new MsBuildSolutionCopConsole(Log);
                var projectsVerifier = new ProjectsVerifier(msBuileAnalysisLogger, new NullBuildServerReporter());
                var verificationResult = projectsVerifier.VerifyProjects(pathToCofigFile, new[] { ProjectFullPath }, (errors, validationResults) =>
                {
                    if (errors.Any())
                    {
                        errors.ForEach(x => Log.LogError("SolutionCop: " + x));
                    }
                    else
                    {
                        msBuileAnalysisLogger.LogInfo("No errors found!");
                    }
                });
                Log.LogMessage("SolutionCop: Analysis finished!");
                return verificationResult == VerificationResult.NoErrors;
            }
            catch (Exception e)
            {
                Log.LogError("SolutionCop: Unexpected error: {0}", e.Message);
                return false;
            }
        }
    }
}