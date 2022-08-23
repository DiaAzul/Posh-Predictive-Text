
// TODO [ ] [RESOLVER] Support return of aliases.
// TODO [ ] [RESOLVER] Consider whether the completion has already been enetered and is, therefore, not appropriate as a suggestion.
// TODO [ ] [RESOLVER] Provide guidance on parameter values and suggest alternatives (e.g. conda environments).
// BUG  [X] [RESOLVER] Giving first letter of subcommand is filtering out All subcommand in options list.
// TODO [X] [CONDA] Implement missing conda commands not included within the documentation (conda activate/deactivate, env options). 

namespace Resolve_Argument
{
    using ResolveArgument;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;

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
    internal class Resolver
    {
        /// <summary>
        /// Processes the command line tokens and suggests completions for the wordToComplete.
        /// </summary>
        /// <param name="wordToComplete">Word for which suggested comlpetions required.</param>
        /// <param name="commandTokens">Tokenised text on the command line.</param>
        /// <param name="cursorPosition">Position of the cursor on the command line.</param>
        /// <returns>Suggested list of completions for the word to complete.</returns>
        /// <remarks>
        /// The method loads the syntax tree for the command if it not already loaded.
        /// It then identifies whether a mulit-word command has been entered, for example
        /// <c>conda create</c>. It then identifies possible tokens for that command and
        /// identifies whether we are entering a parameter or values. Where tab-completion
        /// for values are required then method identifies and calls an appropriate handler.
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
        internal static List<Suggestion> Suggestions(
            string wordToComplete,
            CommandAstVisitor commandTokens,
            int cursorPosition)
        {
            List<Suggestion> suggestions = new();

            if (commandTokens.BaseCommand is not null)
            {
                var baseCommand = (Token)commandTokens.BaseCommand;
                string syntaxTreeName = baseCommand.text;

                // If the syntax tree does not exist then try and load it.
                if (!SyntaxTrees.Exists(syntaxTreeName)) SyntaxTrees.Load(syntaxTreeName);
                // If successfully loaded then continue to process suggestions.
                if (SyntaxTrees.Exists(syntaxTreeName))
                {
                    
#if DEBUG
                    LOGGER.Write("The syntaxTree exists."
                        + $"There are {SyntaxTrees.Count(syntaxTreeName)} entries in the tree.");
#endif

                    // Extract unique commands from the syntax tree and then
                    // evaluate what command and sub-commands have been entred
                    // so far.
                    var uniqueCommands = SyntaxTrees.UniqueCommands(syntaxTreeName);

                    StringBuilder commandPath = new(capacity: 64);
                    int tokensInCommand = 0;
                    foreach (var (position, commandToken) in commandTokens.All)
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

                    // Filter the syntaxTree against the entered command to
                    // reduce the size of the tree for later processing.
                    List<SyntaxItem> filteredSyntaxTree = SyntaxTrees.Get(syntaxTreeName)
                        .Where(syntaxItem =>
                                    syntaxItem.commandPath == commandPath.ToString())
                        .ToList();

                    // Identify Command Parameters that have already been entered.
                    var enteredCommandParameters = commandTokens.CommandParameters;

#if DEBUG
                    foreach (var (position, Token) in enteredCommandParameters)
                    {
                        LOGGER.Write($"Entered parameter {Token.text} @ {position}");
                    }
#endif

                    // If we have more tokens than tokens in the command then the command
                    // is complete. If the tokens on the command line (including the final
                    // partial token to complete) exceeds the count of command tokens plus
                    // any other completed non command token then the command is complete.
                    // If true then we do not need to propose any further sub-commands.
                    bool commandComplete = commandTokens.All.Count > tokensInCommand + 1;
# if DEBUG
                    LOGGER.Write($"The command is: {commandPath}."
                        +" There are {tokensInCommand} tokens in the command.");
                    if (commandComplete) LOGGER.Write("The command is complete.");
# endif
                    // Get relevant commands, parameters and options that can complete
                    // the FINAL token.
                    // - Exclude subcommands if they cannot be added.
                    // - Exclude syntaxItems with no argument entry.
                    // - Filter list against the characters entered in the final token.
                    List<SyntaxItem> availableOptions = filteredSyntaxTree
                        .Where(syntaxItem =>
                                !(syntaxItem.type.Equals("CMD") && commandComplete)
                                && syntaxItem.argument is not null
                                && syntaxItem.argument.StartsWith(wordToComplete))
                        .ToList();
#if DEBUG
                    // Following lists all potential options from the syntax tree
                    // for the final token.
                    LOGGER.Write("This command has the following options:");
                    foreach (var option in availableOptions)
                    {
                        LOGGER.Write($"::Option -> {option.argument} ({option.type})");
                    }
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
                    // Do we have an active command parameter?
                    // Iterate backwards through tokens to find the index of the last
                    // command parameter
                    int? lastCommandParameterIndex = null;
                    for (int index = commandTokens.All.Count - 1; index >= 0; index--)
                    {
                        if (commandTokens.All[index].type == typeof(
                            System.Management.Automation.Language.CommandParameterAst))
                        {
                            lastCommandParameterIndex = index;
                            break;
                        }
                    }

#if DEBUG
                    if (lastCommandParameterIndex is not null)
                    {
                        LOGGER.Write(
                            $"Last command parameter is at index {lastCommandParameterIndex}"
                            + $" is {commandTokens.All[lastCommandParameterIndex ?? 0].text}");
                    }
#endif
                    var filteredOptions = availableOptions;

                    foreach (var syntaxItem in filteredOptions)
                    {
                        Suggestion suggestion = new(
                            syntaxItem.argument??"",
                            syntaxItem.argument??"",
                            syntaxItem.ResultType,
                            SyntaxTrees.Tooltip(syntaxTreeName, syntaxItem.toolTip)??"Tooltip was null."
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
