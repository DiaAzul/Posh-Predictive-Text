using System.Reflection;
using System.Text;

namespace PoshPredictiveText.SyntaxTreeSpecs
{

    /// <summary>
    /// Sets the rules to be followed duruing parsing.
    /// 
    /// There is no single standard for command arguments, and different commands
    /// (arising from different programming heritages) will have different neanings
    /// for symbols and policies for what is, and is not, permissible. The ParseMode
    /// is a flag to indicate what symbols and policies to implement when parsing
    /// the command line and predicting text.
    /// </summary>
    internal enum ParseMode
    {
        Posix,
        Python,
        Windows,
    }

    internal record ConfigItem
    {
        internal string? Definition { get; init; }
        internal string? ToolTips { get; init; }
        internal ParseMode ParseMode { get; init; }
    }

    /// <summary>
    /// Class containing configuration information for supported commands.
    /// </summary>
    internal static class SyntaxTreesConfig
    {
        /// <summary>
        /// Defines the commands supported by the cmdlet. The key for the dictionary
        /// is either a command or an alias to the command. The value in the dictionary
        /// is the name of the command implementing the completions.
        /// </summary>
        private static readonly Dictionary<string, string> SUPPORTED_COMMANDS = new()
        {
            {"conda", "conda" },
            {"conda.exe", "conda" },
            {"mamba", "conda" }  // Note, this is not strictly correct.
        };

        /// <summary>
        /// The root path to the syntax tree specifications. The name of the xml file 
        /// containing the syntax tree definitions and the name of the toolTip resouce
        /// files are defined in the COMMAND_CONFIGS dictionary. File names are added to
        /// the RESOURCE_ROOT to build the location of the relevant file.
        /// </summary>
        private const string RESOURCE_ROOT = "PoshPredictiveText.SyntaxTreeSpecs.";

        private static readonly Dictionary<string, ConfigItem> COMMAND_CONFIGS = new() {
            {"conda",  new ConfigItem {
                Definition = "CondaSyntaxTree.xml",
                ToolTips = "CondaToolTips",
                ParseMode = SyntaxTreeSpecs.ParseMode.Posix } },
        };

        /// <summary>
        /// Test wehther a given command is supported.
        /// </summary>
        /// <param name="commandNameToTest">Name, or part name, of command.</param>
        /// <returns>True if the command is supported.</returns>
        internal static bool IsSupportedCommand(string commandNameToTest)
        {
            return SUPPORTED_COMMANDS.ContainsKey(commandNameToTest);
        }

        /// <summary>
        /// Get list of command suggestions given a partial command name.
        /// </summary>
        /// <param name="partialCommand">Partial command name.</param>
        /// <returns>List of potential commands.</returns>
        internal static List<string>SuggestedCommands(string partialCommand)
        {
            return COMMAND_CONFIGS.Keys
                                    .Where( key => key.StartsWith(partialCommand))
                                    .ToList();
        }

        /// <summary>
        /// Returns supported commands as a comma separated list.
        /// </summary>
        /// <returns>Supported commands as comma separated list.</returns>
        internal static string SupportedCommands()
        {
            StringBuilder commands = new();
            string delimeter = "";
            foreach (var command in SUPPORTED_COMMANDS.Keys)
            {
                commands.Append(delimeter);
                commands.Append(command);
                delimeter = ", ";
            }
            return commands.ToString();
        }

        /// <summary>
        /// Runs a consistency check on the command dictionary.
        /// 
        /// Consistency means that all aliases point to an actual command.
        /// For each command there is a valid definition and toolTip file.
        /// 
        /// Used within testing to confirm the configuration is valud.
        /// </summary>
        /// <returns>True if the dictionary is consistent.</returns>
        internal static void CheckConsistency()
        {
            List<string> configuredCommands = COMMAND_CONFIGS.Keys.ToList();

            var resourceManifest = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            // Check that each alias references a command with a configuration.
            foreach (KeyValuePair<string, string> commandFromAlias in SUPPORTED_COMMANDS)
            {
                if (!configuredCommands.Contains(commandFromAlias.Value))
                    throw new ArgumentException($"Alias {commandFromAlias.Key} has no configured command.");
            }

            // Check that for each configured command there is a valid definition and toolTip resource.
            foreach (KeyValuePair<string, ConfigItem> config in COMMAND_CONFIGS)
            {
                // Test definition file exists and can be loaded as a resource.
                string? definitionPath = Definition(config.Key);
                if (string.IsNullOrWhiteSpace(definitionPath))
                    throw new ArgumentException($"Command config definition resource not defined for {config.Key}");

                bool definitionPathExists = resourceManifest.Contains(definitionPath);
                if (!definitionPathExists)
                    throw new ArgumentException($"Command config definition resource does not exist for {config.Key}");

                // Test tooltip resource is defined and can be loaded.
                string? toolTipPath = ToolTips(config.Key);
                if (string.IsNullOrWhiteSpace(toolTipPath))
                    throw new ArgumentException($"Command config tool tip resource not defined for {config.Key}");

                bool toolTipPathExists = resourceManifest.Contains(toolTipPath + ".resources");
                if (!toolTipPathExists)
                    throw new ArgumentException($"Command config tool tip resource does not exist for {config.Key}");
            }
        }

        /// <summary>
        /// Return the command providing completions for a given command or alias.
        /// 
        /// If the alias is null, or completions for the command do not exist, return null.
        /// </summary>
        /// <param name="alias">Command or alias for which completions required.</param>
        /// <returns>Name of the command implementing completions.</returns>
        /// <exception cref="SyntaxTreeException">Alias does not exist in the syntax trees configuration.</exception>
        internal static string? CommandFromAlias(string? alias)
        {
            string? command = null;
            if (alias != null)
            {
                try
                {
                    command = SUPPORTED_COMMANDS[alias];
                }
                catch (KeyNotFoundException) { };
            }
            return command;
        }

        internal static string? Definition(string syntaxTreeName)
        {
            string? returnValue = null;
            try
            {
                returnValue = RESOURCE_ROOT + COMMAND_CONFIGS[syntaxTreeName].Definition;
            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"No config for {syntaxTreeName}");
            }
            return returnValue;
        }

        internal static string? ToolTips(string syntaxTreeName)
        {
            string? returnValue = null;
            try
            {
                returnValue = RESOURCE_ROOT + COMMAND_CONFIGS[syntaxTreeName].ToolTips;
            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"No config for {syntaxTreeName}");
            }
            return returnValue;
        }

        internal static ParseMode ParseMode(string? syntaxTreeName)
        {
            ParseMode returnValue = SyntaxTreeSpecs.ParseMode.Windows;
            if (string.IsNullOrWhiteSpace(syntaxTreeName)) return returnValue;

            try
            {
                returnValue = COMMAND_CONFIGS[syntaxTreeName].ParseMode;
            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"No config for {syntaxTreeName}");
            }
            return returnValue;

        }

    }
}
