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
<NuspecHasTheSameVersionsWithPackagesConfig>
  <ExcludePackagesOfProject projectName='IgnoredProject.csproj'/>  
  <ExcludePackageId packageId='incorrect-ignored-global-package'/>
  <Nupspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\MultiplePackages\project.nuspec</Path>
        <ExcludePackagesOfProject projectName='SomeMissingProject.csproj'/>  
        <ExcludePackageId packageId='incorrect-ignored-local-package'/>
  </Nupspec>  
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
<NuspecHasTheSameVersionsWithPackagesConfig>
  <Nupspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\MultipleNuspecFiles\*.nuspec</Path>
  </Nupspec>  
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
<NuspecHasTheSameVersionsWithPackagesConfig>
  <ExcludePackagesOfProject projectName='IgnoredProject.csproj'/>  
  <ExcludePackageId packageId='incorrect-ignored-global-package'/>
  <Nupspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\EmptyNuspec\Empty.nuspec</Path>
  </Nupspec>  
</NuspecHasTheSameVersionsWithPackagesConfig>");

            var projects = testFolder.GetFiles("*.csproj", SearchOption.AllDirectories).Select(f => f.FullName).ToArray();

            projects.Length.ShouldBe(1);

            ShouldPassNormally(xmlConfig, projects);
        }

        [Fact]
        public void ShouldFailForInvalidNuspecFilesLookupPatterns()
        {
            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig>
  <Nupspec>
        <Path></Path>
  </Nupspec>  
  <Nupspec/>  
  <Nupspec>
        <Path>ZZ:\</Path>
  </Nupspec>
  <Nupspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\NoFiles\*.nuspec</Path>
  </Nupspec>
  <Nupspec>
        <Path>MissingNuspecFile.nuspec</Path>
  </Nupspec>
</NuspecHasTheSameVersionsWithPackagesConfig>");

            ShouldFailNormally(xmlConfig);
        }
    }
}