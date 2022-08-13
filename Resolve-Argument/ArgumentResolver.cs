
// TODO [RESOLVER] Support return of aliases.
// TODO [RESOLVER] Consider whether the completion has already been enetered and is, therefore, not appropriate as a suggestion.
// TODO [RESOLVER] Provide guidance on parameter values and suggest alternatives (e.g. conda environments).
// TODO [CONDA] Implement missing conda commands not included within the documentation (conda activate/deactivate, env options). 

namespace ResolveArgument
{
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

    // TODO Move syntaxItem to LoadSyntaxTree.cs
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
        internal Suggestion(string completionText, string listText, CompletionResultType type, string toolTip)
        {
            CompletionText = completionText;
            ListText = listText;
            Type = type;
            ToolTip = toolTip;
        }
        internal string CompletionText { get; init; }
        internal string ListText { get; init; }
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
        /// 
        /// The state model for determining suggestions uses the following algorithm:
        /// [X] 1. Identify what command, or partial command has already been entered (commands may be multi-word).
        /// [X] 2. Identify if we have exited command entry (a parameter has been entered) skip to 4.
        /// [X] 3. Identify suggestions for sub-commands.
        /// [ ] 4. Identify suggestions for parameter values if command parameter is active. If mandatory value skip to 7.
        /// [ ] 5. Identify suggestions for positional parameters using a handler if appropriate
        /// [ ] 6. Identify suggestions for command parameters.
        /// [ ] 7. Identify whether we have already entered command parameters which are unique (remove from suggestions).
        /// </remarks>
        internal static List<Suggestion> Suggestions(string WordToComplete, CommandAstVisitor commandTokens, int CursorPosition)
        {
            List<Suggestion> suggestions = new();

            if (commandTokens.BaseCommand is not null)
            {
                var baseCommand = (Token)commandTokens.BaseCommand;
                string syntaxTreeName = baseCommand.text;

                // If the syntax tree does not exist then try and load it.
                if (!SyntaxTreeExists(syntaxTreeName)) LoadSyntaxTree(syntaxTreeName);

                // If successfully loaded then continue to process suggestions.
                if (SyntaxTreeExists(syntaxTreeName))
                {
                    List<SyntaxItem> syntaxTree = syntaxTrees[syntaxTreeName];
#if DEBUG
                    LOGGER.Write($"The syntaxTree exists. There are {syntaxTree.Count} entries in the tree.");
#endif

                    // Extract unique commands from the syntax tree and then
                    // evaluate what command and sub-commands have been entred
                    // so far.
                    var uniqueCommands = syntaxTree
                        .Select(item => item.command)
                        .Distinct()
                        .ToList();

                    StringBuilder commandPath = new(capacity: 64);
                    int tokensInCommand = 0;
                    foreach (Token commandToken in commandTokens.All)
                    {
                        if (uniqueCommands.Contains(commandToken.text))
                        {
                            if (commandPath.Length > 0)
                            {
                                commandPath.Append('.');
                            }
                            commandPath.Append(commandToken.text);
                            tokensInCommand++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // If we have more tokens than tokens in the command then the command is complete.
                    // If true then we do not need to propose any further sub-commands.
                    bool commandComplete = commandTokens.All.Count > tokensInCommand;
# if DEBUG
                    LOGGER.Write($"The command is: {commandPath}. There are {tokensInCommand} tokens in the command.");
                    if (commandComplete) LOGGER.Write("The command is complete.");
# endif
                    // Get options relevant to the command excluding sub-commands if command is already complete.
                    var availableOptions = syntaxTree
                        .Where(syntaxItem =>
                                !(syntaxItem.type.Equals("CMD") && commandComplete)
                                && (syntaxItem.commandPath == commandPath.ToString()))
                        .ToList();
#if DEBUG
                    LOGGER.Write("This command has the following options:");
                    foreach (var option in availableOptions)
                    {
                        LOGGER.Write($"::Option -> {option.argument} ({option.type})");
                    }
#endif
# if DEBUG
                    // Output count of option types.
                    var optionCounts = from option in availableOptions
                                       group option by option.type into typeGroup
                                       select new
                                       {
                                           Type = typeGroup.Key,
                                           Count = typeGroup.Count()
                                       };

                    StringBuilder countsString = new(capacity: 64);
                    countsString.Append("Total tokens by type:");
                    foreach (var option in optionCounts)
                    {
                        countsString.Append($"{option.Type} ({option.Count}) ");
                    }
                    countsString.Append('.');
                    LOGGER.Write(countsString.ToString());
# endif

                    // Iterate backwards through tokens to find the index of the last command parameter
                    int ? lastCommandParameterIndex = null;
                    for (int index = commandTokens.All.Count - 1; index >= 0; index--)
                    {
                        if (commandTokens.All[index].type == typeof(System.Management.Automation.Language.CommandParameterAst))
                        {
                            lastCommandParameterIndex = index;
                            break;
                        }
                    }

#if DEBUG
                    if (lastCommandParameterIndex is not null)
                    {
                        LOGGER.Write($"Last command parameter is at index {lastCommandParameterIndex}"
                            + $" is {commandTokens.All[lastCommandParameterIndex ?? 0].text}");
                    }
#endif

                    var filteredOptions = availableOptions
                        .Where(item => item.argument is not null && (item.argument).StartsWith(WordToComplete))
                        .ToList();

                    foreach (var item in filteredOptions)
                    {
                        Suggestion suggestion = new(
                            item.argument??"",
                            item.argument??"",
                            CompletionResultType.ParameterName,
                            SyntaxTree.Tooltip(syntaxTreeName, item.toolTip)??"Tooltip was null."
                        );

                        suggestions.Add(suggestion);
                    }
#if DEBUG
                    LOGGER.Write($"The algorithm is providing {suggestions.Count} suggestions.");
                    foreach (var suggestion in suggestions)
                    {
                        LOGGER.Write($"::Suggestion -> {suggestion.CompletionText}");
                    }
#endif
                }
            }
            return suggestions;
        }

    }
}
