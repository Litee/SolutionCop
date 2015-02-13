namespace SolutionCop.Core
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;

    public class RulesDirectoryCatalog
    {
        public IProjectRule[] LoadRules()
        {
            Console.Out.WriteLine("INFO: Scanning for rules...");

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog("."));
            var container = new CompositionContainer(catalog);
            var rules = container.GetExports<IProjectRule>().Select(x => x.Value).ToArray();
            foreach (var rule in rules)
            {
                Console.Out.WriteLine("INFO: Rule {0} found.", rule.Id);
            }

            Console.Out.WriteLine("INFO: Scanning for rules finished! Rules found: {0}", rules.Count());
            return rules.ToArray();
        }
    }
}
