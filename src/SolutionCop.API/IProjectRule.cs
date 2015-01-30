using System.Collections.Generic;
using System.Xml.Linq;

namespace SolutionCop.API
{
    public interface IProjectRule
    {
        string Id { get; }
        string DisplayName { get; }
        XElement DefaultConfig { get; }
        IEnumerable<string> Validate(string projectFilePath, XElement xmlRuleConfigs);
    }
}