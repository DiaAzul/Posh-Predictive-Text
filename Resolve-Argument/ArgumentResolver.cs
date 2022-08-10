﻿
namespace ResolveArgument
{
    using System.Linq;
    using System.Xml.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Reflection;
    using System.IO;

    /// <summary>
    /// Record within the command syntax tree.
    /// 
    /// Used to enumerate query results on the XML syntax tree.
    /// </summary>
    internal struct SyntaxItem
    {
        internal string? command;
        internal string? commandPath;
        internal string? type;
        internal string? commandGroup;
        internal string? argument;
        internal string? alias;
        internal bool? multipleUse;
        internal string? parameter;
        internal bool? multipleParameters;
        internal string? toolTip;

        internal string AsString()
        {
            return $"{command}, {commandPath}, {type}, {commandGroup}, {argument}, {alias}, {multipleUse}, {parameter}, {multipleParameters}, {toolTip}";
        }
    }

    /// <summary>
    /// Process tokenised input string and return suggested tab-completions.
    /// </summary>
    internal class ArgumentResolver
    {
        private static XDocument? syntaxTree;


        /// <summary>
        /// Load the syntax tree for which completions required.
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to load.</param>
        internal static void LoadSyntaxTree(string syntaxTreeName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                var resourceStream = assembly.GetManifestResourceStream("Resolve_Argument.SyntaxTrees.CondaSyntaxTree.xml");

                if (resourceStream != null)
                {
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        var xmlDoc = reader.ReadToEnd();
                        syntaxTree = XDocument.Parse(xmlDoc);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                LOGGER.Write("File not found.");
            }
            catch (BadImageFormatException)
            {
                LOGGER.Write("File wrong format.");
            }
            catch (FileLoadException)
            {
                LOGGER.Write("File was found, could not load.");
            }

        }

        /// <summary>
        /// Test that a syntax tree is loaded.
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to test.</param>
        /// <returns>True if syntax tree is loaded.</returns>  
        internal static bool SyntaxTreeExists(string syntaxTreeName)
        {
            return syntaxTree != null;
        }
        
        /// <summary>
        /// Processes the command line tokens and suggests completions for the wordToComplete.
        /// </summary>
        /// <param name="WordToComplete">Word for which suggested comlpetions required.</param>
        /// <param name="commandTokens">Tokenised text on the command line.</param>
        /// <param name="CursorPosition">Position of the cursor on the command line.</param>
        /// <returns>Suggested list of completions for the word to complete.</returns>
        /// <remarks>
        /// The method loads the syntax tree for the command if it not already loaded. It then
        /// identifies whether a mulit-word command has been entered, for example <c>conda create</c>.
        /// It then identifies possible tokens for that command and identifies whether we are entering
        /// a parameter or values. Where tab-completion for values are required then method identifies
        /// and calls an appropriate handler.
        /// </remarks>
        internal static List<CompletionResult> Suggestions(string WordToComplete, CommandAstVisitor commandTokens, int CursorPosition)
        {
            List<CompletionResult> suggestions = new();

            if (commandTokens.BaseCommand != null)
            {
                var baseCommand = (Token) commandTokens.BaseCommand;
                if (!SyntaxTreeExists(baseCommand.text)) LoadSyntaxTree(baseCommand.text);


                // TODO Resolve where we are in the token tree. First token (Link search for exact matches). -> Next options
                // Search next options -> if no returns then previous search has the options available.

                LOGGER.Write("Creating query.");

                LOGGER.Write($"The syntaxTree exists: {syntaxTree != null}");

                XElement? root = syntaxTree?.Root;

                LOGGER.Write($"The root exists: {root != null}");

                IEnumerable<SyntaxItem>? commandQuery = null;
                if (root != null)
                {
                    try
                    {
                        commandQuery = from item in root.Elements("item")
                                       select new SyntaxItem
                                       {
                                           command = (string)item.Element("COMMAND"),
                                           commandPath = (string)item.Element("COMMAND_PATH"),
                                           type = (string)item.Element("TYPE"),
                                           commandGroup = (string)item.Element("COMMAND_GROUP"),
                                           argument = (string)item.Element("ARGUMENT"),
                                           alias = (string)item.Element("ALIAS"),
                                           multipleUse = ((string)item.Element("MULTIPLE_USE")) == "TRUE",
                                           parameter = (string)item.Element("PARAMETER"),
                                           multipleParameters = ((string)item.Element("MULTIPLE_PARAMETER")) == "TRUE",
                                           toolTip = (string)item.Element("TOOLTIP")
                                       };
                        LOGGER.Write($"Hi! The command query exists: {commandQuery != null}");
                    }
                    catch(Exception e)
                    {
                        LOGGER.Write(e.ToString());
                    }
                }
                LOGGER.Write($"Command query exists: {commandQuery != null}");
                LOGGER.Write("Executing query.");
                List<SyntaxItem> list = new List<SyntaxItem>();

                if (commandQuery != null)
                {
                    try
                    {
                        list = commandQuery.ToList();
                    }
                    catch (Exception e)
                    {
                        LOGGER.Write(e.ToString());
                    }
                }

                LOGGER.Write($"List length = {list.Count}");

                var firstItem = list[0];

                LOGGER.Write($"First list item: {firstItem.AsString()}");

                // total_tokens = commandTokens.Length
                // commandTokensIndex = 0
                // Command_Path = ""
                // Get token[commandTokensIndex]
                // If Type = System.Management.Automation.ParameterAst -> 
                // Either another command or positional parameter.


                CompletionResult response = new(
                    WordToComplete + "a",
                    WordToComplete + "a",
                    CompletionResultType.ParameterName, //Need to change this for ast.
                    "ToolTip");

                CompletionResult response2 = new(
                    WordToComplete + "b",
                    WordToComplete + "b",
                    CompletionResultType.ParameterName, //Need to change this for ast.
                    "ToolTip");

                suggestions.Add(response);
                suggestions.Add(response2);

                LOGGER.Write($"Providing suggestions...{WordToComplete}a, b.");
            }

            return suggestions;
        }

    }
}
