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
    public class ReferenceNuGetPackagesOnlyRuleTests : ProjectRuleTest
    {
        public ReferenceNuGetPackagesOnlyRuleTests() : base(new ReferenceNuGetPackagesOnlyRule())
        {
        }

        [Fact]
        public void Should_accept_project_references_to_packages_only()
        {
            var xmlConfig = XElement.Parse("<ReferenceNuGetPackagesOnly/>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\ReferencesPackagesFolderOnly.csproj").FullName);
        }

        [Fact]
        public void Should_fail_for_project_with_direct_references_to_binaries()
        {
            var xmlConfig = XElement.Parse("<ReferenceNuGetPackagesOnly/>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_pass_for_project_with_direct_references_to_binaries_if_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>
    <Project>HasReferencesToLocalBinaries.csproj</Project>
  </Exception>
  <Exception>
    <Project>SomeAnotherProject.csproj</Project>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_pass_for_project_with_direct_references_to_binaries_if_file_name_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>
    <File>ApprovalTests.dll</File>
    <File>ApprovalUtilities.dll</File>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        // Should_fail_for_project_with_direct_references_to_binaries_if_file_name_is_an_exception_but_for_another_project
        [Fact]
        public void Should_fail_for_project_with_direct_references_to_binaries_if_file_name_is_an_exception_2()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>
    <Project>SomeAnotherProject.csproj</Project>
    <File>ApprovalTests.dll</File>
    <File>ApprovalUtilities.dll</File>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_pass_for_project_with_direct_references_to_binaries_if_file_name_is_an_exception_with_whitespaces()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>
    <File> ApprovalTests.dll</File>
    <File>ApprovalUtilities.dll </File>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_pass_for_project_with_direct_references_to_binaries_if_file_name_is_an_exception_case_insensitive()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>
    <File>approvaltests.dll</File>
    <File>APPROVALUTILITIES.DLL</File>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        // Should_fail_for_project_with_direct_references_to_binaries_if_one_of_file_names_is_not_an_exception
        [Fact]
        public void Should_fail_for_project_with_direct_references_to_binaries_if_one_of_file_names_()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>
    <File>ApprovalTests.dll</File>
    <File>SomeNonExistingName.dll</File>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Exception>Some text</Exception>
  <Exception>
    <Project>SomeAnotherProject.csproj</Project>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<ReferenceNuGetPackagesOnly>
  <Dummy>Some text</Dummy>
  <Exception>
    <Project>SomeAnotherProject.csproj</Project>
  </Exception>
</ReferenceNuGetPackagesOnly>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<ReferenceNuGetPackagesOnly enabled=\"false\"/>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnly\HasReferencesToLocalBinaries.csproj").FullName);
        }
    }
}