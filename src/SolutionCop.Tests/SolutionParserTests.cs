using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class SolutionParserTests
    {
        [Fact]
        public void Should_parse_valid_solution_file()
        {
            var solutionInfo = SolutionParser.LoadFromFile(new FileInfo(@"..\..\Data\ValidSolutionVS2013.sln").FullName);
            Assert.True(solutionInfo.IsParsed);
            Assert.NotEmpty(solutionInfo.ProjectFilePaths);
            Approvals.VerifyAll(solutionInfo.ProjectFilePaths.Select(x => Path.GetFileName(x)), "PathsToProjects");
            Assert.Equal(5, solutionInfo.ProjectFilePaths.Count());
        }

        [Fact]
        public void Should_parse_empty_solution_file()
        {
            var solutionInfo = SolutionParser.LoadFromFile(new FileInfo(@"..\..\Data\EmptySolutionVS2013.sln").FullName);
            Assert.True(solutionInfo.IsParsed);
            Assert.Empty(solutionInfo.ProjectFilePaths);
        }

        [Fact]
        public void Should_fail_for_non_existing_file()
        {
            var solutionInfo = SolutionParser.LoadFromFile(@"C:\NonExistingSolution.sln");
            Assert.False(solutionInfo.IsParsed);
            Assert.Empty(solutionInfo.ProjectFilePaths);
        }
    }
}
