namespace SolutionCop.Core
{
    using System;

    public class DefaultSolutionCopConsole : ISolutionCopConsole
    {
        public void LogDebug(string message, params object[] args)
        {
            Console.WriteLine("DEBUG: " + message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            Console.WriteLine("INFO: " + message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Console.WriteLine("WARNING: " + message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Console.WriteLine("ERROR: " + message, args);
        }
    }
}
