namespace SolutionCop.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    public class ConfigurationFileParser
    {
        private readonly Action<string, byte[]> _saveConfigFileAction;

        public ConfigurationFileParser() : this(File.WriteAllBytes)
        {
        }

        // Constructor is used for testing
        internal ConfigurationFileParser(Action<string, byte[]> saveConfigFileAction)
        {
            _saveConfigFileAction = saveConfigFileAction;
        }

        public Dictionary<string, XElement> Parse(string pathToSolutionFile, ref string pathToConfigFile, IEnumerable<IProjectRule> rules, List<string> errors)
        {
            if (string.IsNullOrEmpty(pathToConfigFile))
            {
                pathToConfigFile = Path.Combine(Path.GetDirectoryName(pathToSolutionFile), "SolutionCop.xml");
                Console.Out.WriteLine("INFO: Custom path to config file is not specified, using default one: {0}", pathToConfigFile);
            }
            if (File.Exists(pathToConfigFile))
            {
                Console.Out.WriteLine("INFO: Existing config file found: {0}", pathToConfigFile);
                return Parse(pathToConfigFile, File.ReadAllText(pathToConfigFile), rules, errors);
            }
            else
            {
                Console.Out.WriteLine("WARN: Config file does not exist. Creating a new one {0}", pathToConfigFile);
                return Parse(pathToConfigFile, "<Rules></Rules>", rules, errors);
            }
        }

        public Dictionary<string, XElement> Parse(string pathToConfigFile, string rulesConfiguration, IEnumerable<IProjectRule> rules, List<string> errors)
        {
            try
            {
                var ruleConfigsMap = new Dictionary<string, XElement>();
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
                            ruleConfigsMap.Add(rule.Id, rule.DefaultConfig);

                            // Adding default section into original DOM for saving
                            xmlRules.Add(rule.DefaultConfig);
                            Console.Out.WriteLine("WARN: No config specified for rule {0} - adding default one", rule.Id);
                            saveConfigFileOnExit = true;
                        }
                        else
                        {
                            ruleConfigsMap.Add(rule.Id, xmlRuleConfig);
                        }
                    }
                    var unknownSectionNames = xmlRules.Elements().Select(x => x.Name.LocalName).Except(rules.Select(x => x.Id));
                    foreach (var unknownSectionName in unknownSectionNames)
                    {
                        Console.Out.WriteLine("WARN: Unknown configuration section {0}", unknownSectionName);
                    }
                    if (saveConfigFileOnExit)
                    {
                        Console.Out.WriteLine("DEBUG: Config file was updated. Saving...");
                        var settings = new XmlWriterSettings
                        {
                            Encoding = Encoding.UTF8,
                            Indent = true,
                        };
                        var memoryStream = new MemoryStream();
                        using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                        {
                            xmlAllRuleConfigs.Save(xmlWriter);
                        }
                        _saveConfigFileAction(pathToConfigFile, memoryStream.ToArray());
                    }
                }
                return ruleConfigsMap;
            }
            catch (Exception e)
            {
                errors.Add("Cannot parse rules configuration: " + e.Message);
            }
            return new Dictionary<string, XElement>();
        }
    }
}
