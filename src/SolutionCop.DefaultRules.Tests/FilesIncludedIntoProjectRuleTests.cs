using System.IO;
using System.Xml.Linq;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class FilesIncludedIntoProjectRuleTests : ProjectRuleTest
    {
        public FilesIncludedIntoProjectRuleTests() : base(new FilesIncludedIntoProjectRule())
        {
        }

        [Fact]
        public void Should_pass_if_all_files_are_included()
        {
            var xmlConfig = XElement.Parse("<FilesIncludedIntoProject/>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\AllFilesIncludedIntoProject.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_file_is_not_included()
        {
            var xmlConfig = XElement.Parse("<FilesIncludedIntoProject/>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_file_is_not_included_but_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <Exception>Some text</Exception>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<FilesIncludedIntoProject enabled=\"false\"/>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName, xmlConfig);
        }
    }
}