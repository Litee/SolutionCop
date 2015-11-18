using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SolutionCop.Core;

namespace SolutionCop.MSBuild
{
    internal class MsBuildSolutionCopConsole : ISolutionCopConsole
    {
        private readonly TaskLoggingHelper _taskLoggingHelper;

        public MsBuildSolutionCopConsole(TaskLoggingHelper taskLoggingHelper)
        {
            _taskLoggingHelper = taskLoggingHelper;
        }

        public void LogDebug(string message, params object[] args)
        {
            _taskLoggingHelper.LogMessage(MessageImportance.Low, "DEBUG: " + message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            _taskLoggingHelper.LogMessage("INFO: " + message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _taskLoggingHelper.LogWarning("WARNING: " + message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _taskLoggingHelper.LogError("ERROR: " + message, args);
        }
    }
}