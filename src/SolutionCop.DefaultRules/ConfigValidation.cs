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
                .Select(x => string.Format("<{0}>", x))
                .ToArray();
            if (unknownElements.Any())
            {
                var entryOrEntries = unknownElements.Count() == 1 ? "entry" : "entries";
                var allowedElementsList = string.Join(", ", allowedElementNames.Select(x => string.Format("<{0}>", x)));
                var errorDetails = string.Format("Unknown {0} within <{1}>: {2}. Allowed entries: {3}.", entryOrEntries, xmlElement.Name.LocalName, string.Join(", ", unknownElements), allowedElementsList);
                errors.Add(string.Format(Resources.BadConfiguration, ruleId, errorDetails));
            }
        }
    }
}