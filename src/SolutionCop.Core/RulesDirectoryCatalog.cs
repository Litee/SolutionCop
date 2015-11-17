using System.IO;

namespace SolutionCop.Core
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;

    public class RulesDirectoryCatalog
    {
        private readonly IAnalysisLogger _logger;

        public RulesDirectoryCatalog(IAnalysisLogger logger)
        {
            _logger = logger;
        }

        public IProjectRule[] LoadRules()
        {
            _logger.LogInfo("INFO: Scanning for rules...");

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(typeof(RulesDirectoryCatalog).Assembly.Location)));
            var container = new CompositionContainer(catalog);
            var rules = container.GetExports<IProjectRule>().Select(x => x.Value).ToArray();
            foreach (var rule in rules)
            {
                _logger.LogInfo("INFO: Rule {0} found.", rule.Id);
            }

            _logger.LogInfo("INFO: Scanning for rules finished! Rules found: {0}", rules.Count());
            return rules.ToArray();
        }
    }
}
