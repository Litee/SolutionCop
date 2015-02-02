# SolutionCop

Tool for static analysis of Visual Studio solutions and projects. 

Usage: SolutionCop.exe -s *path-to-vs-solution* [-c *path-to-solutioncop-config*]

If <path-to-solutioncop-config> is not provided then tool looks for SolutionCop.xml file next to target *.sln. If config file cannot be found then default one is created.

## Config file

Below is an example config file. Any rule can be disabled by setting *enabled* attribute to *false*. Note that tool adds default entries for rules if they are missing.

    <Rules>
      <TargetFrameworkVersion>4.5</TargetFrameworkVersion>
      <ReferenceNuGetPackagesOnly enabled="true"/>
      <TreatWarningsAsErrors>All</TreatWarningsAsErrors>
      <SuppressWarnings>0123,0234</SuppressWarnings>
      <WarningLevel>4</WarningLevel>
    </Rules>

## Rules

### Verify target version of the .NET Framework

Description: Fails if one of projects has target version different from one specified

Sample config section:

    <TargetFrameworkVersion>4.5</TargetFrameworkVersion>

### Verify that all referenced binaries come from NuGet packages

Description: Fails if project references binaries outside NuGet *packages* folder

Sample config section:                                    

    <ReferenceNuGetPackagesOnly enabled="true"/>

### Verify warnings treatment as errors

Description: Fails if "Treat warnings as errors" is not enabled in all build configurations or if not all specified warnings are treated as errors

Sample config sections:

    <TreatWarningsAsErrors>All</TreatWarningsAsErrors>

or

    <TreatWarningsAsErrors>0123,0234</TreatWarningsAsErrors>

### Verify suppressed warnings

Description: Fails if project suppresses any warning that is not in the specified white list

Sample config section:

    <SuppressWarnings>0123,0234</SuppressWarnings>

### Verify warning level

Description: Fails if any project has warning level lower than one specified

Sample config section:

    <WarningLevel>4</WarningLevel>

### Verify that NuGet package versions match rules

Description: Fails if some package in packages.config file has version that doesn't match rule or there is no rule for the package

Sample config section:

    <VerifyNuGetPackageVersions>
      <Package id="packageOne" version="*"/> <!-- Any version -->
      <Package id="packageTwo" version="1.2.3"/> <!-- 1.2.3 <= version -->
      <Package id="packageThree" version="(1.2-alpha, 1.99.99]"/> <!-- 1.2-alpha < version <= 1.99.99 -->
    </VerifyNuGetPackageVersions>

### Verify that StyleCop is enabled for all projects

Description: Fails if some VS project does not import StyleCop.MSBuild.Targets

Sample config section:                                    

    <StyleCopEnabled enabled="true"/>


### TODO rules:
* Copy Local
* Binary within NuGet package is referenced without proper reference in packages.config
* There is a reference in packages.config, but no link to binaries within it. Support exceptions.
* Same package versions are used for in project (support exceptions)
* VS solution version
* New NuGet initialization approach should be used
* Assembly and root namespace should have same name
* Unapproved build configurations
* No duplicate NuGet packages. Looks like low priority - haven't seen this problem in practice for a long time
* Classify project by type (e.g. production, testing) and disallow references between some groups