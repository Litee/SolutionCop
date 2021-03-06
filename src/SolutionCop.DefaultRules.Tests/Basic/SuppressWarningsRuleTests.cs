﻿namespace SolutionCop.DefaultRules.Tests.Basic
{
    using System.IO;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.Basic;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class SuppressWarningsRuleTests : ProjectRuleTest
    {
        public SuppressWarningsRuleTests()
            : base(new SuppressWarningsRule())
        {
        }

        [Fact]
        public void Should_pass_with_neither_warning_suppressed()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_pass_with_neither_warning_suppressed_empty_list()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings_EmptyList.csproj").FullName);
        }

        [Fact]
        public void Should_pass_with_no_warnings_suppressed()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_pass_with_no_warnings_suppressed_empty_list()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressNoWarnings_EmptyList.csproj").FullName);
        }

        [Fact]
        public void Should_pass_with_same_warnings_suppressed()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings><Warning>0420</Warning><Warning>0465</Warning></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_pass_with_warnings_in_different_order()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings><Warning>0465</Warning><Warning>0420</Warning></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_pass_with_subset_of_warnings_suppressed()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings><Warning>0465</Warning><Warning>0420</Warning></SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressOneWarning.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings enabled=\"false\"><Warning>0465</Warning></SuppressWarnings>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings></SuppressWarnings>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_two_unapproved_warnings_suppressed_but_project_is_an_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SuppressWarnings>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception><Project>SuppressTwoWarnings.csproj</Project></Exception>
</SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_two_unapproved_warnings_suppressed_but_they_are_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SuppressWarnings>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception>
    <Project>SuppressTwoWarnings.csproj</Project>
    <Warning>0420</Warning>
    <Warning>0465</Warning>
  </Exception>
</SuppressWarnings>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_but_only_one_is_in_exception()
        {
            var xmlConfig = XElement.Parse(@"
<SuppressWarnings>
  <Exception><Project>SomeNonExistingProject.csproj</Project></Exception>
  <Exception>
    <Project>SuppressTwoWarnings.csproj</Project>
    <Warning>0465</Warning>
  </Exception>
</SuppressWarnings>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_and_they_are_exceptions_for_another_project()
        {
            var xmlConfig = XElement.Parse(@"
<SuppressWarnings>
  <Exception>
    <Project>SomeNonExistingProject.csproj</Project>
    <Warning>0420</Warning>
    <Warning>0465</Warning>
  </Exception>
</SuppressWarnings>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_misses_project()
        {
            var xmlConfig = XElement.Parse(@"
<SuppressWarnings>
  <Exception>Some text</Exception>
  <Exception><Project>SuppressTwoWarnings.csproj</Project></Exception>
</SuppressWarnings>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse(@"
<SuppressWarnings>
  <Dummy>Some text</Dummy>
  <Exception><Project>SuppressTwoWarnings.csproj</Project></Exception>
</SuppressWarnings>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_two_unapproved_warnings_suppressed_in_one_config_only()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings></SuppressWarnings>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarningsInOneConfig.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_one_unapproved_warning_suppressed()
        {
            var xmlConfig = XElement.Parse("<SuppressWarnings><Warning>0465</Warning></SuppressWarnings>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SuppressWarnings\SuppressTwoWarnings.csproj").FullName);
        }
    }
}