using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    public class StyleCopEnabledRuleTests
    {
        private readonly StyleCopEnabledRule _instance;

        public StyleCopEnabledRuleTests()
        {
            _instance = new StyleCopEnabledRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled()
        {
            const string config = "<StyleCopEnabled/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabled.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled_with_old_format()
        {
            const string config = "<StyleCopEnabled/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabledOldFormat.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_exception()
        {
            const string config = @"<StyleCopEnabled>
<Exceptions>
<Exception>StyleCopDisabled.csproj</Exception>
<Exception>SomeNonExistingProject.csproj</Exception>
</Exceptions>
</StyleCopEnabled>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_StyleCop_is_disabled()
        {
            const string config = "<StyleCopEnabled/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<StyleCopEnabled enabled=\"false\"/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }
    }
}