using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
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
            const string config = "<SuppressOnlySpecificWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_no_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_same_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_warnings_in_different_order()
        {
            const string config = "<SuppressOnlySpecificWarnings><Warning>0465</Warning><Warning>0420</Warning></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_with_subset_of_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings><Warning>0465</Warning><Warning>0420</Warning></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressOneWarning.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<SuppressOnlySpecificWarnings enabled=\"false\"><Warning>0465</Warning></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_in_one_config_only()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarningsInOneConfig.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_one_unapproved_warning_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings><Warning>0465</Warning></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}