namespace SolutionCop.DefaultRules.Basic
{
    using System.Collections.Generic;

    public class FilesIncludedIntoProjectRuleConfig
    {
        private readonly List<string> _filePatternsToProcess = new List<string>();
        private readonly Dictionary<string, string[]> _projectSpecificFilePatternExceptions = new Dictionary<string, string[]>();
        private readonly List<string> _globalFilePatternExceptions = new List<string>();

        public List<string> FilePatternsToProcess
        {
            get { return _filePatternsToProcess; }
        }

        public Dictionary<string, string[]> ProjectSpecificFilePatternExceptions
        {
            get { return _projectSpecificFilePatternExceptions; }
        }

        public List<string> GlobalFilePatternExceptions
        {
            get { return _globalFilePatternExceptions; }
        }
    }
}