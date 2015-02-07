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
    public class TreatStyleCopWarningsAsErrorsRuleTests : IDisposable
    {
        private readonly TreatStyleCopWarningsAsErrorsRule _instance;

        public TreatStyleCopWarningsAsErrorsRuleTests()
        {
            _instance = new TreatStyleCopWarningsAsErrorsRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Theory]
        [InlineData("TreatStyleCopWarningsAsErrors_All.csproj")]
        [InlineData("TreatStyleCopWarningsAsErrors_Global.csproj")]
        public void Should_pass_if_all_must_and_all_are(string csproj)
        {
            const string config = "<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_all_must_and_all_in_one_config_are()
        {
            const string config = "<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\TreatStyleCopWarningsAsErrors_OneOfTwo.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Theory]
        [InlineData("NoStyleCopWarningsAsErrors_Default.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_All.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_Global.csproj")]
        public void Should_fail_if_no_warnings_treated_as_errors(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            const string config = "<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Theory]
        [InlineData("NoStyleCopWarningsAsErrors_Default.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_All.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_Global.csproj")]
        public void Should_pass_if_no_warnings_treated_as_errors_but_project_is_in_exceptions_list(string csproj)
        {
            const string config = @"
<TreatStyleCopWarningsAsErrors>
  <Exception><Project>NoStyleCopWarningsAsErrors_Default.csproj</Project></Exception>
  <Exception><Project>NoStyleCopWarningsAsErrors_All.csproj</Project></Exception>
  <Exception><Project>NoStyleCopWarningsAsErrors_Global.csproj</Project></Exception>
</TreatStyleCopWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<TreatStyleCopWarningsAsErrors enabled=\"false\"></TreatStyleCopWarningsAsErrors>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\TreatStyleCopWarningsAsErrors_OneOfTwo.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        public void Dispose()
        {
            // Setting to null to switch back to standard file naming for approvals.
            NamerFactory.AdditionalInformation = null;
        }
    }
}