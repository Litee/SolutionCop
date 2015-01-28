using System.Collections.Generic;
using System.Xml.Linq;

namespace SolutionCop.API
{
    public interface IProjectRule : IRule
    {
        IEnumerable<string> Validate(string projectFilePath, XElement xmlRuleConfigs);
    }
}