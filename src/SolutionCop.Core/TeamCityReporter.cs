namespace SolutionCop.Core
{
    using System;

    using Param = System.Collections.Generic.KeyValuePair<string, string>;

    public class TeamCityReporter : IBuildServerReporter
    {
        #region Implementation of IBuildServerReporter

        /// <summary>
        /// Report about started test suite.
        /// </summary>
        /// <param name="suiteName">Test suite name.</param>
        public void TestSuiteStarted(string suiteName)
        {
            WriteCommand("testSuiteStarted", new Param("name", suiteName));
        }

        /// <summary>
        /// Report about finished test suite.
        /// </summary>
        /// <param name="suiteName">Test suite name.</param>
        public void TestSuiteFinished(string suiteName)
        {
            WriteCommand("testSuiteFinished", new Param("name", suiteName));
        }

        /// <summary>
        /// Report about ignored test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        public void TestIgnored(string testName)
        {
            WriteCommand("testIgnored", new Param("name", testName));
        }

        /// <summary>
        /// Report about started test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        public void TestStarted(string testName)
        {
            WriteCommand("testStarted", new Param("name", testName));
        }

        /// <summary>
        /// Report about finished test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        public void TestFinished(string testName)
        {
            WriteCommand("testFinished", new Param("name", testName));
        }

        /// <summary>
        /// Report about failed test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        /// <param name="message">Textual representation of the error.</param>
        /// <param name="details">Detailed information on the test failure, typically a message and an exception stacktrace.</param>
        public void TestFailed(string testName, string message, string details)
        {
            WriteCommand(
                "testFailed",
                new Param("name", testName),
                new Param("message", message),
                new Param("details", details));
        }

        #endregion

        public static void WriteCommand(string command, params Param[] args)
        {
            Console.Out.Write("##teamcity[");
            Console.Out.Write(command);

            foreach (var arg in args)
            {
                Console.Out.Write(" {0}='{1}'", arg.Key, EscapeForTeamCity(arg.Value));
            }

            Console.Out.WriteLine("]");
        }

        /// <summary>
        /// Escape input string as references in TeamCity documentation (https://confluence.jetbrains.com/display/TCD9/Build+Script+Interaction+with+TeamCity#BuildScriptInteractionwithTeamCity-servMsgsServiceMessages).
        /// </summary>
        /// <param name="originalString">Original string.</param>
        /// <returns>Escaped string.</returns>
        private static string EscapeForTeamCity(string originalString)
        {
            return originalString
                .Replace("|", "||")
                .Replace("'", "|'")
                .Replace("\r", "|r")
                .Replace("\n", "|n")
                .Replace("[", "|[")
                .Replace("]", "|]");
        }
    }
}