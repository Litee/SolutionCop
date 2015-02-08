using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Xunit;
using Xunit.Extensions;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TreatStyleCopWarningsAsErrorsRuleTests : ProjectRuleTest
    {
        public TreatStyleCopWarningsAsErrorsRuleTests()
            : base(new TreatStyleCopWarningsAsErrorsRule())
        {
        }

        [Theory]
        [InlineData("TreatStyleCopWarningsAsErrors_All.csproj")]
        [InlineData("TreatStyleCopWarningsAsErrors_Global.csproj")]
        public void Should_pass_if_all_must_and_all_are(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_all_must_and_all_in_one_config_are()
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\TreatStyleCopWarningsAsErrors_OneOfTwo.csproj").FullName, xmlConfig);
        }

        [Theory]
        [InlineData("NoStyleCopWarningsAsErrors_Default.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_All.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_Global.csproj")]
        public void Should_fail_if_no_warnings_treated_as_errors(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName, xmlConfig);
        }

        [Theory]
        [InlineData("NoStyleCopWarningsAsErrors_Default.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_All.csproj")]
        [InlineData("NoStyleCopWarningsAsErrors_Global.csproj")]
        public void Should_pass_if_no_warnings_treated_as_errors_but_project_is_in_exceptions_list(string csproj)
        {
            var xmlConfig = XElement.Parse(@"
<TreatStyleCopWarningsAsErrors>
  <Exception><Project>NoStyleCopWarningsAsErrors_Default.csproj</Project></Exception>
  <Exception><Project>NoStyleCopWarningsAsErrors_All.csproj</Project></Exception>
  <Exception><Project>NoStyleCopWarningsAsErrors_Global.csproj</Project></Exception>
</TreatStyleCopWarningsAsErrors>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors enabled=\"false\"></TreatStyleCopWarningsAsErrors>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\TreatStyleCopWarningsAsErrors_OneOfTwo.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors><Exception/></TreatStyleCopWarningsAsErrors>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\TreatStyleCopWarningsAsErrors_OneOfTwo.csproj").FullName, xmlConfig);
        }
    }
}