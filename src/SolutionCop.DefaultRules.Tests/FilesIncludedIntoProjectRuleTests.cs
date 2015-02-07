using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class FilesIncludedIntoProjectRuleTests
    {
        private readonly FilesIncludedIntoProjectRule _instance;

        public FilesIncludedIntoProjectRuleTests()
        {
            _instance = new FilesIncludedIntoProjectRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_all_files_are_included()
        {
            const string config = "<FilesIncludedIntoProject/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\AllFilesIncludedIntoProject.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_file_is_not_included()
        {
            const string config = "<FilesIncludedIntoProject/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_file_is_not_included_but_project_is_an_exception()
        {
            const string config = @"
<FilesIncludedIntoProject>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            const string config = @"
<FilesIncludedIntoProject>
  <Exception>Some text</Exception>
  <Exception><Project>FileNotIncludedIntoProject.csproj</Project></Exception>
</FilesIncludedIntoProject>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<FilesIncludedIntoProject enabled=\"false\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\FilesIncludedIntoProject\FileNotIncludedIntoProject.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}