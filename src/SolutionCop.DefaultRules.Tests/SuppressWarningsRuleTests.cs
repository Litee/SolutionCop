using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
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
        public void Should_pass_with_neither_warning_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0420,0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_no_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_same_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0420,0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_warnings_in_different_order()
        {
            const string config = "<SuppressOnlySpecificWarnings>0465,0420</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_subset_of_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0465,0420</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressOneWarning.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<SuppressOnlySpecificWarnings enabled=\"false\">0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_in_one_config_only()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarningsInOneConfig.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_one_unapproved_warning_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}