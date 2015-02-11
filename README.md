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
* Forbid pre-release versions of packages (should be added to NuGetPackageVersions rule)
* Unapproved build configurations
* Binary within NuGet package is referenced directly without proper reference in packages.config
* Check for "Copy Local" used
* Assembly and root namespace should have same name
* Classify project by type (e.g. production, testing) and disallow references between some groups
* Proper owner in AssemblyInfo (FxCop?)
* Proper copyright date in AssemblyInfo (FxCop?)
* Check VS solution version

## TODO (other)
* Group errors in output by project or by rule
* Provide NuGet commands in log for upgrading to the lowest version
* Option to fail on missing sections instead of creating them
* Provide links to rule details in output
* Treat each rule as a separate test suite for TeamCity (not sure whether it will work better than current plain list)
* Allow to search custom rules in other folders
* Support build configurations as exception
* Move error messages to resources