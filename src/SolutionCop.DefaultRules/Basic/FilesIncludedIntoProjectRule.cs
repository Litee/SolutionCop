﻿namespace SolutionCop.DefaultRules.Basic
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml.Linq;
    using Core;

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
            ValidateConfigSectionForAllowedElements(xmlRuleConfigs, errors, "Exception", "FileName");

            var config = new FilesIncludedIntoProjectRuleConfig();
            config.FilePatternsToProcess.AddRange(xmlRuleConfigs.Elements("FileName").Select(x => x.Value.Trim()));
            if (!config.FilePatternsToProcess.Any())
            {
                errors.Add($"Bad configuration for rule {Id}: No file names to process.");
            }

            foreach (var xmlException in xmlRuleConfigs.Elements("Exception"))
            {
                var xmlProject = xmlException.Element("Project");
                var xmlFile = xmlException.Element("FileName");
                if (xmlProject == null && xmlFile == null)
                {
                    errors.Add($"Bad configuration for rule {Id}: <Project> or <FileName> element is missing in exceptions list.");
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
            var projectDirPath = Path.GetFullPath(Path.GetDirectoryName(projectFilePath));
            foreach (var filePatternToProcess in config.FilePatternsToProcess)
            {
                var filePaths = Directory.EnumerateFiles(projectDirPath, filePatternToProcess, SearchOption.AllDirectories);

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
                        var xmlHintPaths = xmlProject.Descendants(Namespace + "Compile").Where(x => HttpUtility.UrlDecode(x.Attribute("Include").Value).ToLower().Contains(fileName.ToLower()));
                        if (!xmlHintPaths.Any())
                        {
                            // TODO Yak!
                            var fileSubPath = Path.GetFileName(projectDirPath) + Path.DirectorySeparatorChar + Path.GetFullPath(filePath).Substring(projectDirPath.Length + 1);
                            yield return $"File is not referenced in project: {fileSubPath}";
                        }
                    }
                }
            }
        }
    }
}