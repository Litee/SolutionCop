namespace SolutionCop.DefaultRules.Tests.NuGet
{
    using System.Xml.Linq;
    using ApprovalTests.Namers;
    using ApprovalTests.Reporters;
    using DefaultRules.NuGet;
    using Xunit;

    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public sealed class NuspecHasTheSameVersionsWithPackagesConfigTest : ProjectRuleTest
    {
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
    }
}