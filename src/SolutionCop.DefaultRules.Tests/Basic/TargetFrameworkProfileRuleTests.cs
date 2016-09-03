namespace SolutionCop.DefaultRules.Tests.Basic
{
    using System.IO;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.Basic;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class TargetFrameworkProfileRuleTests : ProjectRuleTest
    {
        public TargetFrameworkProfileRuleTests()
            : base(new TargetFrameworkProfileRule())
        {
        }

        [Fact]
        public void Should_accept_correct_target_profile()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile>
  <Profile>Client</Profile>
</TargetFrameworkProfile>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileClient.csproj").FullName);
        }

        [Fact]
        public void Should_accept_correct_target_profile_2()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile>
  <Profile></Profile>
</TargetFrameworkProfile>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileEmpty.csproj").FullName);
        }

        [Fact]
        public void Should_fail_for_invalid_target_profile()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile>
  <Profile>Client</Profile>
</TargetFrameworkProfile>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileEmpty.csproj").FullName);
        }

        [Fact]
        public void Should_pass_for_invalid_target_profile_but_project_in_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile>
  <Profile>Client</Profile>
  <Exception>
    <Project>TargetProfileEmpty.csproj</Project>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkProfile>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileEmpty.csproj").FullName);
        }

        [Fact]
        public void Should_pass_for_invalid_target_profile_but_project_and_profile_are_exceptions()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile>
  <Profile>Dummy</Profile>
  <Exception>
    <Project>TargetProfileClient.csproj</Project>
    <Profile>Client</Profile>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkProfile>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileClient.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_project_has_other_profile_than_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile>
  <Profile>Dummy</Profile>
  <Exception>
    <Project>TargetFramework3_5.csproj</Project>
    <Profile>AnotherProfile</Profile>
  </Exception>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
  </Exception>
</TargetFrameworkProfile>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileClient.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile enabled='false'>
  <Profile>Client</Profile>
</TargetFrameworkProfile>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileEmpty.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_rule_is_enabled_but_no_profiles_specified()
        {
            var xmlConfig = XElement.Parse("<TargetFrameworkProfile enabled='true'></TargetFrameworkProfile>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileEmpty.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_has_no_project_specified()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile enabled='true'>
  <Profile>Client</Profile>
  <Exception>
    <Profile>Dummy</Profile>
  </Exception>
</TargetFrameworkProfile>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileClient.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_is_empty()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile enabled='true'>
  <Profile>Client</Profile>
  <Exception>
  </Exception>
</TargetFrameworkProfile>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileClient.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<TargetFrameworkProfile enabled='true'>
  <Profile>Client</Profile>
  <Dummy/>
  <Exception>
    <Project>TargetProfileClient.csproj</Project>
    <Profile></Profile>
  </Exception>
</TargetFrameworkProfile>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\TargetFrameworkProfile\TargetProfileClient.csproj").FullName);
        }
    }
}