using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Xml.Linq;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using Shouldly;
using Xunit;

namespace SolutionCop.Core.Tests
{
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovedResults")]
    public class ConfigurationFileParserTests
    {
        private readonly IFileSystem _fileSystemMock;
        private readonly ConfigurationFileParser _instance;

        public ConfigurationFileParserTests()
        {
            _fileSystemMock = new MockFileSystem();
            _instance = new ConfigurationFileParser(_fileSystemMock);
        }

        [Fact]
        public void Should_pass_for_empty_config()
        {
            const string xmlRules = @"<Rules></Rules>";
            var errors = _instance.Parse("SolutionCop.xml", xmlRules, new IProjectRule[] { new DummyRule() });
            errors.ShouldBeEmpty<string>();
            var newContent = _fileSystemMock.File.ReadAllText("SolutionCop.xml");
            Approvals.Verify(newContent);
        }

        [Fact]
        public void Should_pass_for_config_with_valid_section()
        {
            const string xmlRules = @"
<Rules>
  <Dummy />
</Rules>";
            var errors = _instance.Parse("SolutionCop.xml", xmlRules, new IProjectRule[] { new DummyRule() });
            Assert.Empty(errors);
        }

        [Fact]
        public void Should_fail_for_config_with_unknown_section()
        {
            const string xmlRules = @"
<Rules>
  <NonExistingId />
  <Dummy />
</Rules>
";
            var errors = _instance.Parse("SolutionCop.xml", xmlRules, new IProjectRule[] { new DummyRule() });
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void Should_fail_for_config_with_wrong_root_element()
        {
            const string xmlRules = @"
<SomeRoot>
  <Dummy />
</SomeRoot>
";
            var errors = _instance.Parse("SolutionCop.xml", xmlRules, new IProjectRule[] { new DummyRule() });
            Assert.NotEmpty(errors);
        }

        [Fact]
        public void Should_fail_for_misformed_xml()
        {
            const string xmlRules = @"
<Rules>
  <BadElement>
</Rules>
";
            var errors = _instance.Parse("SolutionCop.xml", xmlRules, new IProjectRule[] { new DummyRule() });
            Assert.NotEmpty(errors);
        }

        private class DummyRule : IProjectRule
        {
            private readonly bool _isEnabled = false;

            public string Id
            {
                get { return "Dummy"; }
            }

            public string DisplayName
            {
                get { return "Dummy Display Name"; }
            }

            public XElement DefaultConfig
            {
                get { return new XElement(Id); }
            }

            public bool IsEnabled
            {
                get { return _isEnabled; }
            }

            public IEnumerable<string> ValidateProject(string projectFilePath)
            {
                throw new System.NotImplementedException();
            }

            public IEnumerable<string> ParseConfig(XElement xmlRuleConfigs)
            {
                yield break;
            }
        }
    }
}