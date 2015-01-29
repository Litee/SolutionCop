using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ReferenceNuGetPackagesOnlyRuleTests
    {
        private readonly ReferenceNuGetPackagesOnlyRule _instance;

        public ReferenceNuGetPackagesOnlyRuleTests()
        {
            _instance = new ReferenceNuGetPackagesOnlyRule();
        }

        [Fact]
        public void Should_accept_project_references_to_packages_only()
        {
            const string config = "<ReferenceNuGetPackagesOnlyRule/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnlyRule\ReferencesPackagesFolderOnly.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_for_project_with_direct_references_to_binaries()
        {
            const string config = "<ReferenceNuGetPackagesOnlyRule/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnlyRule\HasReferencesToLocalBinaries.csproj").FullName, XElement.Parse(config));
            Assert.NotEmpty(errors);
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_for_disabled_rule()
        {
            const string config = "<ReferenceNuGetPackagesOnlyRule enabled=\"false\"/>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnlyRule\HasReferencesToLocalBinaries.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }
    }
}