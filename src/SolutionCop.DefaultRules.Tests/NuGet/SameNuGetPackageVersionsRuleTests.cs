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
    public class SameNuGetPackageVersionsRuleTests : ProjectRuleTest
    {
        public SameNuGetPackageVersionsRuleTests()
            : base(new SameNuGetPackageVersionsRule())
        {
        }

        [Fact]
        public void Should_pass_if_same_package_versions_used_in_project()
        {
            var xmlConfig = XElement.Parse("<SameNuGetPackageVersions/>");
            ShouldPassNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_projects_use_different_versions()
        {
            var xmlConfig = XElement.Parse("<SameNuGetPackageVersions/>");
            ShouldFailNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\ProjectTwo.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_projects_has_duplicate_versions()
        {
            var xmlConfig = XElement.Parse("<SameNuGetPackageVersions/>");
            ShouldFailNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\DuplicateIds\ProjectThree.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_package_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception><Package>xunit</Package></Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</SameNuGetPackageVersions>");
            ShouldPassNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\ProjectTwo.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception><Project>NonExistingProject.csproj</Project></Exception>
  <Exception><Project>ProjectOne.csproj</Project></Exception>
</SameNuGetPackageVersions>");
            ShouldPassNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\ProjectTwo.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_project_is_an_exception_but_package_is_not()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception>
    <Project>Project.csproj</Project>
    <Package>someUnusedDummyPackage</Package>
  </Exception>
</SameNuGetPackageVersions>");
            ShouldFailNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\ProjectTwo.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_package_is_an_exception_but_project_is_not()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception>
    <Project>NonExistingProject.csproj</Project>
    <Package>xunit</Package>
  </Exception>
</SameNuGetPackageVersions>");
            ShouldFailNormally(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\ProjectTwo.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project_and_package()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception>Some Text</Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</SameNuGetPackageVersions>");
            ShouldFailOnConfiguration(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Dummy>Some Text</Dummy>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</SameNuGetPackageVersions>");
            ShouldFailOnConfiguration(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<SameNuGetPackageVersions enabled=\"false\"/>");
            ShouldPassAsDisabled(
                xmlConfig,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\ProjectOne.csproj").FullName,
                new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\ProjectOne.csproj").FullName);
        }
    }
}