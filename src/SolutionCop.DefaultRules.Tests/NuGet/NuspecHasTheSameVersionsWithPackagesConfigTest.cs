namespace SolutionCop.DefaultRules.Tests.NuGet
{
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.NuGet;
    using Shouldly;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public sealed class NuspecHasTheSameVersionsWithPackagesConfigTest : ProjectRuleTest
    {
        private static readonly DirectoryInfo TestDirectory = new DirectoryInfo("..\\..\\Data\\NuspecHasTheSameVersionsWithPackagesConfig\\");

        public NuspecHasTheSameVersionsWithPackagesConfigTest()
            : base(new NuspecHasTheSameVersionsWithPackagesConfig())
        {
        }

        [Fact]
        public void ShouldUnionVersionsFromDifferentProjects()
        {
            var testSubfolder = "MultiplePackages";
            var testFolder = TestDirectory.GetDirectories(testSubfolder).Single();

            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig enabled='true'>
    <Exception>
      <Package>incorrect-ignored-global-package</Package>
    </Exception>    
    <Exception>
      <Project>IgnoredProject.csproj</Project>
    </Exception>
  <Nuspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\MultiplePackages\project.nuspec</Path>
    <Exception>
      <Package>incorrect-ignored-local-package</Package>
    </Exception>    
    <Exception>
      <Project>SomeMissingProject.csproj</Project>
    </Exception>
  </Nuspec>  
</NuspecHasTheSameVersionsWithPackagesConfig>");

            var projects = testFolder.GetFiles("*.csproj", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();

            projects.Length.ShouldBe(3);

            ShouldFailNormally(xmlConfig, projects);
        }

        [Fact]
        public void ShouldFindMultipleNuspecPackagesByMask()
        {
            var testSubfolder = "MultipleNuspecFiles";
            var testFolder = TestDirectory.GetDirectories(testSubfolder).Single();

            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig enabled='true'>
  <Nuspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\MultipleNuspecFiles\*.nuspec</Path>
  </Nuspec>  
</NuspecHasTheSameVersionsWithPackagesConfig>");

            var projects = testFolder.GetFiles("*.csproj", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();

            projects.Length.ShouldBe(1);

            ShouldFailNormally(xmlConfig, projects);
        }

        [Fact]
        public void ShouldIgnoreNuspecWithoutDependencies()
        {
            var testSubfolder = "EmptyNuspec";
            var testFolder = TestDirectory.GetDirectories(testSubfolder).Single();

            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig enabled='true'>
    <Exception>
      <Package>incorrect-ignored-global-package</Package>
    </Exception>    
    <Exception>
      <Project>IgnoredProject.csproj</Project>
    </Exception>
  <Nuspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\EmptyNuspec\Empty.nuspec</Path>
  </Nuspec>  
</NuspecHasTheSameVersionsWithPackagesConfig>");

            var projects = testFolder.GetFiles("*.csproj", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();

            projects.Length.ShouldBe(1);

            ShouldPassNormally(xmlConfig, projects);
        }

        [Fact]
        public void ShouldFailForInvalidNuspecFilesLookupPatterns()
        {
            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig enabled='true'>
  <Nuspec>
        <Path></Path>
  </Nuspec>  
  <Nuspec/>  
  <Nuspec>
        <Path>ZZ:\</Path>
  </Nuspec>
  <Nuspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\NoFiles\*.nuspec</Path>
  </Nuspec>
  <Nuspec>
        <Path>MissingNuspecFile.nuspec</Path>
  </Nuspec>
</NuspecHasTheSameVersionsWithPackagesConfig>");

            ShouldFailNormally(xmlConfig);
        }

        [Fact]
        public void ShouldFailIfUnknownTagsAreExists()
        {
            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig enabled='true'>
  <UnknownTag>
        <Path></Path>
  </UnknownTag>
</NuspecHasTheSameVersionsWithPackagesConfig>");

            ShouldFailOnConfiguration(xmlConfig);
        }

        [Fact]
        public void ShouldNotBeExecutedForDisabledConfig()
        {
            var testSubfolder = "MultiplePackages";
            var testFolder = TestDirectory.GetDirectories(testSubfolder).Single();

            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig >
    <Exception>
      <Package>incorrect-ignored-global-package</Package>
    </Exception>    
    <Exception>
      <Project>IgnoredProject.csproj</Project>
    </Exception>
  <Nuspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\MultiplePackages\project.nuspec</Path>
    <Exception>
      <Package>incorrect-ignored-local-package</Package>
    </Exception>    
    <Exception>
      <Project>SomeMissingProject.csproj</Project>
    </Exception>
  </Nuspec>  
</NuspecHasTheSameVersionsWithPackagesConfig>");

            var projects = testFolder.GetFiles("*.csproj", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();

            projects.Length.ShouldBe(3);

            ShouldPassAsDisabled(xmlConfig, projects);
        }
    }
}