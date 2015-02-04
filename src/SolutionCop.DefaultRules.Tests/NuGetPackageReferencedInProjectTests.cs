using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    public class NuGetPackageReferencedInProjectTests
    {
        private readonly NuGetPackageReferencedInProject _instance;

        public NuGetPackageReferencedInProjectTests()
        {
            _instance = new NuGetPackageReferencedInProject();
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_pass_if_same_package_version_used_in_project()
        {
            const string config = "<NuGetPackageReferencedInProject/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageReferencedInProject\UsesTwoPackages.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_if_there_is_an_unreferenced_package()
        {
            const string config = "<NuGetPackageReferencedInProject/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageReferencedInProject_2\UsesOnePackage.csproj").FullName, XElement.Parse(config));
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_if_unreferenced_package_is_an_exception()
        {
            const string config = @"<NuGetPackageReferencedInProject>
<Exceptions>
<Exception>xunit</Exception>
<Exception>someUnusedDummyPackage</Exception>
</Exceptions>
</NuGetPackageReferencedInProject>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageReferencedInProject_2\UsesOnePackage.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            const string config = "<NuGetPackageReferencedInProject enabled=\"false\"/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\NuGetPackageReferencedInProject_2\UsesOnePackage.csproj").FullName, XElement.Parse(config));
            errors.ShouldBeEmpty();
        }
    }
}