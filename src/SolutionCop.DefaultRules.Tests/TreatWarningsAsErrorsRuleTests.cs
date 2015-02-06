using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TreatWarningsAsErrorsRuleTests
    {
        private readonly TreatWarningsAsErrorsRule _instance;

        public TreatWarningsAsErrorsRuleTests()
        {
            _instance = new TreatWarningsAsErrorsRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_all_warnings_must_be_treated_as_errors_and_project_treats_all_as_errors()
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_all_warnings_must_be_treated_as_errors_and_project_treats_only_two_as_errors()
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_all_warnings_must_be_treated_as_errors_and_project_treats_only_two_as_errors_but_project_is_in_exceptions_list()
        {
            const string config = @"<TreatWarningsAsErrors>
<AllWarnings/>
<Exception>TreatTwoWarningsAsErrorsInAllConfigurations.csproj</Exception>
<Exception>SomeNonExistingProjectName.csproj</Exception>
</TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_all_warnings_must_be_treated_as_errors_and_project_treats_them_in_one_configuration()
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_all_warnings_must_be_treated_as_errors_and_project_treats_none_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_two_warnings_must_be_treated_as_errors_and_project_treats_all_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0420,0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_two_warnings_must_be_treated_as_errors_and_project_treats_those_specific_two_as_errors()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0420</Warning><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_two_warnings_must_be_treated_as_errors_and_project_treats_those_specific_two_as_errors_in_different_order()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465</Warning><Warning>0420</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_one_warning_must_be_treated_as_an_error_and_project_treats_this_specific_one_and_one_more_as_errors()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_two_warnings_must_be_treated_as_errors_and_project_treats_those_but_only_in_one_configuration()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0420</Warning><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_warnings_must_be_treated_as_errors_and_project_treats_different_ones_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0466</Warning><Warning>0421</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_warnings_must_be_treated_as_errors_and_project_treats_none_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465 </Warning><Warning> 0420</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_one_warning_must_be_treated_as_an_error_and_project_treats_none_as_an_error()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_no_warnings_must_be_treated_as_errors_and_project_treats_all_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInAllConfigurations.csproj").FullName);
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_no_warnings_must_be_treated_as_errors_and_project_treats_two_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_no_warnings_must_be_treated_as_errors_and_project_treats_no_warnings_as_errors()
        {
            const string config = "<TreatWarningsAsErrors/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<TreatWarningsAsErrors enabled=\"false\"><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}