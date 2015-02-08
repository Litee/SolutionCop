using System.Collections.Generic;
using System.Xml.Linq;

namespace SolutionCop.Core
{
    public interface IProjectRule
    {
        string Id { get; }

        string DisplayName { get; }

        XElement DefaultConfig { get; }

        ValidationResult ValidateProject(string projectFilePath, XElement xmlRuleConfigs);
    }
}