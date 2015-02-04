using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SolutionCop
{
    internal class RulesDirectoryCatalog
    {
        internal IEnumerable<IProjectRule> LoadRules()
        {
            Console.Out.WriteLine("INFO: Scanning for rules...");

            var rules = new List<IProjectRule>();
            var assemblies = new DirectoryInfo(".").GetFiles("*.DLL");
            foreach (var assemblyFile in assemblies)
            {
                var ruleTypes = Assembly.LoadFrom(assemblyFile.FullName).GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && typeof(IProjectRule).IsAssignableFrom(t));
                foreach (var ruleType in ruleTypes)
                {
                    var rule = (IProjectRule)Activator.CreateInstance(ruleType);
                    Console.Out.WriteLine("INFO: Rule {0} found. Description: '{1}'", rule.Id, rule.DisplayName);
                    rules.Add(rule);
                }
            }

            Console.Out.WriteLine("INFO: Scanning for rules finished! Rules found: {0}", rules.Count);
            return rules;
        }
    }
}
