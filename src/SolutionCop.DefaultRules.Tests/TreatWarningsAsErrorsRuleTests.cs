using System;
using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;
using Xunit.Extensions;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TreatWarningsAsErrorsRuleTests : IDisposable
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

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_all_must_and_all_are(string csproj)
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_fail_if_all_must_and_only_two_are(string csproj)
        {
            NamerFactory.AdditionalInformation = csproj;
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_all_must_and_only_two_are_but_project_is_in_exceptions_list(string csproj)
        {
            const string config = @"<TreatWarningsAsErrors>
<AllWarnings/>
  <Exception><Project>TreatTwoWarningsAsErrorsInAllConfigurations.csproj</Project></Exception>
  <Exception><Project>TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj</Project></Exception>
  <Exception><Project>SomeNonExistingProjectName.csproj</Project></Exception>
</TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_all_must_and_all_in_one_config_are()
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_all_must_and_none_are()
        {
            const string config = "<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_two_must_and_all_warnings_as_errors(string csproj)
        {
            const string config = "<TreatWarningsAsErrors><Warning>0420,0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_two_must_and_those_two_are(string csproj)
        {
            const string config = "<TreatWarningsAsErrors><Warning>0420</Warning><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_two_must_and_those_two_are_in_different_order(string csproj)
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465</Warning><Warning>0420</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_one_must_and_this_specific_one_and_one_more_are(string csproj)
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_two_must_and_both_are_but_only_in_one_configuration()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0420</Warning><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_fail_if_two_must_and_different_ones_are(string csproj)
        {
            NamerFactory.AdditionalInformation = csproj;
            const string config = "<TreatWarningsAsErrors><Warning>0466</Warning><Warning>0421</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_two_must_and_none_are()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465 </Warning><Warning> 0420</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_one_must_and_none_are()
        {
            const string config = "<TreatWarningsAsErrors><Warning>0465</Warning></TreatWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatNoWarningsAsErrors.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_none_must_and_all_are(string csproj)
        {
            const string config = "<TreatWarningsAsErrors/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            Assert.Empty(errors);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_none_must_and_two_are(string csproj)
        {
            const string config = "<TreatWarningsAsErrors/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_none_must_and_none_are()
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

        public void Dispose()
        {
            NamerFactory.AdditionalInformation = null;
        }
    }
}