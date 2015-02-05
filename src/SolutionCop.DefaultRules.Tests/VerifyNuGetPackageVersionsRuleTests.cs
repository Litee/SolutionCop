using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    public class VerifyNuGetPackageVersionsRuleTests
    {
        private readonly VerifyNuGetPackageVersionsRule _instance;

        public VerifyNuGetPackageVersionsRuleTests()
        {
            _instance = new VerifyNuGetPackageVersionsRule();
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
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='0.0.0'></Package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_all_used_packages_match_rules_lower_case()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <package id='ApprovalTests' version='0.0.0'></package>
    <package id='xunit' version='0.0.0'></package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_no_packages_used()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='0.0.0'></Package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions_2\UsesNoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_version_does_not_match_the_rule()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='[2.0]'></Package>
    <Package id='xunit' version='[1.9.2]'></Package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_unknown_package_used()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='xunit' version='[1.9.2]'></Package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_rule_has_bad_format()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='test'></Package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldNotBeEmpty();
            Approvals.VerifyAll(configErrors, "Errors");
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = @"
<VerifyNuGetPackageVersions enabled='false'>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='0.0.0'></Package>
</VerifyNuGetPackageVersions>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions_2\UsesTwoPackages.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}