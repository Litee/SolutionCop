# SolutionCop

SolutionCop is tool for analysing Visual Studio solutions and projects. It covers gap between FxCop (static analysis of binaries) and StyleCop (static analysis of isolated files) and allows to analyse solution/project structure and settings.

Usage: SolutionCop.exe -s MySolution.sln [-c SolutionCop.xml] [-b TeamCity]

If no configuration file is specified then tool will look for SolutionCop.xml file next to *.sln. If config file cannot be found then default one will be created next to VS solution with all rules disabled.

## Supported rules

* Basic
  * [TargetFrameworkVersion](https://github.com/Litee/SolutionCop/wiki/TargetFrameworkVersion)
  * [TreatWarningsAsErrors](https://github.com/Litee/SolutionCop/wiki/TreatWarningsAsErrors)
  * [SuppressWarnings](https://github.com/Litee/SolutionCop/wiki/SuppressWarnings)
  * [WarningLevel](https://github.com/Litee/SolutionCop/wiki/WarningLevel)
  * [FilesIncludedIntoProject](https://github.com/Litee/SolutionCop/wiki/FilesIncludedIntoProject)
  * TargetFrameworkProfile - TODO
  * SameNameForAssemblyAndRootNamespace - TODO
* NuGet
  * [NuGetAutomaticPackagesRestore](https://github.com/Litee/SolutionCop/wiki/NuGetAutomaticPackagesRestore)
  * [NuGetPackagesUsage](https://github.com/Litee/SolutionCop/wiki/NuGetPackagesUsage)
  * [ReferenceNuGetPackagesOnly](https://github.com/Litee/SolutionCop/wiki/ReferenceNuGetPackagesOnly)
  * [NuGetPackageVersions](https://github.com/Litee/SolutionCop/wiki/NuGetPackageVersions)
  * [SameNuGetPackageVersions](https://github.com/Litee/SolutionCop/wiki/SameNuGetPackageVersions)
* StyleCop
  * [StyleCopEnabled](https://github.com/Litee/SolutionCop/wiki/StyleCopEnabled)
  * [TreatStyleCopWarningsAsErrors](https://github.com/Litee/SolutionCop/wiki/TreatStyleCopWarningsAsErrors)

## How to get

NuGet.exe Install SolutionCop -Prerelease

## Compatibility

* Tool is using .NET 4.0

## TODO (rules in priority order)
* Check for "Copy Local" used
* Classify project by type (e.g. production, testing) and disallow references between some groups
* Proper owner in AssemblyInfo (can it be done via FxCop?)
* Proper copyright date in AssemblyInfo (can it be done via FxCop?)

## TODO (other)
* Option to fail on missing sections instead of creating them
* Provide links to rule details in output
* Treat each rule as a separate test suite for TeamCity (not sure whether it will work better than current plain list)
* Move error messages to resources
* XSD for config