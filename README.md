# SolutionCop
Tool for static analysis of Visual Studio solutions

# Supported Rules
## Verify target version of the .NET Framework

Description: Checks <TargetFrameworkVersion> property in *.csproj files and fails if incompatible value was found

Example parameters:
    <TargetFrameworkVersion enabled="true">4.5</TargetFrameworkVersion>
    <TargetFrameworkVersion enabled="false"/>

## Should reference binaries only in NuGet packages

Description: Looks into all <HintPath> values and fails if path doesn't points to NuGet package

Example parameters:
    <ReferencePackagesOnly/>
    <ReferencePackagesOnly enabled="false"/>


TODO:
// Copy Local
// No duplicate packages
// Package binary referenced, but package is not used
// Same version for packages
// Solution version
// Package versions
// StyleCop used
// Treat warnings as errors
// Suppress specific warnings
// New NuGet initialization