namespace SolutionCop.Core
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class SolutionParser
    {
        public static SolutionInfo LoadFromFile(ISolutionCopConsole console, string pathToSolutionFile)
        {
            if (!new FileInfo(pathToSolutionFile).Exists)
            {
                console.LogError("Cannot find solution file {0}", pathToSolutionFile);
                return new SolutionInfo();
            }

            console.LogInfo("Parsing solution {0}", pathToSolutionFile);
            var solutionFileLines = File.ReadAllLines(pathToSolutionFile).ToArray();
            var projectReferenceRegEx = new Regex("Project\\(\"([\\{\\}0-9A-Z\\-]+)\"\\) = \"(.*)\", \"(.*.csproj)\", \"([\\{\\}0-9A-Z\\-]+)\"\\s*");
            var projectRelativePaths = solutionFileLines.Select(line => projectReferenceRegEx.Match(line)).Where(x => x.Success).Select(x => x.Groups[3].ToString());
            var projectPaths = projectRelativePaths.Select(x => Path.Combine(Path.GetDirectoryName(pathToSolutionFile), x));
            return new SolutionInfo(projectPaths.ToArray());
        }
    }
}
