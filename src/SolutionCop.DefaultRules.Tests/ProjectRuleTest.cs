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
        protected ProjectRuleTest(IProjectRule instance)
        {
            Instance = instance;
        }

        [Fact]
        public void Should_generate_proper_default_configuration()
        {
            NamerFactory.AdditionalInformation = GetType().Name;
            Approvals.Verify(Instance.DefaultConfig);
        }

        protected IProjectRule Instance { get; private set;
        }

        protected void ShouldPassNormally(string projectFileName, XElement xmlConfig)
        {
            var validationResult = Instance.ValidateProject(projectFileName, xmlConfig);
            validationResult.IsEnabled.ShouldBe(true);
            validationResult.HasErrorsInConfiguration.ShouldBe(false);
            validationResult.Errors.ShouldBeEmpty();
        }
        protected void ShouldPassAsDisabled(string projectFileName, XElement xmlConfig)
        {
            var validationResult = Instance.ValidateProject(projectFileName, xmlConfig);
            validationResult.IsEnabled.ShouldBe(false);
            validationResult.HasErrorsInConfiguration.ShouldBe(false);
            validationResult.Errors.ShouldBeEmpty();
        }

        protected void ShouldFailNormally(string projectFileName, XElement xmlConfig)
        {
            var validationResult = Instance.ValidateProject(projectFileName, xmlConfig);
            validationResult.IsEnabled.ShouldBe(true);
            validationResult.HasErrorsInConfiguration.ShouldBe(false);
            validationResult.Errors.ShouldNotBeEmpty();
            Approvals.VerifyAll(validationResult.Errors, "Errors");
        }

        protected void ShouldFailOnConfiguration(string projectFileName, XElement xmlConfig)
        {
            var validationResult = Instance.ValidateProject(projectFileName, xmlConfig);
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