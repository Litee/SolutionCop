msbuild ..\src\SolutionCop.sln /p:Configuration=Release

nuget pack SolutionCop.nuspec -Version %1
nuget pack SolutionCop.MSBuild.nuspec -Version %1