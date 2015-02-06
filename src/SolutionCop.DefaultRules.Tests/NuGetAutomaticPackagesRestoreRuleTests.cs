using System.IO;
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
    public class NuGetAutomaticPackagesRestoreRuleTests
    {
        private readonly NuGetAutomaticPackagesRestoreRule _instance;

        public NuGetAutomaticPackagesRestoreRuleTests()
        {
            _instance = new NuGetAutomaticPackagesRestoreRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_NuGet_targets_file_is_not_referenced()
        {
            const string config = "<NuGetAutomaticPackagesRestore/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<NuGetAutomaticPackagesRestore enabled=\"false\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_old_restore_mode_is_used_in_exception()
        {
            const string config = @"
<NuGetAutomaticPackagesRestore>
  <Exception>NoNuGet.csproj</Exception>
</NuGetAutomaticPackagesRestore>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_old_restore_mode_is_used()
        {
            const string config = "<NuGetAutomaticPackagesRestore enabled=\"true\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\OldNuGetRestoreMode.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}