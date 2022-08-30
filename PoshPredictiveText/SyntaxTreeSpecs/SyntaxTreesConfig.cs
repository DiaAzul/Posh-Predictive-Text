﻿
using System.Text;

namespace PoshPredictiveText
{
    internal record ConfigItem
    {
        internal string? Definition { get; init; }
        internal string? ToolTips { get; init; }
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
        internal static readonly Dictionary<string, string> SUPPORTED_COMMANDS = new()
        {
            {"conda", "conda" },
            {"mamba", "conda" }  // Note, this is not strictly correct.
        };

        /// <summary>
        /// The root path to the syntax tree specifications. The name of the xml file 
        /// containing the syntax tree definitions and the name of the toolTip resouce
        /// files are defined in the COMMAND_CONFIGS dictionary. File names are added to
        /// the RESOURCE_ROOT to build the location of the relevant file.
        /// </summary>
        internal const string RESOURCE_ROOT = "PoshPredictiveText.SyntaxTreeSpecs.";

        internal static readonly Dictionary<string, ConfigItem> COMMAND_CONFIGS = new() {
            {"conda",  new ConfigItem {
                Definition = "CondaSyntaxTree.xml",
                ToolTips = "CondaToolTips" } },
        };

        /// <summary>
        /// Returns supported commands as a comma separated list.
        /// </summary>
        /// <returns>Supported commands as comma separated list.</returns>
        internal static string SupportedCommands()
        {
            StringBuilder commands = new();
            string delimeter = "";
            foreach(var command in SUPPORTED_COMMANDS.Keys)
            {
                commands.Append(delimeter);
                commands.Append(command);
                delimeter = ", ";
            }
            return commands.ToString();
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

    }
}