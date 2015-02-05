using System.IO;
using System.Linq;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
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
            const string config = "<WarningLevel minimalValue='2'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_project_file_not_found()
        {
            const string config = "<WarningLevel minimalValue='2'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations-DoesNotExist.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            errors.First().ShouldStartWith("Project file not found:");
            errors.First().ShouldContain("WarningLevelTwoInAllConfigurations-DoesNotExist.csproj");
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_higher_than_expected()
        {
            const string config = "<WarningLevel minimalValue='0'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_lower_than_expected()
        {
            const string config = "<WarningLevel minimalValue='4'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_warning_level_in_project_is_lower_than_expected_but_project_is_in_exceptions_list()
        {
            const string config = @"
<WarningLevel minimalValue='4'>
  <Exception>WarningLevelTwoInAllConfigurations.csproj</Exception>
  <Exception>SomeNonExistingProjectName.csproj</Exception>
</WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_lower_than_expected_in_one_configuration()
        {
            const string config = "<WarningLevel minimalValue='3'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInOneConfigurationAndThreeInAnother.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_warning_level_in_project_is_specified_in_one_configuration_only()
        {
            const string config = "<WarningLevel minimalValue='2'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInOneConfiguration.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<WarningLevel enabled=\"false\" minimalValue='4'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_wihouth_minimal_value_specified_if_rule_isabled()
        {
            const string config = "<WarningLevel enabled=\"false\"/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_no_minimal_value_specified()
        {
            const string config = "<WarningLevel enabled=\"true\"/>";
            var errors = _instance.ValidateConfig(XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_parameter_is_float()
        {
            const string config = "<WarningLevel minimalValue='4.2'></WarningLevel>";
            var errors = _instance.ValidateConfig(XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}