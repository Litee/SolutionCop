using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    public class SuppressOnlySpecificWarningsRuleTests
    {
        private readonly SuppressOnlySpecificWarningsRule _instance;

        public SuppressOnlySpecificWarningsRuleTests()
        {
            _instance = new SuppressOnlySpecificWarningsRule();
        }

        [Fact]
        public void Should_pass_with_no_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0420,0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressNoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_same_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0420,0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_warnings_in_different_order()
        {
            const string config = "<SuppressOnlySpecificWarnings>0465,0420</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_with_subset_of_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0465,0420</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressOneWarning.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Approvals.VerifyAll(errors, "Unapproved warnings");
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_in_one_config_only()
        {
            const string config = "<SuppressOnlySpecificWarnings></SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressTwoWarningsInOneConfig.csproj").FullName, XElement.Parse(config));
            Approvals.VerifyAll(errors, "Unapproved warnings");
        }

        [Fact]
        public void Should_fail_if_one_unapproved_warning_suppressed()
        {
            const string config = "<SuppressOnlySpecificWarnings>0465</SuppressOnlySpecificWarnings>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\SuppressTwoWarnings.csproj").FullName, XElement.Parse(config));
            Approvals.VerifyAll(errors, "Unapproved warnings");
        }
    }
}