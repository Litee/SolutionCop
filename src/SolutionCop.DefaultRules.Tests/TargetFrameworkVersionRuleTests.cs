using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class TargetFrameworkVersionRuleTests
    {
        private readonly TargetFrameworkVersionRule _instance;

        public TargetFrameworkVersionRuleTests()
        {
            _instance = new TargetFrameworkVersionRule();
        }

        [Fact]
        public void Should_accept_correct_target_version()
        {
            const string config = "<TargetFrameworkVersion>3.5</TargetFrameworkVersion>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_for_invalid_target_version()
        {
            const string config = "<TargetFrameworkVersion>4.5</TargetFrameworkVersion>";
            Approvals.VerifyAll(_instance.Validate(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, XElement.Parse(config)), "Errors");
        }

        [Fact]
        public void Should_skip_disabled_rule()
        {
            const string config = "<TargetFrameworkVersion enabled=\"false\"></TargetFrameworkVersion>";
            var errors = _instance.Validate(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, XElement.Parse(config));
            Assert.Empty(errors);
        }
    }
}
