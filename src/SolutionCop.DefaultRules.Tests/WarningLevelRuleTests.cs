using System.IO;
using System.Linq;
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
    public class WarningLevelRuleTests
    {
        private readonly WarningLevelRule _instance;

        public WarningLevelRuleTests()
        {
            _instance = new WarningLevelRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_as_expected()
        {
            const string config = "<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_as_expected_in_global_section()
        {
            const string config = "<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInGlobalConfiguration.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_project_file_not_found()
        {
            const string config = "<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations-DoesNotExist.csproj").FullName);
            errors.ShouldNotBeEmpty();
            errors.First().ShouldStartWith("Project file not found:");
            errors.First().ShouldContain("WarningLevelTwoInAllConfigurations-DoesNotExist.csproj");
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_higher_than_expected()
        {
            const string config = "<WarningLevel><MinimalValue>0</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_lower_than_expected()
        {
            const string config = "<WarningLevel><MinimalValue>4</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_lower_than_expected_but_project_is_in_exceptions_list()
        {
            const string config = @"
<WarningLevel>
  <MinimalValue>4</MinimalValue>
  <Exception>
    <Project>WarningLevelTwoInAllConfigurations.csproj</Project>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProjectName.csproj</Project>
  </Exception>
</WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_lower_than_expected_but_project_have_exceptional_lower_value()
        {
            const string config = @"
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
</WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_higher_than_one_specified_in_exception()
        {
            const string config = @"
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
</WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_lower_than_expected_in_one_configuration()
        {
            const string config = "<WarningLevel><MinimalValue>3</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInOneConfigurationAndThreeInAnother.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_specified_in_one_configuration_only()
        {
            const string config = "<WarningLevel><MinimalValue>2</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInOneConfiguration.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<WarningLevel enabled=\"false\"><MinimalValue>4</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_without_minimal_value_specified_if_rule_disabled()
        {
            const string config = "<WarningLevel enabled=\"false\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        // Check bad configurations

        [Fact]
        public void Should_fail_if_no_minimal_value_specified()
        {
            const string config = "<WarningLevel enabled=\"true\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_parameter_is_float()
        {
            const string config = "<WarningLevel><MinimalValue>4.2</MinimalValue></WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_no_project_specified_in_exceptions()
        {
            const string config = @"
<WarningLevel>
  <MinimalValue>4</MinimalValue>
  <Exception>
    <Project>WarningLevelTwoInAllConfigurations.csproj</Project>
    <MinimalValue>1</MinimalValue>
  </Exception>
  <Exception>
    <MinimalValue>1</MinimalValue>
  </Exception>
</WarningLevel>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}