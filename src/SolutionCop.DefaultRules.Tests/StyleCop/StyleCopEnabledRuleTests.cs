using System.IO;
using System.Xml.Linq;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using SolutionCop.DefaultRules.StyleCop;
using Xunit;

namespace SolutionCop.DefaultRules.Tests.StyleCop
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class StyleCopEnabledRuleTests : ProjectRuleTest
    {
        public StyleCopEnabledRuleTests() : base(new StyleCopEnabledRule())
        {
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled/>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabled.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_StyleCop_is_enabled_with_old_format()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled/>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopEnabledOldFormat.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_exception()
        {
            var xmlConfig = XElement.Parse(@"<StyleCopEnabled>
<Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse(@"<StyleCopEnabled>
<Exception>Some text</Exception>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"<StyleCopEnabled>
<Dummy>Some text</Dummy>
<Exception><Project>StyleCopDisabled.csproj</Project></Exception>
</StyleCopEnabled>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_StyleCop_is_disabled()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled/>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<StyleCopEnabled enabled=\"false\"/>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\StyleCopEnabled\StyleCopDisabled.csproj").FullName);
        }
    }
}