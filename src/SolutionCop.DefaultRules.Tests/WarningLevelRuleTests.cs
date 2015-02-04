using System.IO;
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
            const string config = @"<WarningLevel>
<MinimalValue>4</MinimalValue>
<Exceptions>
<Exception>WarningLevelTwoInAllConfigurations.csproj</Exception>
<Exception>SomeNonExistingProjectName.csproj</Exception>
</Exceptions>
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
        public void Should_fail_if_parameter_is_float()
        {
            const string config = "<WarningLevel minimalValue='4.2'></WarningLevel>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\WarningLevel\WarningLevelTwoInAllConfigurations.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}