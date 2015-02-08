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
            const string config = @"
<TargetFrameworkVersion>
  <FrameworkVersion>3.5</FrameworkVersion>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_accept_correct_target_versions()
        {
            const string config = @"
<TargetFrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <FrameworkVersion>3.5</FrameworkVersion>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_for_invalid_target_version()
        {
            const string config = @"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_for_invalid_target_version_but_project_in_an_exception()
        {
            const string config = @"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_for_invalid_target_version_but_project_and_version_are_exceptions()
        {
            const string config = @"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
    <FrameworkVersion>3.5</FrameworkVersion>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_project_has_other_version_than_in_exception()
        {
            const string config = @"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
    <FrameworkVersion>4.5.1</FrameworkVersion>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_skip_disabled_rule()
        {
            const string config = @"
<TargetFrameworkVersion enabled='false'>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_rule_is_enabled_but_no_versions_specified()
        {
            const string config = "<TargetFrameworkVersion enabled=\"true\"></TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_exception_has_no_project_specified()
        {
            const string config = @"
<TargetFrameworkVersion enabled='true'>
  <FrameworkVersion>4.0</FrameworkVersion>
  <Exception>
    <FrameworkVersion>4.5.1</FrameworkVersion>
  </Exception>
</TargetFrameworkVersion>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}
