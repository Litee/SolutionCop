using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TargetFrameworkVersionRuleTests
    {
        private readonly TargetFrameworkVersionRule _instance;

        public TargetFrameworkVersionRuleTests()
        {
            _instance = new TargetFrameworkVersionRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_accept_correct_target_version()
        {
            const string config = "<TargetFrameworkVersion><AllowedValue>3.5</AllowedValue></TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_accept_correct_target_versions()
        {
            const string config = "<TargetFrameworkVersion><AllowedValue>4.5</AllowedValue><AllowedValue>3.5</AllowedValue></TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_for_invalid_target_version()
        {
            const string config = "<TargetFrameworkVersion><AllowedValue>4.5</AllowedValue></TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_skip_disabled_rule()
        {
            const string config = "<TargetFrameworkVersion enabled=\"false\"></TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_rule_is_enabled_but_no_allowed_versions_specified()
        {
            const string config = "<TargetFrameworkVersion enabled=\"true\"></TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}
