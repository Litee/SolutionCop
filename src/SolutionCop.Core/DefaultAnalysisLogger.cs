using System;

namespace SolutionCop.Core
{
    public class DefaultAnalysisLogger : IAnalysisLogger
    {
        public void LogDebug(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message, params object[] args)
        {
            Console.WriteLine(message);
        }
    }
}
