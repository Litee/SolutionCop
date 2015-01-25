# SolutionCop
Tool for static analysis of Visual Studio solutions. 

TODO command-line parameters description

## Supported Rules

### Verify target version of the .NET Framework

Description: Checks <TargetFrameworkVersion> property in *.csproj files and fails if incompatible value was found

Id: TargetFrameworkVersion

Example parameter:

    <TargetFrameworkVersion enabled="true">4.5</TargetFrameworkVersion>

### Should reference binaries only in NuGet packages

Description: Looks into all <HintPath> values and fails if path doesn't points to NuGet package

Id: ReferencePackagesOnly

### Verify that all warnings are treated as errors

Description: Fails if "Treat warnings as errors" is not enabled in all build configurations

Id: TreatWarningsAsErrors

### Verify that only approved warnings are suppressed

Description: Fails if project suppresses warning that is not in the white list

Id: SuppressOnlySpecificWarnings

### TODO rules:
* Copy Local
* No duplicate NuGet packages
* Package binary referenced, but package is not used
* Same package versions are used for in project (support exceptions)
* VS solution version
* NuGet package versions should match specific ranges
* StyleCop enabled for all projects
* New NuGet initialization approach
* Same name for Assembly and root namespace