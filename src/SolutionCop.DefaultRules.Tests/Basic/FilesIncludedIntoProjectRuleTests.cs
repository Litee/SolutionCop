namespace SolutionCop.DefaultRules.Tests.Basic
{
    using System.IO;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.Basic;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class FilesIncludedIntoProjectRuleTests : ProjectRuleTest
    {
        public FilesIncludedIntoProjectRuleTests() : base(new FilesIncludedIntoProjectRule())
        {
        }

        [Fact]
        public void Should_pass_if_all_files_are_included()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\AllFilesIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_all_files_are_included_with_different_case()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\AllFilesIncludedIntoProject_DifferentCase.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_files_do_not_match_search_pattern()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.txt</FileName>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\AllFilesIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_file_is_not_included()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
</FilesIncludedIntoProject>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_file_is_not_included_but_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_file_name_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><FileName>My.File.cs</FileName></Exception>
  <Exception><FileName>AssemblyInfo.cs</FileName></Exception>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_file_name_has_non_standard_char()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject_2\AllFilesIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_file_name_with_wildcard_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><FileName>My.*.cs</FileName></Exception>
  <Exception><FileName>AssemblyInfo.cs</FileName></Exception>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_both_project_and_file_name_with_wildcard_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception>
    <Project>FileNotIncludedIntoProject.csproj</Project>
    <FileName>My.*.cs</FileName>
    <FileName>AssemblyInfo.cs</FileName>
  </Exception>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_both_project_and_file_name_with_wildcard_is_an_exception_plus_global_file_exception()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception>
    <Project>FileNotIncludedIntoProject.csproj</Project>
    <FileName>My.*.cs</FileName>
  </Exception>
  <Exception>
    <FileName>AssemblyInfo.cs</FileName>
  </Exception>
</FilesIncludedIntoProject>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_file_name_is_an_exception_but_for_another_project()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception><Project>SomeNonExistingProject.csproj</Project><FileName>My.File.cs</FileName><FileName>AssemblyInfo.cs</FileName></Exception>
</FilesIncludedIntoProject>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Exception>Some text</Exception>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
  <FileName>*.cs</FileName>
  <Dummy>Some text</Dummy>
  <Dummy2>Some text</Dummy2>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_no_search_pattern()
        {
            var xmlConfig = XElement.Parse(@"
<FilesIncludedIntoProject>
</FilesIncludedIntoProject>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<FilesIncludedIntoProject enabled=\"false\"/>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
        }
    }
}