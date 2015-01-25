using System.Collections.Generic;
using System.Xml.Linq;

namespace SolutionCop.API
{
    public interface IRule
    {
        string Id { get; }
        string DisplayName { get; }
        IEnumerable<string> ValidateProject(string projectFilePath, XElement xmlRuleConfigs);
    }
}
