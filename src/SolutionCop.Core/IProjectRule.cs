using System.Xml.Linq;

namespace SolutionCop.Core
{
    public interface IProjectRule
    {
        string Id { get; }

        string DisplayName { get; }

        XElement DefaultConfig { get; }

        ValidationResult ValidateProjects(XElement xmlRuleConfigs, params string[] projectFilePaths);
    }
}