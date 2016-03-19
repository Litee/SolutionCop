namespace SolutionCop.Core
{
    public interface ISolutionCopConsole
    {
        void LogDebug(string message, params object[] args);

        void LogInfo(string message, params object[] args);

        void LogWarning(string message, params object[] args);

        void LogError(string message, params object[] args);
    }
}