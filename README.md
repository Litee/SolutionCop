# SolutionCop

Tool for static analysis of Visual Studio solutions and projects. 

Usage: SolutionCop.exe -s *path-to-vs-solution* [-c *path-to-solutioncop-config*]

If <path-to-solutioncop-config> is not provided then tool looks for SolutionCop.xml file next to target *.sln. If config file cannot be found then default one is created.

## Config file

Below is an example config file. Any rule can be disabled by setting *enabled* attribute to *false*. Note that tool adds default entries for rules if they are missing.

    <Rules>
      <TargetFrameworkVersion>
        <AllowedValue>4.5</AllowedValue>
      </TargetFrameworkVersion>

      <ReferenceNuGetPackagesOnly enabled="true"/>

      <TreatWarningsAsErrors>
        <Warning>0123</Warning>
        <Warning>1234</Warning>
        <Exception>Project.csproj</Exception>
        <Exception>Another.Project.csproj</Exception>
      </TreatWarningsAsErrors>

      <SuppressWarnings>
        <Warning>0123</Warning>
        <Warning>1234</Warning>
        <Exception>Project.csproj</Exception>
        <Exception>Another.Project.csproj</Exception>
      </SuppressWarnings>

      <WarningLevel minimalValue="4">
        <Exception>Project.csproj</Exception>
        <Exception>Another.Project.csproj</Exception>
      </WarningLevel>

      <VerifyNuGetPackageVersions>
        <Package id="packageOne" version="1.2.3"/> <!-- 1.2.3 <= version -->
        <Package id="packageTwo" version="(1.2-alpha, 1.99.99]"/> <!-- 1.2-alpha < version <= 1.99.99 -->
      </VerifyNuGetPackageVersions>

      <NuGetPackageReferencedInProject>
        <Exception>package-id</Exception>
        <Exception>another-package-id</Exception>
      </NuGetPackageReferencedInProject>
    </Rules>

## Rules

### Verify target version of the .NET Framework

Description: Fails if one of projects has target version different from one specified

Sample config section:

    <TargetFrameworkVersion>
      <AllowedValue>4.5</AllowedValue>
    </TargetFrameworkVersion>

### Verify that all referenced binaries come from NuGet packages

Description: Fails if project references binaries outside NuGet *packages* folder

Sample config section:

    <ReferenceNuGetPackagesOnly enabled="true"/>

### Verify warnings treatment as errors

Description: Fails if "Treat warnings as errors" is not enabled in all build configurations or if not all specified warnings are treated as errors

Sample config sections:

    <TreatWarningsAsErrors>
      <AllWarnings />
    </TreatWarningsAsErrors>

or

    <TreatWarningsAsErrors>
      <Warning>0123</Warning>
      <Warning>1234</Warning>
      <Exception>Project.csproj</Exception>
      <Exception>Another.Project.csproj</Exception>
    </TreatWarningsAsErrors>

### Verify suppressed warnings

Description: Fails if project suppresses any warning that is not in the specified white list

Sample config section:

    <SuppressWarnings>
      <Warning>0123</Warning>
      <Warning>1234</Warning>
      <Exception>Project.csproj</Exception>
      <Exception>Another.Project.csproj</Exception>
    </SuppressWarnings>

### Verify warning level

Description: Fails if any project has warning level lower than one specified

Sample config section:

    <WarningLevel minimalValue="4">
      <Exception>Project.csproj</Exception>
      <Exception>Another.Project.csproj</Exception>
    </WarningLevel>

### Verify that NuGet package versions match rules

Description: Fails if some package in packages.config file has version that doesn't match rule or there is no rule for the package

Sample config section:

    <VerifyNuGetPackageVersions>
      <Package id="packageOne" version="1.2.3"/> <!-- 1.2.3 <= version -->
      <Package id="packageTwo" version="(1.2-alpha, 1.99.99]"/> <!-- 1.2-alpha < version <= 1.99.99 -->
    </VerifyNuGetPackageVersions>

### Verify that StyleCop is enabled for all projects (exceptions supported)

Description: Fails if some VS project does not import StyleCop.MSBuild.Targets or Microsoft.SourceAnalysis.targets

Sample config section:

    <StyleCopEnabled>
      <Exception>Project.csproj</Exception>
      <Exception>Another.Project.csproj</Exception>
    </StyleCopEnabled>

### Verify that all packages specified in packages.config are used in *.csproj (exceptions supported)

Sample config section:

    <NuGetPackageReferencedInProject>
      <Exception>package-id</Exception>
      <Exception>another-package-id</Exception>
    </NuGetPackageReferencedInProject>

### Verify that correct automatic packages restore mode is used (see https://docs.nuget.org/Consume/Package-Restore/Migrating-to-Automatic-Package-Restore). Exceptions are supported.

    <NuGetAutomaticPackagesRestore>
      <Exception>Project.csproj</Exception>
      <Exception>Another.Project.csproj</Exception>
    </NuGetAutomaticPackagesRestore>

### TODO rules:
* Copy Local
* Binary within NuGet package is referenced without proper reference in packages.config
* Same package versions are used in project (support exceptions)
* VS solution version
* Assembly and root namespace should have same name
* Unapproved build configurations
* No duplicate NuGet packages. Looks like low priority - haven't seen this problem in practice for a long time
* Classify project by type (e.g. production, testing) and disallow references between some groups
* TreatStyleCopWarningsAsErrors
* All *.cs files are referenced in project
* Proper owner in AssemblyInfo
* Proper copyright date in AssemblyInfo

### Other Todos:
* Group errors by project or by rule
* NuGet commands in log for fixing versions
* More flexible exceptions
* Check for unknown config sections
* Option fail on missing sections
* Links to broken rule details in wiki
* Treat rules as separate tests for TeamCity (not sure whether it will work better than plain list)
* Custom folders for searching rules