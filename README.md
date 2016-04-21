# SolutionCop and SolutionCop.MSBuild overview

SolutionCop is a tool for analyzing Visual Studio solutions and projects. It covers the gap between [FxCop](https://msdn.microsoft.com/en-us/library/bb429476(v=vs.80).aspx) (static analysis of assemblies) and [StyleCop](http://stylecop.codeplex.com/) (static analysis of source code) and allows to do checks like lost files or unused NuGet packages. This tool is successfully used in several big .NET projects and saved me a lot of time. 

SolutionCop.MSBuild allows to run SolutionCop checks during project compilation and detect issues earlier.

## Supported rules

* Basic rules
  * [TargetFrameworkVersion](https://github.com/Litee/SolutionCop/wiki/TargetFrameworkVersion)
  * [TreatWarningsAsErrors](https://github.com/Litee/SolutionCop/wiki/TreatWarningsAsErrors)
  * [SuppressWarnings](https://github.com/Litee/SolutionCop/wiki/SuppressWarnings)
  * [WarningLevel](https://github.com/Litee/SolutionCop/wiki/WarningLevel)
  * [FilesIncludedIntoProject](https://github.com/Litee/SolutionCop/wiki/FilesIncludedIntoProject) - very useful rule for finding "lost" files. Very typical example is unit test that was lost in .csproj file during some complex merge.
  * [TargetFrameworkProfile](https://github.com/Litee/SolutionCop/wiki/TargetFrameworkProfile)
  * [SameNameForAssemblyAndRootNamespace](https://github.com/Litee/SolutionCop/wiki/SameNameForAssemblyAndRootNamespace)
* NuGet-related rules
  * [NuGetAutomaticPackagesRestore](https://github.com/Litee/SolutionCop/wiki/NuGetAutomaticPackagesRestore)
  * [NuGetPackagesUsage](https://github.com/Litee/SolutionCop/wiki/NuGetPackagesUsage)
  * [ReferenceNuGetPackagesOnly](https://github.com/Litee/SolutionCop/wiki/ReferenceNuGetPackagesOnly) - very useful to detect cases when projects switch from NuGet binary to binaries in Bin/Debug
  * [NuGetPackageVersions](https://github.com/Litee/SolutionCop/wiki/NuGetPackageVersions) - super-useful if you have large project or set of projects and want to control all changes for external dependencies - all dependencies and versions are in a single file
  * [SameNuGetPackageVersions](https://github.com/Litee/SolutionCop/wiki/SameNuGetPackageVersions) - one more useful rule to avoid partial NuGet upgrades when only some  
* StyleCop-related rules
  * [StyleCopEnabled](https://github.com/Litee/SolutionCop/wiki/StyleCopEnabled)
  * [TreatStyleCopWarningsAsErrors](https://github.com/Litee/SolutionCop/wiki/TreatStyleCopWarningsAsErrors)

## How to use SolutionCop

Install it like this `NuGet.exe Install SolutionCop` or (for brave ones) like this `NuGet.exe Install SolutionCop -Prerelease`

```PowerShell
SolutionCop.exe -s MySolution.sln [-c SolutionCop.xml] [-b TeamCity] [--build-server-no-success-messages]
```

If you specify no configuration file then the tool will look for `SolutionCop.xml` file next to `*.sln`. If configuration file cannot be found, then tool will create a default one next to VS solution with **all rules disabled**. I recommend you not to enable all rules at once - instead turn them on one by one and fix identified issues after each new rule.

## How to use SolutionCop.MSBuild

Simply install **SolutionCop.MSBuild** NuGet package into every VS project you want to check and watch for errors in build output. Some notes:
* SolutionCop.MSBuild doesn't allow to specify path to config file - instead it looks for *SolutionCop.xml* file in parent folders starting with folder where .csproj file is defined. Once config file is found logic is the same as in SolutionCop.
* SameNuGetPackageVersions rule won't work in SolutionCop.MSBuild because tool processes each project separately, so has no chance to compare package versions between different projects.
* To see more detailed output select verbose MSBuild output level in VS settings

## TODO - Rules (in priority order)
* Check for "Copy Local" used
* Classify project by type (e.g. production, testing) and disallow references between some project types
* Proper owner in AssemblyInfo (can it be done via FxCop?)
* Proper copyright date in AssemblyInfo (can it be done via FxCop?)

## TODO - Other
* Option to fail on missing sections instead of creating them
* Provide links to rule details in output
* Move error messages to resources