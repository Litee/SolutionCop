using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class TreatWarningsAsErrorsRuleTests
    {
        private readonly TreatWarningsAsErrorsRule _instance;

        public TreatWarningsAsErrorsRuleTests()
        {
            _instance = new TreatWarningsAsErrorsRule();
        }

        [Fact]
        public void Should_pass_if_all_warnings_must_be_treated_as_errors_and_project_treats_all_as_errors()
        {
            const string config = "<TreatWarningsAsErrors>All</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_all_warnings_must_be_treated_as_errors_and_project_treats_only_two_as_errors()
        {
            const string config = "<TreatWarningsAsErrors>All</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_all_warnings_must_be_treated_as_errors_and_project_treats_all_as_errors_but_only_in_one_configuration()
        {
            const string config = "<TreatWarningsAsErrors>All</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_all_warnings_must_be_treated_as_errors_and_project_treats_none_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors>All</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_two_warnings_must_be_treated_as_errors_and_project_treats_all_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors>0420,0465</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_two_warnings_must_be_treated_as_errors_and_project_treats_those_specific_two_as_errors()
        {
            const string config = "<TreatWarningsAsErrors>0420,0465</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_two_warnings_must_be_treated_as_errors_and_project_treats_those_specific_two_as_errors_in_different_order()
        {
            const string config = "<TreatWarningsAsErrors>0465, 0420</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_one_warning_must_be_treated_as_an_error_and_project_treats_this_specific_one_and_one_more_as_errors()
        {
            const string config = "<TreatWarningsAsErrors>0465</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_two_warnings_must_be_treated_as_errors_and_project_treats_those_but_only_in_one_configuration()
        {
            const string config = "<TreatWarningsAsErrors>0420,0465</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInOneConfigurationOnly.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_warnings_must_be_treated_as_errors_and_project_treats_different_ones_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors>0466,0421</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_warnings_must_be_treated_as_errors_and_project_treats_none_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors>0465, 0420</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_one_warning_must_be_treated_as_an_error_and_project_treats_none_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors>0465</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_no_warnings_must_be_treated_as_errors_and_project_treats_all_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_no_warnings_must_be_treated_as_errors_and_project_treats_two_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_no_warnings_must_be_treated_as_errors_and_project_treats_no_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<TreatWarningsAsErrors enabled=\"false\">All</TreatWarningsAsErrors>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }
    }
}