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
    public class StyleCopEnabledRuleTests : ProjectRuleTest
    {
        public StyleCopEnabledRuleTests() : base(new StyleCopEnabledRule())
        {
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(Instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled/>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabled.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled_with_old_format()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled/>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabledOldFormat.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_exception()
        {
            var xmlConfig = XElement.Parse(@"<StyleCopEnabled>
<Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse(@"<StyleCopEnabled>
<Exception>Some text</Exception>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_StyleCop_is_disabled()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled/>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled enabled=\"false\"/>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName, xmlConfig);
        }
    }
}