
// TODO [RESOLVER] Support return of aliases.
// TODO [RESOLVER] Consider whether the completion has already been enetered and is, therefore, not appropriate as a suggestion.
// TODO [RESOLVER] Provide guidance on parameter values and suggest alternatives (e.g. conda environments).
// TODO [CONDA] Implement missing conda commands not included within the documentation (conda activate/deactivate). 

namespace ResolveArgument
{
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    /// <summary>
    /// Record within the command syntax tree.
    /// 
    /// Used to enumerate query results on the XML syntax tree.
    /// </summary>
    internal struct SyntaxItem
    {
        internal string command;
        internal string commandPath;
        internal string type;
        internal string? argument;
        internal string? alias;
        internal bool? multipleUse;
        internal string? parameter;
        internal bool? multipleParameters;
        internal string? toolTip;

        internal string AsString()
        {
            return $"{command}, {commandPath}, {type}, {argument}, {alias}, {multipleUse}, {parameter}, {multipleParameters}, {toolTip}";
        }
    }

    /// <summary>
    /// Suggested response.
    /// 
    /// This implements a data structure returned by the completion class and used by the calling
    /// class to generate specific data structures for the calling application.
    /// </summary>
    internal readonly struct Suggestion
    {
        internal Suggestion(string completionText, CompletionResultType type, string toolTip)
        {
            CompletionText = completionText;
            Type = type;
            ToolTip = toolTip;
        }
        internal string CompletionText { get; init; }
        internal CompletionResultType Type { get; init; }
        internal string ToolTip { get; init; }

        public override string ToString() => $"{CompletionText}";
    }

    /// <summary>
    /// Process tokenised input string and return suggested tab-completions.
    /// </summary>
    internal class ArgumentResolver
    {
        /// <summary>
        /// Each command has a syntax tree which sets out the possible combination of tokens
        /// on the command line. The trees are strored as XML resource files embeded within
        /// the application. These are loaded, parsed and converted to a list of Syntax items
        /// when a command requires tab-completions.
        /// </summary>
        private static readonly Dictionary<string, List<SyntaxItem>> syntaxTrees = new();


        /// <summary>
        /// Loads the syntax tree for a named command into the dictionary of syntax trees.
        /// 
        /// The method reads the XML file embeded within the application, parses it
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to load.</param>
        internal static void LoadSyntaxTree(string syntaxTreeName)
        { 
            // Load syntax tree
            var syntaxTree = SyntaxTree.Load(syntaxTreeName);

            // If the syntax tree loaded than add to the dictionary.
            if (syntaxTree.Any())
            {
                syntaxTrees[syntaxTreeName] = syntaxTree;
            }
            else
            {
                syntaxTrees.Remove(syntaxTreeName);
            }
        }

        /// <summary>
        /// Test that a syntax tree is loaded.
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to test.</param>
        /// <returns>True if syntax tree is loaded.</returns>  
        internal static bool SyntaxTreeExists(string syntaxTreeName)
        {
            return syntaxTrees.ContainsKey(syntaxTreeName);
        }

        /// <summary>
        /// Gets the syntax tree name from the base command token.
        /// 
        /// This method should text that the command is valid and resolve
        /// any aliases to the correct tree.
        /// </summary>
        /// <param name="baseCommandToken"></param>
        /// <returns>Syntax tree name.</returns>
        internal static string SyntaxTreeName(Token baseCommandToken)
        {
            // TODO: [SYNTAXTREES] Manage aliases for the syntax tree (e.g. mamba -> conda).
            return baseCommandToken.text;
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
        internal static List<Suggestion> Suggestions(string WordToComplete, CommandAstVisitor commandTokens, int CursorPosition)
        {
            List<Suggestion> suggestions = new();

            if (commandTokens.BaseCommand != null)
            {
                var baseCommand = (Token)commandTokens.BaseCommand;
                string syntaxTreeName = baseCommand.text;

                // If the syntax tree does not exist then try and load it.
                if (!SyntaxTreeExists(syntaxTreeName)) LoadSyntaxTree(syntaxTreeName);

                // If successfully loaded then continue to process suggestions.
                if (SyntaxTreeExists(syntaxTreeName))
                {
                    List<SyntaxItem> syntaxTree = syntaxTrees[syntaxTreeName];
                    LOGGER.Write($"The syntaxTree exists. Length: {syntaxTree.Count}");

                    //var uniqueCommands = (from item in syntaxTree
                    //                     group item.command by item.command into uniqueCommand
                    //                     select uniqueCommand).Keys.ToList();
                    var uniqueCommands = syntaxTree
                        .Select(item => item.command)
                        .Distinct()
                        .ToList();

                    // Identify where we are in command chain.
                    StringBuilder commandPath = new(capacity: 64);
                    foreach (Token commandToken in commandTokens.All)
                    {
                        if (uniqueCommands.Contains(commandToken.text))
                        {
                            if (commandPath.Length > 0)
                            {
                                commandPath.Append('.');
                            }
                            commandPath.Append(commandToken.text);
                        }
                        else
                        {
                            break;
                        }
                    }

                    LOGGER.Write($"Command path: {commandPath}");

                    // Note: Test argument not null to prevent nulls in filteredOptions linq query.
                    var availableOptions = syntaxTree
                        .Where(item => item.commandPath == commandPath.ToString() & item.argument != null)
                        .ToList();

                    foreach (var option in availableOptions)
                    {
                        LOGGER.Write($"Options -> {option.argument}");
                    }

#pragma warning disable CS8602 
                    // Dereference of a possibly null reference
                    // Guarded with item.argument != null in availableOptions Linq query.
                    var filteredOptions = availableOptions
                        .Where(item => item.argument.StartsWith(WordToComplete))
                        .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    foreach (var item in filteredOptions)
                    {
                        Suggestion suggestion = new(
                            item.argument??"",
                            CompletionResultType.ParameterName,
                            SyntaxTree.Tooltip(syntaxTreeName, item.toolTip)
                        );

                        suggestions.Add(suggestion);
                    }

                    LOGGER.Write($"Providing {suggestions.Count} suggestions.");
                    foreach (var suggestion in suggestions)
                    {
                        LOGGER.Write($"I suggest -> {suggestion.CompletionText}");
                    }
                }
            }

            return suggestions;
        }

    }
}
