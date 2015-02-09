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
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(true);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_projects_use_different_versions()
        {
            var xmlConfig = XElement.Parse("<SameNuGetPackageVersions/>");
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(true);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult2.Errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_package_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception><Package>xunit</Package></Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</SameNuGetPackageVersions>");
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(true);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception><Project>NonExistingProject.csproj</Project></Exception>
  <Exception><Project>Project.csproj</Project></Exception>
</SameNuGetPackageVersions>");
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(true);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldBeEmpty();
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
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(true);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult2.Errors, "Errors");
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
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(true);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult2.Errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_exception_misses_project_and_package()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Exception>Some Text</Exception>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</SameNuGetPackageVersions>");
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(true);
            validationResult1.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult1.Errors, "Errors");
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<SameNuGetPackageVersions>
  <Dummy>Some Text</Dummy>
  <Exception><Package>someUnusedDummyPackage</Package></Exception>
</SameNuGetPackageVersions>");
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(true);
            validationResult1.HasErrorsInConfiguration.ShouldBe(true);
            validationResult1.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult1.Errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<SameNuGetPackageVersions enabled=\"false\"/>");
            var instance = Instance;
            var validationResult1 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionOne\Project.csproj").FullName, xmlConfig);
            validationResult1.IsEnabled.ShouldBe(false);
            validationResult1.HasErrorsInConfiguration.ShouldBe(false);
            validationResult1.Errors.ShouldBeEmpty();
            var validationResult2 = instance.ValidateProject(new FileInfo(@"..\..\Data\SameNuGetPackageVersions\UsesVersionTwo\Project.csproj").FullName, xmlConfig);
            validationResult2.IsEnabled.ShouldBe(false);
            validationResult2.HasErrorsInConfiguration.ShouldBe(false);
            validationResult2.Errors.ShouldBeEmpty();
        }
    }
}