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
    public class NuGetPackageVersionsRuleTests
    {
        private readonly NuGetPackageVersionsRule _instance;

        public NuGetPackageVersionsRuleTests()
        {
            _instance = new NuGetPackageVersionsRule();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_all_used_packages_match_rules()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='0.0.0'></Package>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_no_packages_used()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='0.0.0'></Package>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions_2\UsesNoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_version_does_not_match_the_rule()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='[2.0]'></Package>
  <Package id='xunit' version='[1.9.2]'></Package>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_version_does_not_match_the_rule_but_project_is_an_exception()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='[2.0]'></Package>
  <Package id='xunit' version='[1.9.2]'></Package>
  <Exception>
    <Project>UsesTwoPackages.csproj</Project>
  </Exception>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='[2.0]'></Package>
  <Package id='xunit' version='[1.9.2]'></Package>
  <Exception>Some text</Exception>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_unknown_package_used()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='xunit' version='[1.9.2]'></Package>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_rule_has_bad_format()
        {
            const string config = @"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='test'></Package>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = @"
<NuGetPackageVersions enabled='false'>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='0.0.0'></Package>
</NuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageVersions_2\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}