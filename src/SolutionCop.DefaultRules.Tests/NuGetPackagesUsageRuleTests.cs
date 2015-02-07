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
    public class NuGetPackagesUsageRuleTests
    {
        private readonly NuGetPackagesUsageRule _instance;

        public NuGetPackagesUsageRuleTests()
        {
            _instance = new NuGetPackagesUsageRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_same_package_version_used_in_project()
        {
            const string config = "<NuGetPackagesUsage/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_there_is_an_unreferenced_package()
        {
            const string config = "<NuGetPackagesUsage/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_unreferenced_package_is_an_exception()
        {
            const string config = @"
<NuGetPackagesUsage>
  <Exception><Package>xunit</Package></Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</NuGetPackagesUsage>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_project_is_an_exception()
        {
            const string config = @"
<NuGetPackagesUsage>
  <Exception><Project>NonExistingProject.csproj</Project></Exception>
  <Exception><Project>UsesOnePackage.csproj</Project></Exception>
</NuGetPackagesUsage>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_project_is_an_exception_but_package_is_not()
        {
            const string config = @"
<NuGetPackagesUsage>
  <Exception>
    <Project>UsesOnePackage.csproj</Project>
    <Package>someUnusedDummyPackage</Package>
  </Exception>
</NuGetPackagesUsage>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_package_is_an_exception_but_project_is_not()
        {
            const string config = @"
<NuGetPackagesUsage>
  <Exception>
    <Project>NonExistingProject.csproj</Project>
    <Package>xunit</Package>
  </Exception>
</NuGetPackagesUsage>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_exception_misses_project_and_package()
        {
            const string config = @"
<NuGetPackagesUsage>
  <Exception>Some Text</Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</NuGetPackagesUsage>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<NuGetPackagesUsage enabled=\"false\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}