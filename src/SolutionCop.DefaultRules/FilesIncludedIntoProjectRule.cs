using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using SolutionCop.Core;

namespace SolutionCop.DefaultRules
{
    [Export(typeof(IProjectRule))]
    public class FilesIncludedIntoProjectRule : ProjectRule<FilesIncludedIntoProjectRuleConfig>
    {
        public override string Id
        {
            get { return "FilesIncludedIntoProject"; }
        }

        public override XElement DefaultConfig
        {
            get
            {
                var element = new XElement(Id);
                element.SetAttributeValue("enabled", "false");
                element.Add(new XElement("FileName", "*.cs"));
                element.Add(new XElement("FileName", "*.xaml"));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectToExcludeFromCheck.csproj")));
                element.Add(new XElement("Exception", new XElement("Project", "ProjectWithSpecificException.csproj"), new XElement("FileName", "ProjectSpecificException*.cs")));
                element.Add(new XElement("Exception", new XElement("FileName", "*.global.exception.pattern.cs")));
                return element;
            }
        }

        protected override FilesIncludedIntoProjectRuleConfig ParseConfigurationSection(XElement xmlRuleConfigs, List<string> errors)
        {
            var unknownElements = xmlRuleConfigs.Elements().Select(x => x.Name.LocalName).Where(x => x != "Exception" && x != "FileName").ToArray();
            if (unknownElements.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: Unknown element(s) {1} in configuration.", Id, string.Join(",", unknownElements)));
            }
            var config = new FilesIncludedIntoProjectRuleConfig();
            config.FilePatternsToProcess.AddRange(xmlRuleConfigs.Elements("FileName").Select(x => x.Value.Trim()));
            if (!config.FilePatternsToProcess.Any())
            {
                errors.Add(string.Format("Bad configuration for rule {0}: No file names to process.", Id));
            }
            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                var xmlFile = xmlException.Element("FileName");
                if (xmlProject == null && xmlFile == null)
                {
                    errors.Add(string.Format("Bad configuration for rule {0}: <Project> or <FileName> element is missing in exceptions list.", Id));
                }
                else if (xmlProject != null)
                {
                    config.ProjectSpecificFilePatternExceptions.Add(xmlProject.Value.Trim(), xmlException.Elements("FileName").Select(x => x.Value.Trim()).ToArray());
                }
                else
                {
                    config.GlobalFilePatternExceptions.AddRange(xmlException.Elements("FileName").Select(x => x.Value.Trim()));
                }
            }
            return config;
        }

        protected override IEnumerable<string> ValidateSingleProject(XDocument xmlProject, string projectFilePath, FilesIncludedIntoProjectRuleConfig config)
        {
            var projectFileName = Path.GetFileName(projectFilePath);
            var filePatternsToExclude = new List<string>(config.GlobalFilePatternExceptions);
            if (config.ProjectSpecificFilePatternExceptions.ContainsKey(projectFileName))
            {
                var fileExceptions = config.ProjectSpecificFilePatternExceptions[projectFileName];
                if (fileExceptions.Any())
                {
                    filePatternsToExclude.AddRange(fileExceptions);
                }
                else
                {
                    yield break;
                }
            }
            foreach (var filePatternToProcess in config.FilePatternsToProcess)
            {
                var filePaths = Directory.EnumerateFiles(Path.GetDirectoryName(projectFilePath), filePatternToProcess, SearchOption.AllDirectories);
                // TODO More flexible folder exclusions
                foreach (var filePath in filePaths.Where(x => !x.ToLower().Contains(@"\obj\") && !x.ToLower().Contains(@"\bin\")))
                {
                    var parentDir = Path.GetDirectoryName(filePath);
                    // TODO Yak!
                    var matchesExcludeFilePattern = filePatternsToExclude.Any(fileToExclude => Directory.EnumerateFiles(parentDir, fileToExclude).Any(x => x == filePath));
                    if (!matchesExcludeFilePattern)
                    {
                        var fileName = Path.GetFileName(filePath);
                        // TODO Make more precise
                        var xmlHintPaths = xmlProject.Descendants(Namespace + "Compile").Where(x => x.Attribute("Include").Value.Contains(fileName));
                        if (!xmlHintPaths.Any())
                        {
                            // TODO Yak!
                            var fileSubPath = Path.GetFullPath(filePath).Substring(Path.GetDirectoryName(projectFilePath).Length + 1);
                            yield return string.Format("File is not referenced in project: {0}", fileSubPath);
                        }
                    }
                }
            }
        }
    }

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