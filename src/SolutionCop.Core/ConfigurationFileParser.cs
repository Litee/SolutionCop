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
        private const string SolutionCopXsdFileName = "SolutionCop.xsd";
        private readonly XNamespace _xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        private readonly Action<string, byte[]> _saveConfigFileAction;
        private readonly ISolutionCopConsole _logger;

        public ConfigurationFileParser(ISolutionCopConsole logger)
            : this(File.WriteAllBytes, logger)
        {
        }

        // Constructor is used for testing
        internal ConfigurationFileParser(Action<string, byte[]> saveConfigFileAction, ISolutionCopConsole logger)
        {
            _saveConfigFileAction = saveConfigFileAction;
            _logger = logger;
        }

        public Dictionary<string, XElement> ParseConfigFile(string pathToConfigFile, IEnumerable<IProjectRule> rules, List<string> errors)
        {
            if (File.Exists(pathToConfigFile))
            {
                _logger.LogInfo("Existing config file found: {0}", pathToConfigFile);
                return ParseConfigString(pathToConfigFile, File.ReadAllText(pathToConfigFile), rules, errors);
            }
            else
            {
                _logger.LogWarning("Config file does not exist. Creating a new one {0}", pathToConfigFile);
                return ParseConfigString(pathToConfigFile, "<Rules></Rules>", rules, errors);
            }
        }

        public Dictionary<string, XElement> ParseConfigString(string pathToConfigFile, string rulesConfiguration, IEnumerable<IProjectRule> knownRules, List<string> errors)
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
                    if (xmlRules.Attributes().All(x => x.Value != "http://www.w3.org/2001/XMLSchema-instance"))
                    {
                        xmlRules.Add(new XAttribute(XNamespace.Xmlns + "xsi", _xsi));
                        xmlRules.Add(new XAttribute(_xsi + "noNamespaceSchemaLocation", SolutionCopXsdFileName));
                        saveConfigFileOnExit = true;
                        var pathToSchemaFile = Path.GetDirectoryName(pathToConfigFile) + Path.DirectorySeparatorChar + SolutionCopXsdFileName;
                        if (!File.Exists(pathToSchemaFile))
                        {
                            using (var stream = typeof(ConfigurationFileParser).Assembly.GetManifestResourceStream("SolutionCop.Core." + SolutionCopXsdFileName))
                            {
                                byte[] buffer = new byte[stream.Length];
                                stream.Read(buffer, 0, buffer.Length);
                                File.WriteAllBytes(pathToSchemaFile, buffer);
                            }
                        }
                    }
                    foreach (var rule in knownRules)
                    {
                        var xmlRuleConfig = xmlRules.Element(rule.Id);
                        if (xmlRuleConfig == null)
                        {
                            ruleConfigsMap.Add(rule.Id, rule.DefaultConfig);

                            // Adding default section into original DOM for saving
                            xmlRules.Add(rule.DefaultConfig);
                            _logger.LogWarning("No config specified for rule {0} - adding default one", rule.Id);
                            saveConfigFileOnExit = true;
                        }
                        else
                        {
                            ruleConfigsMap.Add(rule.Id, xmlRuleConfig);
                        }
                    }
                    var unknownSectionNames = xmlRules.Elements().Select(x => x.Name.LocalName).Except(knownRules.Select(x => x.Id));
                    foreach (var unknownSectionName in unknownSectionNames)
                    {
                        _logger.LogWarning("Unknown configuration section {0}", unknownSectionName);
                    }
                    if (saveConfigFileOnExit)
                    {
                        _logger.LogDebug("Config file was updated. Saving...");
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
