namespace SolutionCop.DefaultRules.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal static class XmlNodeExtensions
    {
        /// <summary>
        /// Some nuspec files have schema, which should be ignored in solutioncop
        /// </summary>
        /// <param name="current">Target element to start searching</param>
        /// <param name="localName">Local name of xml element</param>
        /// <returns>Child element</returns>
        public static XElement GetElementWithLocalName(this XContainer current, string localName)
        {
            return current.GetElementsWithLocalName(localName).SingleOrDefault();
        }

        public static IReadOnlyCollection<XElement> GetElementsWithLocalName(this XContainer current, string localName)
        {
            return current
                .Elements()
                .Where(e => string.Equals(e.Name.LocalName, localName, StringComparison.Ordinal))
                .ToArray();
        }
    }
}