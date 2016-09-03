namespace SolutionCop.DefaultRules.Tests.Basic
{
    using System.IO;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.NuGet;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class SameNameForAssemblyAndRootNamespaceRuleTests : ProjectRuleTest
    {
        public SameNameForAssemblyAndRootNamespaceRuleTests()
            : base(new SameNameForAssemblyAndRootNamespaceRule())
        {
        }

        [Fact]
        public void Should_pass_if_names_are_the_same()
        {
            var xmlConfig = XElement.Parse("<SameNameForAssemblyAndRootNamespace></SameNameForAssemblyAndRootNamespace>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\SameName.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_names_are_different()
        {
            var xmlConfig = XElement.Parse("<SameNameForAssemblyAndRootNamespace></SameNameForAssemblyAndRootNamespace>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\DifferentNames.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_names_are_different_but_project_is_in_exceptions_list()
        {
            var xmlConfig = XElement.Parse(@"
<SameNameForAssemblyAndRootNamespace>
  <Exception><Project>DifferentNames.csproj</Project></Exception>
  <Exception><Project>SomeNonExistingProjectName.csproj</Project></Exception>
</SameNameForAssemblyAndRootNamespace>");
            ShouldPassNormally(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\DifferentNames.csproj").FullName);
        }

        [Fact]
        public void Should_pass_if_rule_is_disabled()
        {
            var xmlConfig = XElement.Parse("<SameNameForAssemblyAndRootNamespace enabled=\"false\"></SameNameForAssemblyAndRootNamespace>");
            ShouldPassAsDisabled(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\DifferentNames.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_unknown_element_in_config()
        {
            var xmlConfig = XElement.Parse("<SameNameForAssemblyAndRootNamespace><Dummy/></SameNameForAssemblyAndRootNamespace>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\SameNames.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_exception_is_empty()
        {
            var xmlConfig = XElement.Parse("<SameNameForAssemblyAndRootNamespace><Exception/></SameNameForAssemblyAndRootNamespace>");
            ShouldFailOnConfiguration(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\SameNames.csproj").FullName);
        }

        [Fact]
        public void Should_fail_if_assemblyname_is_missing()
        {
            var xmlConfig = XElement.Parse("<SameNameForAssemblyAndRootNamespace></SameNameForAssemblyAndRootNamespace>");
            ShouldFailNormally(xmlConfig, new FileInfo(@"..\..\Data\SameNameForAssemblyAndRootNamespace\MissingAssemblyName.csproj").FullName);
        }
    }
}