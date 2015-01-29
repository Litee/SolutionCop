# SolutionCop
Tool for static analysis of Visual Studio solutions and projects. 

Usage: SolutionCop.exe -s <path-to-vs-solution> [-c <path-to-solutioncop-config>]

If <path-to-solutioncop-config> is not provided then tool looks for SolutionCop.xml file next to target *.sln. If config file cannot be found then default one is created.

## Supported Rules

### Verify target version of the .NET Framework

Description: Fails if one of projects has target version different from one specified

Id: TargetFrameworkVersion

### Verify that all referenced binaries come from NuGet packages

Description: Fails if project references binaries outside NuGet *packages* folder

Id: ReferenceNuGetPackagesOnlyRule

### Verify warnings treatment as errors

Description: Fails if "Treat warnings as errors" is not enabled in all build configurations or if not all specified warnings are treated as errors

Id: TreatWarningsAsErrors

### Verify suppressed warnings

Description: Fails if project suppresses any warning that is not in the specified white list

Id: SuppressWarnings

### Verify warning level

Description: Fails if any project has warning level lower than one specified

Id: WarningLevel

### Verify that NuGet package versions match rules

Description: Fails if some package in packages.config file has version that doesn't match rule or there is no rule for the package

Sample config section:
    <VerifyNuGetPackageVersions>
	<Package id="packageOne" version="*"/> <!-- Any version -->
	<Package id="packageTwo" version="1.2.3"/> <!-- 1.2.3 <= version -->
	<Package id="packageThree" version="(1.2-alpha, 1.99.99]"/> <!-- 1.2-alpha < version <= 1.99.99 -->
    </VerifyNuGetPackageVersions>

## Sample config file:
    <Rules>
      <TargetFrameworkVersion enabled="true">4.5</TargetFrameworkVersion>
      <ReferenceNuGetPackagesOnly enabled="true"/>
      <TreatWarningsAsErrors>All</TreatWarningsAsErrors>
      <!--<TreatWarningsAsErrors>0123,0234</TreatWarningsAsErrors>-->
      <SuppressWarnings>0123,0234</SuppressWarnings>
      <WarningLevel>4</WarningLevel>
    </Rules>

### TODO rules:
* Copy Local
* No duplicate NuGet packages
* Binary within NuGet package is referenced without proper reference in projects.config
* Same package versions are used for in project (support exceptions)
* VS solution version
* StyleCop must be enabled for all projects
* New NuGet initialization approach should be used
* Name must be same for Assembly and its root namespace
* Unapproved packages
* Unapproved build configurations