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
    public class SuppressWarningsRuleTests
    {
        private readonly SuppressWarningsRule _instance;

        public SuppressWarningsRuleTests()
        {
            _instance = new SuppressWarningsRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_with_neither_warning_suppressed()
        {
            const string config = "<SuppressWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_no_warnings_suppressed()
        {
            const string config = "<SuppressWarnings></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_same_warnings_suppressed()
        {
            const string config = "<SuppressWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_warnings_in_different_order()
        {
            const string config = "<SuppressWarnings><Warning>0465</Warning><Warning>0420</Warning></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_subset_of_warnings_suppressed()
        {
            const string config = "<SuppressWarnings><Warning>0465</Warning><Warning>0420</Warning></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressOneWarning.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<SuppressWarnings enabled=\"false\"><Warning>0465</Warning></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed()
        {
            const string config = "<SuppressWarnings></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_two_unapproved_warnings_suppressed_but_project_is_an_exception()
        {
            const string config = @"
<SuppressWarnings>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><Project>SuppressTwoWarnings.csproj</Project></Exception>
</SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_two_unapproved_warnings_suppressed_but_they_are_in_exception()
        {
            const string config = @"
<SuppressWarnings>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception>
    <Project>SuppressTwoWarnings.csproj</Project>
    <Warning>0420</Warning>
    <Warning>0465</Warning>
  </Exception>
</SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_two_unapproved_warnings_suppressed_but_only_one_is_in_exception()
        {
            const string config = @"
<SuppressWarnings>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception>
    <Project>SuppressTwoWarnings.csproj</Project>
    <Warning>0465</Warning>
  </Exception>
</SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            const string config = @"
<SuppressWarnings>
  <Exception>Some text</Exception>
  <Exception><Project>SuppressTwoWarnings.csproj</Project></Exception>
</SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_in_one_config_only()
        {
            const string config = "<SuppressWarnings></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarningsInOneConfig.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_one_unapproved_warning_suppressed()
        {
            const string config = "<SuppressWarnings><Warning>0465</Warning></SuppressWarnings>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}