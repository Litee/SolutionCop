using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using SolutionCop.Core;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class NuGetPackagesUsageRuleTests : ProjectRuleTest
    {
        public NuGetPackagesUsageRuleTests() : base(new NuGetPackagesUsageRule())
        {
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(Instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_same_package_version_used_in_project()
        {
            var xmlConfig = XElement.Parse("<NuGetPackagesUsage/>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetPackagesUsage\UsesTwoPackages.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_there_is_an_unreferenced_package()
        {
            var xmlConfig = XElement.Parse("<NuGetPackagesUsage/>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_unreferenced_package_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackagesUsage>
  <Exception><Package>xunit</Package></Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</NuGetPackagesUsage>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackagesUsage>
  <Exception><Project>NonExistingProject.csproj</Project></Exception>
  <Exception><Project>UsesOnePackage.csproj</Project></Exception>
</NuGetPackagesUsage>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_project_is_an_exception_but_package_is_not()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackagesUsage>
  <Exception>
    <Project>UsesOnePackage.csproj</Project>
    <Package>someUnusedDummyPackage</Package>
  </Exception>
</NuGetPackagesUsage>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_package_is_an_exception_but_project_is_not()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackagesUsage>
  <Exception>
    <Project>NonExistingProject.csproj</Project>
    <Package>xunit</Package>
  </Exception>
</NuGetPackagesUsage>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project_and_package()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetPackagesUsage>
  <Exception>Some Text</Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</NuGetPackagesUsage>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<NuGetPackagesUsage enabled=\"false\"/>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\NuGetPackagesUsage_2\UsesOnePackage.csproj").FullName, xmlConfig);
        }
    }
}