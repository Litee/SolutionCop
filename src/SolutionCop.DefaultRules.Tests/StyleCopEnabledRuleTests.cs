using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
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
        public void Should_pass_if_StyleCop_is_enabled()
        {
            const string config = "<StyleCopEnabled/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabled.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_StyleCop_is_disabled()
        {
            const string config = "<StyleCopEnabled/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }
        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<StyleCopEnabled enabled=\"false\"/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }
    }
}