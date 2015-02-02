using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SolutionCop
{
    internal static class SolutionParser
    {
        public static SolutionInfo LoadFromFile(string pathToSolutionFile)
        {
            if (!new FileInfo(pathToSolutionFile).Exists)
            {
                Console.Out.WriteLine("FATAL: Cannot find solution file {0}", pathToSolutionFile);
                return new SolutionInfo();
            }
            Console.Out.WriteLine("INFO: Parsing solution {0}", pathToSolutionFile);
            var solutionFileLines = File.ReadAllLines(pathToSolutionFile).ToArray();
            var projectReferenceRegEx = new Regex("Project\\(\"([\\{\\}0-9A-Z\\-]+)\"\\) = \"(.*)\", \"(.*.csproj)\", \"([\\{\\}0-9A-Z\\-]+)\"\\s*");
            var projectRelativePaths = solutionFileLines.Select(line => projectReferenceRegEx.Match(line)).Where(x => x.Success).Select(x => x.Groups[3].ToString());
            var projectPaths = projectRelativePaths.Select(x => Path.Combine(Path.GetDirectoryName(pathToSolutionFile), x));
            return new SolutionInfo(projectPaths.ToArray());
        }
    }
}
