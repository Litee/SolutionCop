﻿using System.IO;
using System.Xml.Linq;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using SolutionCop.DefaultRules.NuGet;
using Xunit;

namespace SolutionCop.DefaultRules.Tests.NuGet
{
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