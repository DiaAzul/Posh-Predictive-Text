
namespace PoshPredictiveText
{
    using PoshPredictiveText.Helpers;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Linq;
    using System.Management.Automation;

    /// <summary>
    /// Suggested response.
    /// 
    /// Data structure returned by the completion class and used by the calling
    /// class to generate specific data structures for the calling application.
    /// </summary>
    internal record Suggestion
    {
        internal string CompletionText { get; init; } = default!;
        internal string ListText { get; init; } = default!;
        internal CompletionResultType Type { get; init; }
        internal string ToolTip { get; init; } = default!;
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
        /// <param name="enteredTokens">Tokenised text on the command line.</param>
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
        /// </remarks>
        internal static List<Suggestion> Suggestions(
            string wordToComplete,
            Tokeniser enteredTokens,
            int cursorPosition)
        {
            // ----- INITIALISE -----
            // Check whether the syntax tree exists and load if not already loaded.
            // If we can't load a syntax tree then return early with empty suggestion list.
            List<Suggestion> suggestions = new();

            string? syntaxTreeName = SyntaxTreesConfig.CommandFromAlias(enteredTokens.BaseCommand);
            // If syntax tree not loaded then load it. If still not loaded or command does exist abort early.

            SyntaxTree? syntaxTree = SyntaxTrees.Tree(syntaxTreeName);
            if (syntaxTree is null)
                return suggestions;

#if DEBUG
            LOGGER.Write($"The syntaxTree {syntaxTreeName} exists. "
                + $"There are {syntaxTree.Count} entries in the tree.");
#endif

            // ----- IDENTIFY SUB-COMMANDS -----
            // Many CLI apps use several tokens to identify sub-commands. These are parsed
            // from the entered tokens to identify the path in the syntax tree which
            // identifies the parameters for each sub-command. At each stage in the tree
            // we may have the option to enter a sub-command or parameters. If we have
            // entered parameters the it should not be possible to enter a sub-command,
            // therefore, we need to identify if this particular command is complete so
            // that we don't offer sub-commands as an option. Also, if the command is
            // complete we may offer positional parameters.
            List<string> uniqueCommands = syntaxTree.UniqueCommands;
            var (commandPath, tokensInPath) = enteredTokens.CommandPath(uniqueCommands);

            int countOfEnteredTokens = enteredTokens.Count + (wordToComplete == "" ? 1 : 0);
            int expectedCommandTokens = tokensInPath + (syntaxTree.CountOfSubCommands(commandPath) > 0 ? 1 : 0);
            bool commandComplete = countOfEnteredTokens > expectedCommandTokens;

            // ----- PARAMETER VALUES -----
            // If one of the previous tokens was a parameter token expecting a parameter value
            // then we should not suggest other tokens until the parameter value has been 
            // entered. If the paramter expects more than value then we can offer other
            // suggestions once more than one value is entered. If the parameter value has a
            // helper then the results of the helper should be suggested.
            bool listOnlyParameterValues = false;
            Dictionary<int, Token> enteredCommandParameters = enteredTokens.CommandParameters;
            if (enteredCommandParameters.Count > 0)
            {
                int lastCommandPosition = enteredCommandParameters.Keys.Max();
                int enteredValues = enteredTokens.Count - lastCommandPosition - 1;
                // Can we enter more than one value?
                string lastParameter = enteredCommandParameters[lastCommandPosition].Value;
                // Search prior parameters for both parameter AND aliases.
                var parameterSyntaxItems = syntaxTree.ParameterSyntaxItems(commandPath, lastParameter);

                if (parameterSyntaxItems.Count > 0)
                {
                    SyntaxItem syntaxItem = parameterSyntaxItems.First();
                    bool acceptsMultipleParameterValues
                        = syntaxItem?.MultipleParameterValues ?? false;
                    bool isParameter = syntaxItem?.IsParameter ?? false;
                    if (isParameter && (enteredValues == 0 | acceptsMultipleParameterValues))
                    {
                        List<Suggestion> parameterValueOptions
                            = CondaHelpers.GetParamaterValues(syntaxItem?.Parameter ?? "",
                                                              wordToComplete);
                        suggestions.AddRange(parameterValueOptions);
                        // If the parameter value is mandatory then don't provide
                        // any more suggestions.
                        listOnlyParameterValues = (enteredValues == 0);
                    }
                }
            }

            // ----- POSITIONAL VALUES -----
            // If we have a helper for positional parameters then return the suggestions.
            // BUG [HIGH][RESOLVER] If only one positional parameter allowed do not permit repeat suggstions.
            if (!listOnlyParameterValues && commandComplete)
            {
                LOGGER.Write("Listing positional parameters.");
                var positionalValues = syntaxTree.PositionalValues(commandPath);
                if (positionalValues.Count > 0)
                {
                    SyntaxItem positionalSyntaxItem = positionalValues.First();
                    LOGGER.Write(positionalSyntaxItem.Parameter ?? "");
                    List<Suggestion> positionalValueSuggestions
                        = CondaHelpers.GetParamaterValues(positionalSyntaxItem.Parameter ?? "",
                                                          wordToComplete);

                    suggestions.AddRange(positionalValueSuggestions);
                }
            }

            // ----- SUB-COMMANDS, OPTIONAL and PARAMETER TOKENS -----
            // If the command is not complete then offer sub-commands.
            // Filter tokens already added from optional and parameter suggestions.
            if (!listOnlyParameterValues)
            {
                List<SyntaxItem> availableOptions = syntaxTree.AvailableOptions(
                                                                    commandPath,
                                                                    commandComplete,
                                                                    enteredTokens,
                                                                    wordToComplete);

                foreach (var syntaxItem in availableOptions)
                {
                    Suggestion suggestion = new()
                    {
                        CompletionText = syntaxItem.Argument??"",
                        ListText = syntaxItem.Argument??"",
                        Type = syntaxItem.ResultType,
                        ToolTip = syntaxTree.Tooltip(syntaxItem.ToolTip) ?? "Tooltip was null."
                    };
                    suggestions.Add(suggestion);
                }
            }

            // ----- RETURN SUGGESTIONS -----
#if DEBUG
            LOGGER.Write($"The algorithm is providing {suggestions.Count} suggestions.");
            foreach (var suggestion in suggestions)
            {
                LOGGER.Write($"::Suggestion -> {suggestion.CompletionText}");
            }
#endif
            return suggestions;
        }

    }
}
