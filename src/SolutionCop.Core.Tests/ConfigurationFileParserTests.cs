using System.Collections.Generic;
using System.Xml.Linq;
using ApprovalTests.Reporters;
using Xunit;

namespace SolutionCop.Core.Tests
{
    [UseReporter(typeof (DiffReporter))]
    public class ConfigurationFileParserTests
    {
        [Fact]
        public void Should_pass_for_empty_config()
        {
            const string rulesRules = @"<Rules></Rules>";
            var xmlAllRuleConfigs = XDocument.Parse(rulesRules);
            var errors = ConfigurationFileParser.Parse("MySolution.sln", xmlAllRuleConfigs, new IProjectRule[] {new DummyRule()});
            Assert.Empty(errors);
            Assert.NotNull(xmlAllRuleConfigs.Descendants("Dummy"));
        }

        [Fact]
        public void Should_pass_for_config_with_valid_section()
        {
            const string xmlRules = @"
<Rules>
  <Dummy />
</Rules>
";
            var errors = ConfigurationFileParser.Parse("MySolution.sln", XDocument.Parse(xmlRules), new IProjectRule[] { new DummyRule() });
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
            var errors = ConfigurationFileParser.Parse("MySolution.sln", XDocument.Parse(xmlRules), new IProjectRule[] { new DummyRule() });
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