using System;
using System.Collections.Generic;

namespace SolutionCop.Core
{
    using Arg = KeyValuePair<string, string>;

    public class TeamCityReporter : IBuildServerReporter
    {
        #region Implementation of IBuildServerReporter

        /// <summary>
        /// Report about started test suite.
        /// </summary>
        /// <param name="suiteName">Test suite name.</param>
        public void TestSuiteStarted(string suiteName)
        {
            WriteCommand("testSuiteStarted", new Arg("name", suiteName));
        }

        /// <summary>
        /// Report about finished test suite.
        /// </summary>
        /// <param name="suiteName">Test suite name.</param>
        public void TestSuiteFinished(string suiteName)
        {
            WriteCommand("testSuiteFinished", new Arg("name", suiteName));
        }

        /// <summary>
        /// Report about ignored test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        public void TestIgnored(string testName)
        {
            WriteCommand("testIgnored", new Arg("name", testName));
        }

        /// <summary>
        /// Report about started test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        public void TestStarted(string testName)
        {
            WriteCommand("testStarted", new Arg("name", testName));
        }

        /// <summary>
        /// Report about finished test.
        /// </summary>
        /// <param name="testName">Test name.</param>
        public void TestFinished(string testName)
        {
            WriteCommand("testFinished", new Arg("name", testName));
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
                new Arg("name", testName),
                new Arg("message", message),
                new Arg("details", details));
        }

        #endregion

        public static void WriteCommand(string command, params Arg[] args)
        {
            Console.Out.Write("##teamcity[");
            Console.Out.Write(command);

            foreach (var arg in args)
            {
                Console.Out.Write(" {0}='{1}'", arg.Key, EscapeForTeamCity(arg.Value));
            }

            Console.Out.WriteLine("]");
        }

        private static string EscapeForTeamCity(string originalString)
        {
            return originalString.Replace("|", "||").Replace("'", "|'").Replace("\r", "|r").Replace("\n", "|n").Replace("]", "|]");
        }
    }
}