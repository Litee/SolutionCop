using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using SolutionCop.Core;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class NuGetPackageVersionsRuleTests : ProjectRuleTest
    {
        public NuGetPackageVersionsRuleTests()
            : base(new NuGetPackageVersionsRule())
        {
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(Instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_all_used_packages_match_rules()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='0.0.0'></Package>
</NuGetPackageVersions>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_no_packages_used()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='0.0.0'></Package>
</NuGetPackageVersions>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetPackageVersions_2\UsesNoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_version_does_not_match_the_rule()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='[2.0]'></Package>
  <Package id='xunit' version='[1.9.2]'></Package>
</NuGetPackageVersions>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_version_does_not_match_the_rule_but_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='[2.0]'></Package>
  <Package id='xunit' version='[1.9.2]'></Package>
  <Exception>
    <Project>UsesTwoPackages.csproj</Project>
  </Exception>
</NuGetPackageVersions>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_project_is_missing_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='[2.0]'></Package>
  <Package id='xunit' version='[1.9.2]'></Package>
  <Exception>Some text</Exception>
</NuGetPackageVersions>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_unknown_package_used()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='xunit' version='[1.9.2]'></Package>
</NuGetPackageVersions>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_version_has_bad_format()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='test'></Package>
</NuGetPackageVersions>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\NuGetPackageVersions\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackageVersions enabled='false'>
  <Package id='ApprovalTests' version='0.0.0'></Package>
  <Package id='xunit' version='0.0.0'></Package>
</NuGetPackageVersions>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\NuGetPackageVersions_2\UsesTwoPackages.csproj").FullName, xmlConfig);
        }
    }
}