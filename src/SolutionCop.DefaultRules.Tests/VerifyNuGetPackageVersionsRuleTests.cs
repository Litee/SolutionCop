using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
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
        public void Should_pass_if_all_used_packages_match_rules()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='0.0.0'></Package>
</VerifyNuGetPackageVersions>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_all_used_packages_match_rules_lower_case()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <package id='ApprovalTests' version='0.0.0'></package>
    <package id='xunit' version='0.0.0'></package>
</VerifyNuGetPackageVersions>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_all_used_packages_match_rules_with_external_rules()
        {
            const string config = @"<VerifyNuGetPackageVersions externalRules='.\VerifyNuGetPackageVersions\VersionRules.xml' />";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_no_packages_used()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='0.0.0'></Package>
</VerifyNuGetPackageVersions>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions_2\UsesNoPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_version_does_not_match_the_rule()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='ApprovalTests' version='[2.0]'></Package>
    <Package id='xunit' version='[1.9.2]'></Package>
</VerifyNuGetPackageVersions>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_unknown_package_used()
        {
            const string config = @"
<VerifyNuGetPackageVersions>
    <Package id='xunit' version='[1.9.2]'></Package>
</VerifyNuGetPackageVersions>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
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
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = @"
<VerifyNuGetPackageVersions enabled='false'>
    <Package id='ApprovalTests' version='0.0.0'></Package>
    <Package id='xunit' version='0.0.0'></Package>
</VerifyNuGetPackageVersions>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\VerifyNuGetPackageVersions_2\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }
    }
}