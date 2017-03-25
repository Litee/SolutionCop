namespace SolutionCop.DefaultRules.Tests.NuGet
{
    using System.IO;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.NuGet;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class NuGetMandatoryPackageRuleTests : ProjectRuleTest
    {
        public NuGetMandatoryPackageRuleTests()
            : base(new NuGetMandatoryPackageRule())
        {
        }

        [Fact]
        public void Should_pass_if_mandatory_package_exists()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage>
  <Package id='ApprovalTests'></Package>
</NuGetMandatoryPackage>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_1\Project.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_mandatory_package_is_not_used()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage>
  <Package id='MissingPackage'></Package>
</NuGetMandatoryPackage>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_1\Project.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_no_packages_used_at_all()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage>
  <Package id='ApprovalTests'></Package>
</NuGetMandatoryPackage>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_2\UsesNoPackages.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_version_does_not_match_the_rule_but_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage>
  <Package id='MissingPackage'></Package>
  <Exception>
    <Project>Project.csproj</Project>
  </Exception>
</NuGetMandatoryPackage>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_1\Project.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_project_is_missing_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage>
  <Package id='ApprovalTests'></Package>
  <Exception>Some text</Exception>
</NuGetMandatoryPackage>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_1\Project.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage>
  <Dummy></Dummy>
  <Package id='ApprovalTests'></Package>
</NuGetMandatoryPackage>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_1\Project.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetMandatoryPackage enabled='false'>
  <Package id='MissingPackage'></Package>
</NuGetMandatoryPackage>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\NuGetMandatoryPackage_1\Project.csproj").FullName);
        }
    }
}