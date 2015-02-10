using System.IO;
using System.Xml.Linq;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Xunit;
using Xunit.Extensions;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TreatWarningsAsErrorsRuleTests : ProjectRuleTest
    {
        public TreatWarningsAsErrorsRuleTests()
            : base(new TreatWarningsAsErrorsRule())
        {
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_all_must_and_all_are(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_fail_if_all_must_and_only_two_are(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_all_must_and_only_two_are_but_project_is_in_exceptions_list(string csproj)
        {
            var xmlConfig = XElement.Parse(@"<TreatWarningsAsErrors>
<AllWarnings/>
  <Exception><Project>TreatTwoWarningsAsErrorsInAllConfigurations.csproj</Project></Exception>
  <Exception><Project>TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj</Project></Exception>
  <Exception><Project>SomeNonExistingProjectName.csproj</Project></Exception>
</TreatWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Fact]
        public void Should_fail_if_all_must_and_all_in_one_config_are()
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
        }

        [Theory]
        [InlineData("TreatNoWarningsAsErrors.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInAllConfigurations.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInGlobalConfiguration.csproj")]
        public void Should_fail_if_all_must_and_none_are(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><AllWarnings/></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_two_must_and_all_warnings_as_errors(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0420,0465</Warning></TreatWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_two_must_and_those_two_are(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0420</Warning><Warning>0465</Warning></TreatWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_two_must_and_those_two_are_in_different_order(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0465</Warning><Warning>0420</Warning></TreatWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_one_must_and_this_specific_one_and_one_more_are(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0465</Warning></TreatWarningsAsErrors>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Fact]
        public void Should_fail_if_two_must_and_both_are_but_only_in_one_configuration()
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0420</Warning><Warning>0465</Warning></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatTwoWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_fail_if_two_must_and_different_ones_are(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0466</Warning><Warning>0421</Warning></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatNoWarningsAsErrors.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInAllConfigurations.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInGlobalConfiguration.csproj")]
        public void Should_fail_if_two_must_and_none_are(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0465 </Warning><Warning> 0420</Warning></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatNoWarningsAsErrors.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInAllConfigurations.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInGlobalConfiguration.csproj")]
        public void Should_fail_if_one_must_and_none_are(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><Warning>0465</Warning></TreatWarningsAsErrors>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatAllWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatAllWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_none_must_and_all_are(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors/>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatTwoWarningsAsErrorsInAllConfigurations.csproj")]
        [InlineData("TreatTwoWarningsAsErrorsInGlobalConfiguration.csproj")]
        public void Should_pass_if_none_must_and_two_are(string csproj)
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors/>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Theory]
        [InlineData("TreatNoWarningsAsErrors.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInAllConfigurations.csproj")]
        [InlineData("TreatNoWarningsAsErrorsExplicitlyInGlobalConfiguration.csproj")]
        public void Should_pass_if_none_must_and_none_are(string csproj)
        {
            // Special step to generate unique *.received.txt files for each theory run. Note that it is cleared in Dispose() method to avoid affecting other tests.
            NamerFactory.AdditionalInformation = csproj;
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors/>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\" + csproj).FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors enabled=\"false\"><AllWarnings/></TreatWarningsAsErrors>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><AllWarnings/><Dummy/></TreatWarningsAsErrors>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_is_empty()
        {
            var xmlConfig = XElement.Parse("<TreatWarningsAsErrors><AllWarnings/><Exception/></TreatWarningsAsErrors>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TreatWarningsAsErrors\TreatAllWarningsAsErrorsInOneConfigurationOnly.csproj").FullName);
        }

        // TODO Case for exception with warnings inside
    }
}