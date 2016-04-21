namespace SolutionCop.Core
{
    /// <summary>
    /// Interface for interacting with build servers.
    /// </summary>
    public interface IBuildServerReporter
    {
        /// <summary>
        /// Report about started test suite.
        /// </summary>
        /// <param name="suiteName">Test suite name.</param>
        void TestSuiteStarted(string suiteName);

        /// <summary>
        /// Report about finished test suite.
        /// </summary>
        /// <param name="suiteName">Test suite name.</param>
        void TestSuiteFinished(string suiteName);

        /// <summary>
        /// Report about ignored test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        void TestIgnored(string testName);

        /// <summary>
        /// Report about started test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        void TestStarted(string testName);

        /// <summary>
        /// Report about finished test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        void TestFinished(string testName);

        /// <summary>
        /// Report about failed test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        /// <param name="message">Textual representation of the error.</param>
        /// <param name="details">Detailed information on the test failure, typically a message and an exception stacktrace.</param>
        void TestFailed(string testName, string message, string details);
    }
}