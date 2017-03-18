namespace SolutionCop.Core
{
    /// <summary>
    /// Null build server reporter.
    /// </summary>
    public class NullBuildServerReporter : IBuildServerReporter
    {
        public void TestSuiteStarted(string suiteName)
        {
        }

        public void TestSuiteFinished(string suiteName)
        {
        }

        public void TestIgnored(string testName)
        {
        }

        public void TestStarted(string testName)
        {
        }

        public void TestFinished(string testName)
        {
        }

        public void TestFailed(string testName, string message, string details)
        {
        }

        public void SolutionVerificationFailed(string description)
        {
        }

        public void SolutionVerificationPassed(string description)
        {
        }
    }
}