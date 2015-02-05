using System.Collections.Generic;
using System.Xml.Linq;

namespace SolutionCop.Core
{
    public interface IProjectRule
    {
        string Id { get; }

        string DisplayName { get; }

        XElement DefaultConfig { get; }

        bool IsEnabled { get; }

        IEnumerable<string> ParseConfig(XElement xmlRuleConfigs);

        IEnumerable<string> ValidateProject(string projectFilePath);
    }
}