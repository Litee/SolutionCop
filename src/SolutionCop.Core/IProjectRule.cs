using System.Xml.Linq;

namespace SolutionCop.Core
{
    public interface IProjectRule
    {
        string Id { get; }

        XElement DefaultConfig { get; }

        ValidationResult ValidateAllProjects(XElement xmlRuleConfigs, params string[] projectFilePaths);
    }
}