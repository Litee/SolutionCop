using System;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using SolutionCop.Core;
using Xunit;

namespace SolutionCop.DefaultRules.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public abstract class ProjectRuleTest : IDisposable
    {
        private readonly IProjectRule _instance;

        protected ProjectRuleTest(IProjectRule instance)
        {
            _instance = instance;
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            NamerFactory.AdditionalInformation = GetType().Name;
            Approvals.Verify(_instance.DefaultConfig);
        }

        protected void ShouldPassNormally(XElement xmlConfig, params string[] projectFileNames)
        {
            var validationResult = _instance.ValidateAllProjects(xmlConfig, projectFileNames);
            validationResult.IsEnabled.ShouldBe(true);
            validationResult.HasErrorsInConfiguration.ShouldBe(false);
            validationResult.Errors.ShouldBeEmpty();
        }

        protected void ShouldPassAsDisabled(XElement xmlConfig, params string[] projectFileNames)
        {
            var validationResult = _instance.ValidateAllProjects(xmlConfig, projectFileNames);
            validationResult.IsEnabled.ShouldBe(false);
            validationResult.HasErrorsInConfiguration.ShouldBe(false);
            validationResult.Errors.ShouldBeEmpty();
        }

        protected void ShouldFailNormally(XElement xmlConfig, params string[] projectFileNames)
        {
            var validationResult = _instance.ValidateAllProjects(xmlConfig, projectFileNames);
            validationResult.IsEnabled.ShouldBe(true);
            validationResult.HasErrorsInConfiguration.ShouldBe(false);
            validationResult.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult.Errors, "Errors");
        }

        protected void ShouldFailOnConfiguration(XElement xmlConfig, params string[] projectFileNames)
        {
            var validationResult = _instance.ValidateAllProjects(xmlConfig, projectFileNames);
            validationResult.IsEnabled.ShouldBe(true);
            validationResult.HasErrorsInConfiguration.ShouldBe(true);
            validationResult.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult.Errors, "Errors");
        }

        public void Dispose()
        {
            // Setting to null to switch back to standard file naming for approvals.
            NamerFactory.AdditionalInformation = null;
        }
    }
}