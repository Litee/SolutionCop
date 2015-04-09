set version=0.3.1

msbuild ..\src\SolutionCop.sln /p:Configuration=Release

nuget pack SolutionCop.nuspec -Version %1