using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    public class AllowOnlySpecificNuGetPackagesRuleTests
    {
        private readonly AllowOnlySpecificNuGetPackagesRule _instance;

        public AllowOnlySpecificNuGetPackagesRuleTests()
        {
            _instance = new AllowOnlySpecificNuGetPackagesRule();
        }

        [Fact]
        public void Should_pass_if_only_allowed_packages_are_used()
        {
            const string config = @"
<AllowOnlySpecificNuGetPackages>
    <Package>ApprovalTests</Package>
    <Package>xunit</Package>
</AllowOnlySpecificNuGetPackages>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\AllowOnlySpecificNuGetPackages\AllowOnlySpecificNuGetPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_pass_if_no_packages_used()
        {
            const string config = @"
<AllowOnlySpecificNuGetPackages>
    <Package>ApprovalTests</Package>
    <Package>xunit</Package>
</AllowOnlySpecificNuGetPackages>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\AllowOnlySpecificNuGetPackages_2\AllowOnlySpecificNuGetPackages.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_if_unapproved_package_used()
        {
            const string config = @"
<AllowOnlySpecificNuGetPackages>
    <Package>ApprovalTests</Package>
</AllowOnlySpecificNuGetPackages>";
            var errors = _instance.ValidateProject(new FileInfo(@"..\..\Data\AllowOnlySpecificNuGetPackages\AllowOnlySpecificNuGetPackages.csproj").FullName, XElement.Parse(config));
            Approvals.VerifyAll(errors, "Errors");
        }
    }
}