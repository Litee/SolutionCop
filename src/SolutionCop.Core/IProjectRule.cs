using System.Collections.Generic;
using System.Xml.Linq;

namespace SolutionCop.Core
{
    public interface IProjectRule
    {
        string Id { get; }

        string DisplayName { get; }

        XElement DefaultConfig { get; }

        IEnumerable<string> ValidateConfig(XElement xmlRuleConfigs);

        IEnumerable<string> ValidateProject(string projectFilePath, XElement xmlRuleConfigs);
    }
}