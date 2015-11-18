using System.IO;

namespace SolutionCop.Core
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;

    public class RulesDirectoryCatalog
    {
        private readonly ISolutionCopConsole _logger;

        public RulesDirectoryCatalog(ISolutionCopConsole logger)
        {
            _logger = logger;
        }

        public IProjectRule[] LoadRules()
        {
            _logger.LogInfo("Scanning for rules...");

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(typeof(RulesDirectoryCatalog).Assembly.Location)));
            var container = new CompositionContainer(catalog);
            var rules = container.GetExports<IProjectRule>().Select(x => x.Value).ToArray();
            foreach (var rule in rules)
            {
                _logger.LogInfo("Rule {0} found.", rule.Id);
            }

            _logger.LogInfo("Scanning for rules finished! Rules found: {0}", rules.Count());
            return rules.ToArray();
        }
    }
}
