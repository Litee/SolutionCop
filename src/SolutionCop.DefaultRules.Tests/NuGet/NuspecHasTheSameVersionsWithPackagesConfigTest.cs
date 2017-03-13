namespace SolutionCop.DefaultRules.Tests.NuGet
{
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.NuGet;
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
        public void ShouldNotFailIfNoNuspecAndNoProjectsFilesWereFoundByMask()
        {
            var xmlConfig = XElement.Parse(@"
<NuspecHasTheSameVersionsWithPackagesConfig>
  <Nupspec><Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\NoFiles\*.nuspec</Path></Nupspec>  
  <ExcludePackagesOfProject projectName='SomeMissingProject.csproj'/>  
  <ExcludePackageId packageId='ApprovalTests'/>
  <Nupspec>
        <Path>..\..\Data\NuspecHasTheSameVersionsWithPackagesConfig\NoFiles\*.nuspec</Path>
        <ExcludePackagesOfProject projectName='SomeMissingProject.csproj'/>  
        <ExcludePackageId packageId='ApprovalTests'/>
  </Nupspec>  
</NuspecHasTheSameVersionsWithPackagesConfig>");

            ShouldPassNormally(xmlConfig, "..\\..\\Data\\NuspecHasTheSameVersionsWithPackagesConfig\\NoFiles\\missingProject.csproj");
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

            ShouldFailNormally(xmlConfig, projects);
        }
    }
}