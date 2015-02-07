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
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabled.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled_with_old_format()
        {
            const string config = "<StyleCopEnabled/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabledOldFormat.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_exception()
        {
            const string config = @"<StyleCopEnabled>
<Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            const string config = @"<StyleCopEnabled>
<Exception>Some text</Exception>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_StyleCop_is_disabled()
        {
            const string config = "<StyleCopEnabled/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<StyleCopEnabled enabled=\"false\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}