namespace SolutionCop.Core
{
    using System.Xml.Linq;

    public interface IProjectRule
    {
        string Id { get; }

        XElement DefaultConfig { get; }

        ValidationResult ValidateAllProjects(XElement xmlRuleConfigs, params string[] projectFilePaths);
    }
}