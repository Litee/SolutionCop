namespace SolutionCop.DefaultRules.Tests.StyleCop
{
    using System.IO;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.StyleCop;
    using Xunit;
    using Xunit.Extensions;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TreatStyleCopWarningsAsErrorsRuleTests : ProjectRuleTest
    {
        public TreatStyleCopWarningsAsErrorsRuleTests()
            : base(new TreatStyleCopWarningsAsErrorsRule())
        {
        }

        [Theory]
        [InlineData("Default.csproj")]
        [InlineData("False_And_False.csproj")]
        [InlineData("False_Global.csproj")]
        public void Should_pass_if_no_suppression(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("True_And_Default.csproj")]
        [InlineData("True_And_False.csproj")]
        [InlineData("True_And_True.csproj")]
        [InlineData("True_Global.csproj")]
        public void Should_fail_if_errors_are_treated_as_warnings(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors></TreatStyleCopWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("True_And_Default.csproj")]
        [InlineData("True_And_False.csproj")]
        [InlineData("True_And_True.csproj")]
        [InlineData("True_Global.csproj")]
        public void Should_pass_if_errors_are_treated_as_warnings_but_project_is_in_exceptions_list(string csproj)
        {
            var xmlConfig = XElement.Parse(@"
<TreatStyleCopWarningsAsErrors>
  <Exception><Project>True_And_Default.csproj</Project></Exception>
  <Exception><Project>True_And_False.csproj</Project></Exception>
  <Exception><Project>True_And_True.csproj</Project></Exception>
  <Exception><Project>True_Global.csproj</Project></Exception>
</TreatStyleCopWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\" + csproj).FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors enabled=\"false\"></TreatStyleCopWarningsAsErrors>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\True_And_False.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors><Exception/></TreatStyleCopWarningsAsErrors>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\True_And_False.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse("<TreatStyleCopWarningsAsErrors><Dummy/><Exception><Project>NoStyleCopWarningsAsErrors_Default.csproj</Project></Exception></TreatStyleCopWarningsAsErrors>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TreatStyleCopWarningsAsErrors\True_And_False.csproj").FullName);
        }
    }
}