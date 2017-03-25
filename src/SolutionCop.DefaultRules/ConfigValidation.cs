namespace SolutionCop.DefaultRules
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Properties;

    internal static class ConfigValidation
    {
        public static void ValidateConfigSectionForAllowedElements(XElement xmlElement, List<string> errors, string ruleId, params string[] allowedElementNames)
        {
            var unknownElements = xmlElement.Elements()
                .Select(x => x.Name.LocalName)
                .Where(x => allowedElementNames.All(y => y != x))
                .Select(x => $"<{x}>")
                .ToArray();
            if (unknownElements.Any())
            {
                var entryOrEntries = unknownElements.Length == 1 ? "entry" : "entries";
                var allowedElementsList = string.Join(", ", allowedElementNames.Select(x => $"<{x}>"));
                var errorDetails = $"Unknown {entryOrEntries} within <{xmlElement.Name.LocalName}>: {string.Join(", ", unknownElements)}. Allowed entries: {allowedElementsList}.";
                errors.Add(string.Format(Resources.BadConfiguration, ruleId, errorDetails));
            }
        }
    }
}