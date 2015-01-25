using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SolutionCop.API;

namespace SolutionCop
{
    internal class RulesDirectoryCatalog
    {
        internal IEnumerable<IRule> LoadRules()
        {
            Console.Out.WriteLine("INFO: Scanning for rules...");

            var rules = new List<IRule>();
            var assemblies = new DirectoryInfo(".").GetFiles("*.DLL");
            foreach (var assemblyFile in assemblies)
            {
                var ruleTypes = Assembly.LoadFrom(assemblyFile.FullName).GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IRule).IsAssignableFrom(t));
                foreach (var ruleType in ruleTypes)
                {
                    var rule = (IRule)Activator.CreateInstance(ruleType);
                    Console.Out.WriteLine("INFO: Rule found: '{0}'", rule.DisplayName);
                    rules.Add(rule);
                }
            }

            Console.Out.WriteLine("INFO: Scanning for rules finished! Rules found: {0}", rules.Count);
            return rules;
        }
    }
}
