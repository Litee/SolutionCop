using System.IO;
using System.Xml.Linq;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof (DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class NuGetAutomaticPackagesRestoreRuleTests :ProjectRuleTest
    {
        public NuGetAutomaticPackagesRestoreRuleTests()
            : base(new NuGetAutomaticPackagesRestoreRule())
        {
        }

        [Fact]
        public void Should_pass_if_NuGet_targets_file_is_not_referenced()
        {
            var xmlConfig = XElement.Parse("<NuGetAutomaticPackagesRestore/>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_old_restore_mode_is_used_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetAutomaticPackagesRestore>
  <Exception><Project>NoNuGet.csproj</Project></Exception>
</NuGetAutomaticPackagesRestore>");
            ShouldPassNormally(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_old_restore_mode_is_used()
        {
            var xmlConfig = XElement.Parse("<NuGetAutomaticPackagesRestore enabled=\"true\"/>");
            ShouldFailNormally(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\OldNuGetRestoreMode.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_fail_if_exception_does_not_have_project_specified()
        {
            var xmlConfig = XElement.Parse(@"
<NuGetAutomaticPackagesRestore>
  <Exception>Some text</Exception>
</NuGetAutomaticPackagesRestore>");
            ShouldFailOnConfiguration(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName, xmlConfig);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<NuGetAutomaticPackagesRestore enabled=\"false\"/>");
            ShouldPassAsDisabled(new FileInfo(@"..\..\Data\NuGetAutomaticPackagesRestoreRule\NoNuGet.csproj").FullName, xmlConfig);
        }
    }
}