using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class WarningLevelRuleTests : ProjectRuleTest
    {
        public WarningLevelRuleTests()
            : base(new WarningLevelRule())
        {
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(Instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_as_expected()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_as_expected_in_global_section()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInGlobalConfiguration.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_project_file_not_found()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations-DoesNotExist.csproj").FullName, xmlConfig);
            // TODO
//            errors.First().ShouldStartWith("Project file not found:");
//            errors.First().ShouldContain("WarningLevelTwoInAllConfigurations-DoesNotExist.csproj");
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_higher_than_expected()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>0</MinimalValue></WarningLevel>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_lower_than_expected()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>4</MinimalValue></WarningLevel>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_lower_than_expected_but_project_is_in_exceptions_list()
        {
            var xmlConfig = XElement.Parse(@"
<WarningLevel>
  <MinimalValue>4</MinimalValue>
  <Exception>
    <Project>WarningLevelTwoInAllConfigurations.csproj</Project>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProjectName.csproj</Project>
  </Exception>
</WarningLevel>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_lower_than_expected_but_project_have_exceptional_lower_value()
        {
            var xmlConfig = XElement.Parse(@"
<WarningLevel>
  <MinimalValue>4</MinimalValue>
  <Exception>
    <Project>WarningLevelTwoInAllConfigurations.csproj</Project>
    <MinimalValue>1</MinimalValue>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProjectName.csproj</Project>
    <MinimalValue>2</MinimalValue>
  </Exception>
</WarningLevel>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_higher_than_one_specified_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<WarningLevel>
  <MinimalValue>4</MinimalValue>
  <Exception>
    <Project>WarningLevelTwoInAllConfigurations.csproj</Project>
    <MinimalValue>3</MinimalValue>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProjectName.csproj</Project>
    <MinimalValue>0</MinimalValue>
  </Exception>
</WarningLevel>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_lower_than_expected_in_one_configuration()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>3</MinimalValue></WarningLevel>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInOneConfigurationAndThreeInAnother.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_specified_in_one_configuration_only()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInOneConfiguration.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<WarningLevel enabled=\"false\"><MinimalValue>4</MinimalValue></WarningLevel>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_without_minimal_value_specified_if_rule_disabled()
        {
            var xmlConfig = XElement.Parse("<WarningLevel enabled=\"false\"/>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        // Check bad configurations

        [Fact]
        public void Should_fail_if_no_minimal_value_specified()
        {
            var xmlConfig = XElement.Parse("<WarningLevel enabled=\"true\"/>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_parameter_is_float()
        {
            var xmlConfig = XElement.Parse("<WarningLevel><MinimalValue>4.2</MinimalValue></WarningLevel>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_no_project_specified_in_exceptions()
        {
            var xmlConfig = XElement.Parse(@"
<WarningLevel>
  <MinimalValue>4</MinimalValue>
  <Exception>
    <Project>WarningLevelTwoInAllConfigurations.csproj</Project>
    <MinimalValue>1</MinimalValue>
  </Exception>
  <Exception>
    <MinimalValue>1</MinimalValue>
  </Exception>
</WarningLevel>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, xmlConfig);
        }
    }
}