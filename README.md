# SolutionCop
Tool for static analysis of Visual Studio solutions. 

TODO command-line parameters description

## Supported Rules

### Verify target version of the .NET Framework

Description: Checks <TargetFrameworkVersion> property in *.csproj files and fails if incompatible value was found

Id: TargetFrameworkVersion

Parameter example:

    <TargetFrameworkVersion enabled="true">4.5</TargetFrameworkVersion>

### Should reference binaries only in NuGet packages

Description: Fails if project references binaries outside NuGet *packages* folder

Id: ReferenceNuGetPackagesOnlyRule

### Verify that all or specific warnings are treated as errors

Description: Fails if "Treat warnings as errors" is not enabled in all build configurations or if not all specified warnings are treated as errors

Id: TreatWarningsAsErrors

Parameter example:

    <TreatWarningsAsErrors>All</TreatWarningsAsErrors>

    <TreatWarningsAsErrors>0123,0234</TreatWarningsAsErrors>

### Verify that unapproved warnings are not suppressed

Description: Fails if project suppresses warning that is not in the white list

Id: SuppressWarnings

Parameter example:

    <SuppressWarnings>0123,0234</SuppressWarnings>

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