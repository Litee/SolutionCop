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
        public void Should_accept_correct_project()
        {
            const string parameters = "<TreatWarningsAsErrors/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrorsInAllConfigurations.csproj").FullName, XElement.Parse(parameters));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_for_warnings_not_enabled()
        {
            const string parameters = "<TreatWarningsAsErrors/>";
            Approvals.VerifyAll(_instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrorsDisabled.csproj").FullName, XElement.Parse(parameters)), "Errors");
        }

        [Fact]
        public void Should_fail_for_warnings_enabled_in_one_configuration_only()
        {
            const string parameters = "<TreatWarningsAsErrors/>";
            Approvals.VerifyAll(_instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrorsInOneConfigurationOnly.csproj").FullName, XElement.Parse(parameters)), "Errors");
        }

        [Fact]
        public void Should_skip_disabled_rule()
        {
            const string parameters = "<TreatWarningsAsErrors enabled=\"false\"></TreatWarningsAsErrors>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TreatWarningsAsErrorsInOneConfigurationOnly.csproj").FullName, XElement.Parse(parameters));
            Assert.Empty(errors);
        }
    }
}