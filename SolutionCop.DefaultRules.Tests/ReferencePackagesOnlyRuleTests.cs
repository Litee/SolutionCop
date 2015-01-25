using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ReferencePackagesOnlyRuleTests
    {
        private readonly ReferencePackagesOnlyRule _instance;

        public ReferencePackagesOnlyRuleTests()
        {
            _instance = new ReferencePackagesOnlyRule();
        }

        [Fact]
        public void Should_accept_project_references_to_packages_only()
        {
            const string config = "<ReferencePackagesOnly/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\PackageRefsOnly.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_for_project_with_direct_references_to_binaries()
        {
            const string config = "<ReferencePackagesOnly/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\HasReferencesToLocalBinaries.csproj").FullName, XElement.Parse(config));
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_for_disabled_verification()
        {
            const string config = "<ReferencePackagesOnly enabled=\"false\"/>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\HasReferencesToLocalBinaries.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }
    }
}