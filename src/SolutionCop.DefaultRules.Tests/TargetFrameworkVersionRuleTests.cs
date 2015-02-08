using System.IO;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TargetFrameworkVersionRuleTests : ProjectRuleTest
    {

        public TargetFrameworkVersionRuleTests() : base(new TargetFrameworkVersionRule())
        {
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            Approvals.Verify(Instance.DefaultConfig);
        }

        [Fact]
        public void Should_accept_correct_target_version()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion>
  <FrameworkVersion>3.5</FrameworkVersion>
</TargetFrameworkVersion>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_accept_correct_target_versions()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <FrameworkVersion>3.5</FrameworkVersion>
</TargetFrameworkVersion>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_for_invalid_target_version()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
</TargetFrameworkVersion>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_for_invalid_target_version_but_project_in_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkVersion>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_for_invalid_target_version_but_project_and_version_are_exceptions()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
    <FrameworkVersion>3.5</FrameworkVersion>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkVersion>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_project_has_other_version_than_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
    <FrameworkVersion>4.5.1</FrameworkVersion>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkVersion>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion enabled='false'>
  <FrameworkVersion>4.0</FrameworkVersion>
  <FrameworkVersion>4.5</FrameworkVersion>
</TargetFrameworkVersion>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_rule_is_enabled_but_no_versions_specified()
        {
            var xmlConfig = XElement.Parse("<TargetFrameworkVersion enabled=\"true\"></TargetFrameworkVersion>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_exception_has_no_project_specified()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkVersion enabled='true'>
  <FrameworkVersion>4.0</FrameworkVersion>
  <Exception>
    <FrameworkVersion>4.5.1</FrameworkVersion>
  </Exception>
</TargetFrameworkVersion>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\TargetFrameworkVersion\TargetFramework3_5.csproj").FullName, xmlConfig);
        }
    }
}
