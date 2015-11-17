using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SolutionCop.Core;

namespace SolutionCop.MSBuild
{
    internal class MsBuileAnalysisLogger : IAnalysisLogger
    {
        private readonly TaskLoggingHelper _taskLoggingHelper;

        public MsBuileAnalysisLogger(TaskLoggingHelper taskLoggingHelper)
        {
            _taskLoggingHelper = taskLoggingHelper;
        }

        public void LogDebug(string message, params object[] args)
        {
            _taskLoggingHelper.LogMessage(MessageImportance.Low, message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            _taskLoggingHelper.LogMessage(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _taskLoggingHelper.LogWarning(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _taskLoggingHelper.LogError(message, args);
        }
    }
}