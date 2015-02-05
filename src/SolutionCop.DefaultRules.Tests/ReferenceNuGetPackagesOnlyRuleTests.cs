using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Shouldly;
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
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(_instance.DefaultConfig);
        }

        [Fact]
        public void Should_accept_project_references_to_packages_only()
        {
            const string config = "<ReferenceNuGetPackagesOnlyRule/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnlyRule\ReferencesPackagesFolderOnly.csproj").FullName);
            errors.ShouldBeEmpty();
        }

        [Fact]
        public void Should_fail_for_project_with_direct_references_to_binaries()
        {
            const string config = "<ReferenceNuGetPackagesOnlyRule/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnlyRule\HasReferencesToLocalBinaries.csproj").FullName);
            errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(errors, "Errors");
        }

        [Fact]
        public void Should_pass_for_disabled_rule()
        {
            const string config = "<ReferenceNuGetPackagesOnlyRule enabled=\"false\"/>";
            var configErrors = _instance.ParseConfig(XElement.Parse(config));
            configErrors.ShouldBeEmpty();
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\ReferenceNuGetPackagesOnlyRule\HasReferencesToLocalBinaries.csproj").FullName);
            errors.ShouldBeEmpty();
        }
    }
}