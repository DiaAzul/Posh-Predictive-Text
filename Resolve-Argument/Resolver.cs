// TODO [ ][RESOLVER] Support return of aliases.

namespace ResolveArgument
{
    using System.Linq;
    using System.Management.Automation;

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
        /// TODO [X] 1. Identify what command, or partial command has already been entered (commands may be multi-word).
        /// TODO [X] 2. Identify if we have exited command entry (a parameter has been entered) skip to 4.
        /// TODO [X] 3. Identify suggestions for sub-commands.
        /// TODO [X] 4. Identify suggestions for parameter values if command parameter is active. If mandatory value skip to 7.
        /// TODO [X] 5. Identify suggestions for positional parameters using a handler if appropriate
        /// TODO [X] 6. Identify suggestions for command parameters.
        /// TODO [ ] 7. Identify whether we have already entered command parameters which are unique (remove from suggestions).
        /// </remarks>
        internal static List<Suggestion> Suggestions(
            string wordToComplete,
            CommandAstVisitor enteredTokens,
            int cursorPosition)
        {
            // ----- INITIALISE -----
            // Check whether the syntax tree exists and load if not already loaded.
            // If we can't load a syntax tree then return early with empty suggestion list.
            List<Suggestion> suggestions = new();

            string syntaxTreeName = enteredTokens.BaseCommand ?? "";
            // If syntax tree not loaded then load it. If still not loaded abort early.
            if (!SyntaxTrees.Exists(syntaxTreeName)) SyntaxTrees.Load(syntaxTreeName);
            if (!SyntaxTrees.Exists(syntaxTreeName)) return suggestions;

#if DEBUG
            LOGGER.Write($"The syntaxTree {syntaxTreeName} exists. "
                + $"There are {SyntaxTrees.Count(syntaxTreeName)} entries in the tree.");
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
            List<string> uniqueCommands = SyntaxTrees.UniqueCommands(syntaxTreeName);
            var (commandPath, tokensInPath) = enteredTokens.CommandPath(uniqueCommands);
            LOGGER.Write($"Tokens in command {tokensInPath}. Entered Tokens {enteredTokens.Count}");

            List<SyntaxItem> filteredSyntaxTree = SyntaxTrees.Get(syntaxTreeName)
                .Where(syntaxItem =>
                            syntaxItem.commandPath == commandPath)
                .ToList();

            List<SyntaxItem> subCommands = filteredSyntaxTree
                                            .Where(syntaxItem => syntaxItem.type == "CMD")
                                            .ToList();
            int countOfEnteredTokens = enteredTokens.Count + (wordToComplete == "" ? 1 : 0);
            int expectedCommandTokens = tokensInPath + (subCommands.Count > 0 ? 1 : 0);
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
                int enteredValues = enteredTokens.Count - lastCommandPosition;
                // Can we enter more than one value?
                string lastParameter = enteredCommandParameters[lastCommandPosition].text;
                var parameterSyntaxItems = filteredSyntaxTree
                                .Where(syntaxItem => syntaxItem.parameter == lastParameter);

                if (parameterSyntaxItems is not null)
                {
                    var syntaxItem = parameterSyntaxItems.FirstOrDefault();
                    bool multipleParameterValues = syntaxItem?.multipleParameterValues ?? false;

                    if (enteredValues == 0 | multipleParameterValues)
                    {
                        List<Suggestion> parameterValueOptions = CondaHelpers
                                                                    .GetParamaterValues(
                                                                        syntaxItem?.parameter ?? "",
                                                                        wordToComplete);

                        suggestions.AddRange(parameterValueOptions);
                        // Don't provide any other suggestions if we must enter a parameter value.
                        listOnlyParameterValues = (enteredValues == 0);
                    }
                }
            }

            // ----- POSITIONAL VALUES -----
            // If we have a helper for positional parameters then return the suggestions.
            if (!listOnlyParameterValues && commandComplete)
            {
                LOGGER.Write("Listing positional parameters.");
                var positionalValue = filteredSyntaxTree
                                        .Where(syntaxItem => syntaxItem.type == "POS")
                                        .ToList();
                if (positionalValue.Count > 0)
                {
                    SyntaxItem positionalSyntaxItem = positionalValue.First();
                    LOGGER.Write(positionalSyntaxItem.parameter ?? "");
                    List<Suggestion> positionalValueSuggestions = CondaHelpers
                                                                     .GetParamaterValues(
                                                                        positionalSyntaxItem.parameter ?? "",
                                                                        wordToComplete);

                    suggestions.AddRange(positionalValueSuggestions);
                }
            }

            // ----- SUB-COMMANDS, OPTIONAL and PARAMETER TOKENS -----
            // If the command is not complete then offer sub-commands.
            // Filter tokens already added from optional and parameter suggestions.
            if (!listOnlyParameterValues)
            {
                List<SyntaxItem> availableOptions = filteredSyntaxTree
                    .Where(syntaxItem =>
                            !(syntaxItem.type.Equals("CMD") && commandComplete)
                            && syntaxItem.argument is not null
                            && syntaxItem.argument.StartsWith(wordToComplete))
                    .ToList();

                // TODO [ ][RESOLVER] Filter parameters by those already added.
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
