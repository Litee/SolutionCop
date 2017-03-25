namespace SolutionCop.DefaultRules.Tests.NuGet
{
    using System;
    using System.Xml.Linq;
    using DefaultRules.NuGet;
    using Shouldly;
    using Xunit;
    using Xunit.Extensions;

    public sealed class PackageProjectExceptionTest
    {
        private const string ConfiguredPackage = "MyPackage";
        private const string ConfiguredProject = "MyProject";

        [Theory]
        [InlineData(ConfiguredPackage, ConfiguredProject, true)]
        [InlineData("123", ConfiguredProject, false)]
        [InlineData(null, ConfiguredProject, false)]
        [InlineData(ConfiguredPackage, "123", false)]
        [InlineData(ConfiguredPackage, null, false)]
        public void ShouldUnionPackageAndProjectCondition(string packageName, string projectName, bool expectedIsMatched)
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<Exception>" +
                $"<Package>{ConfiguredPackage}</Package>" +
                $"<Project>{ConfiguredProject}</Project>" +
                "</Exception>");

            // When
            var exception = PackageProjectException.Parse(xmlConfig);

            // Then
            exception.Matches(packageName, projectName).ShouldBe(expectedIsMatched);
        }

        [Theory]
        [InlineData(ConfiguredPackage, ConfiguredProject, true)]
        [InlineData("123", ConfiguredProject, false)]
        [InlineData(null, ConfiguredProject, false)]
        [InlineData(ConfiguredPackage, "123", true)]
        [InlineData(ConfiguredPackage, null, true)]
        public void ShouldIgnoreProjectIfItIsNotConfigured(string packageName, string projectName, bool expectedIsMatched)
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<Exception>" +
                $"<Package>{ConfiguredPackage}</Package>" +
                "</Exception>");

            // When
            var exception = PackageProjectException.Parse(xmlConfig);

            // Then
            exception.Matches(packageName, projectName).ShouldBe(expectedIsMatched);
        }

        [Theory]
        [InlineData(ConfiguredPackage, ConfiguredProject, true)]
        [InlineData("123", ConfiguredProject, true)]
        [InlineData(null, ConfiguredProject, true)]
        [InlineData(ConfiguredPackage, "123", false)]
        [InlineData(ConfiguredPackage, null, false)]
        public void ShouldIgnorePackageIfItIsNotConfigured(string packageName, string projectName, bool expectedIsMatched)
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<Exception>" +
                $"<Project>{ConfiguredProject}</Project>" +
                "</Exception>");

            // When
            var exception = PackageProjectException.Parse(xmlConfig);

            // Then
            exception.Matches(packageName, projectName).ShouldBe(expectedIsMatched);
        }

        [Theory]
        [InlineData("c:\\myProject.csproj", "myProject", true)]
        [InlineData("c:\\MYProject.csproj", "myProject", true)]
        [InlineData("myProject", "c:\\MYProject.csproj", true)]
        [InlineData("myProject2", "myProject", false)]
        [InlineData("myProject", "myProject2", false)]
        [InlineData("c:\\myProject.csproj", "d:\\myProject.csproj", true)]
        public void ShouldUseProjectNameForParsing(string configuredProjectName, string actualProjectName, bool expectedIsMatched)
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<Exception>" +
                $"<Project>{configuredProjectName}</Project>" +
                "</Exception>");

            // When
            var exception = PackageProjectException.Parse(xmlConfig);

            // Then
            exception.Matches(null, actualProjectName).ShouldBe(expectedIsMatched);
        }

        [Fact]
        public void ShouldFailForUnknownTags()
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<Exception>" +
                $"<Project>123</Project>" +
                "<UnknownTag/>" +
                "</Exception>");

            // When
            var action = new Assert.ThrowsDelegate(() => PackageProjectException.Parse(xmlConfig));

            // Then
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ShouldFailForNoTags()
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<Exception>" +
                "</Exception>");

            // When
            var action = new Assert.ThrowsDelegate(() => PackageProjectException.Parse(xmlConfig));

            // Then
            Assert.Throws<ArgumentException>(action);
        }

        [Fact]
        public void ShouldFailIncorrectParentTag()
        {
            // Given
            var xmlConfig = XElement.Parse(
                @"<IncorrentParentTag>" +
                $"<Project>123</Project>" +
                "</IncorrentParentTag>");

            // When
            var action = new Assert.ThrowsDelegate(() => PackageProjectException.Parse(xmlConfig));

            // Then
            Assert.Throws<ArgumentException>(action);
        }
    }
}