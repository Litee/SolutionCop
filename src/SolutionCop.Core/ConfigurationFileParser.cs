using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;

namespace SolutionCop.Core
{
    public class ConfigurationFileParser
    {
        private readonly IFileSystem _fileSystem;

        public ConfigurationFileParser() : this(new FileSystem())
        {
        }

        // Constructor is used for testing
        internal ConfigurationFileParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IEnumerable<string> Parse(string pathToSolutionFile, ref string pathToConfigFile, IEnumerable<IProjectRule> rules)
        {
            if (string.IsNullOrEmpty(pathToConfigFile))
            {
                pathToConfigFile = _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(pathToSolutionFile), "SolutionCop.xml");
                Console.Out.WriteLine("INFO: Custom path to config file is not specified, using default one: {0}", pathToConfigFile);
            }
            if (_fileSystem.File.Exists(pathToConfigFile))
            {
                Console.Out.WriteLine("INFO: Existing config file found: {0}", pathToConfigFile);
                return Parse(pathToConfigFile, _fileSystem.File.ReadAllText(pathToConfigFile), rules);
            }
            else
            {
                Console.Out.WriteLine("WARN: Config file does not exist. Creating a new one {0}", pathToConfigFile);
                return Parse(pathToConfigFile, "<Rules></Rules>", rules);
            }
        }

        public IEnumerable<string> Parse(string pathToConfigFile, string rulesConfiguration, IEnumerable<IProjectRule> rules)
        {
            var errors = new List<string>();
            try
            {
                var xmlAllRuleConfigs = XDocument.Parse(rulesConfiguration);
                bool saveConfigFileOnExit = false;
                var xmlRules = xmlAllRuleConfigs.Element("Rules");
                if (xmlRules == null)
                {
                    errors.Add("Root XML element should be <Rules>");
                }
                else
                {
                    foreach (var rule in rules)
                    {
                        var xmlRuleConfig = xmlRules.Element(rule.Id);
                        if (xmlRuleConfig == null)
                        {
                            xmlRules.Add(rule.DefaultConfig);
                            Console.Out.WriteLine("WARNING: No config specified for rule {0} - adding default one", rule.Id);
                            saveConfigFileOnExit = true;
                        }
                        else
                        {
                            Console.Out.WriteLine("DEBUG: Parsing config for rule {0}...", rule.Id);
                            var ruleConfigErrors = rule.ParseConfig(xmlRuleConfig).ToArray();
                            if (ruleConfigErrors.Any())
                            {
                                foreach (var error in ruleConfigErrors)
                                {
                                    Console.Out.WriteLine("ERROR: {0}", error);
                                    Console.Out.WriteLine("ERROR: Rule {0} disabled", rule.Id);
                                    saveConfigFileOnExit = true;
                                    errors.Add(error);
                                }
                            }
                        }
                    }
                    errors.AddRange(xmlRules.Elements().Select(x => x.Name.LocalName).Except(rules.Select(x => x.Id)).Select(unknownSectionName => string.Format("Unknown configuration section {0}", unknownSectionName)));
                    if (saveConfigFileOnExit)
                    {
                        Console.Out.WriteLine("DEBUG: Config file was updated. Saving...");
                        var stringWriter = new StringWriter();
                        xmlAllRuleConfigs.Save(stringWriter);
                        _fileSystem.File.WriteAllText(pathToConfigFile, stringWriter.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                errors.Add("Cannot parse rules configuration: " + e.Message);
            }
            return errors;
        }
    }
}
