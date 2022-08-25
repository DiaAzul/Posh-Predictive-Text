
// TODO [ ][RESOLVER] Support return of aliases.
// TODO [ ][RESOLVER] Consider whether the completion has already been enetered and is, therefore, not appropriate as a suggestion.
// TODO [ ][RESOLVER] Provide guidance on parameter values and suggest alternatives (e.g. conda environments).
// BUG [ ][RESOLVER] Error thrown when base command (conda) and tab-complete empty next option.

namespace ResolveArgument
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
        /// TODO [ ] 5. Identify suggestions for positional parameters using a handler if appropriate
        /// TODO [ ] 6. Identify suggestions for command parameters.
        /// TODO [ ] 7. Identify whether we have already entered command parameters which are unique (remove from suggestions).
        /// </remarks>
        internal static List<Suggestion> Suggestions(
            string wordToComplete,
            CommandAstVisitor enteredTokens,
            int cursorPosition)
        {
            List<Suggestion> suggestions = new();

            if (enteredTokens.BaseCommand is not null)
            {
                var baseCommand = (Token)enteredTokens.BaseCommand;
                string syntaxTreeName = baseCommand.text;

                // Test syntax tree exists, if not try and load it, if can't load then skip suggestions.
                if (!SyntaxTrees.Exists(syntaxTreeName)) SyntaxTrees.Load(syntaxTreeName);
                if (SyntaxTrees.Exists(syntaxTreeName))
                {
#if DEBUG
                    LOGGER.Write("The syntaxTree exists."
                        + $"There are {SyntaxTrees.Count(syntaxTreeName)} entries in the tree.");
#endif
                    // Pre-process inputs:
                    // Get a list of unique commands from the syntax tree.
                    // Get the command path from the entered tokens.
                    // Identify any parameter arguments.
                    List<string> uniqueCommands = SyntaxTrees.UniqueCommands(syntaxTreeName);
                    var (commandPath, tokensInPath) = enteredTokens.CommandPath(uniqueCommands);
                    LOGGER.Write($"Tokens in command {tokensInPath}. Entered Tokens {enteredTokens.Count}");

                    // Filter the syntax tree extracting only those items that match the command path.
                    List<SyntaxItem> filteredSyntaxTree = SyntaxTrees.Get(syntaxTreeName)
                        .Where(syntaxItem =>
                                    syntaxItem.commandPath == commandPath)
                        .ToList();

                    // Identify if the command is complete. If the command is complete we do not
                    // need to suggest sub-commands, and can suggest positional parameters.
                    List<SyntaxItem> subCommands = filteredSyntaxTree
                                                    .Where(syntaxItem => syntaxItem.type == "CMD")
                                                    .ToList();
                    int countOfEnteredTokens = enteredTokens.Count + (wordToComplete == "" ? 1 : 0);
                    int expectedCommandTokens = tokensInPath + (subCommands.Count > 0 ? 1 : 0);
                    bool commandComplete = countOfEnteredTokens > expectedCommandTokens;

                    // Are we entering data for a parameter (POSITIONAL, PARAMETER)?
                    // Get last command token
                    bool listOnlyParameterValues = false;
                    Dictionary<int, Token> enteredCommandParameters = enteredTokens.CommandParameters;
                    if (enteredCommandParameters.Count > 0)
                    {
                        LOGGER.Write("Listing parameter values.");
                        // Calculate how many parameter values entered for the last parameter.
                        // Note: If we started entering a command parameter this will be identified as the last command.
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
                                List<string> parameterValueOptions = CondaHelpers
                                            .GetParamaterValues(syntaxItem?.parameter ?? "");

                                foreach (var item in parameterValueOptions)
                                {
                                    Suggestion suggestion = new(
                                        item,
                                        item,
                                        CompletionResultType.ParameterValue,
                                        ""
                                    );

                                    suggestions.Add(suggestion);
                                }
                                // Don't provide any other suggestions if we must enter a parameter value.
                                listOnlyParameterValues = (enteredValues == 0);
                            }
                        }
                    }
                    LOGGER.Write($"List only parameters {listOnlyParameterValues}. command complete {commandComplete}");
                    // Positional parameters
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
                            List<string> postionalValueOptions = CondaHelpers
                                                 .GetParamaterValues(positionalSyntaxItem.parameter ?? "");

                            var filteredSuggestions = postionalValueOptions
                                                                .Where(item => item.StartsWith(wordToComplete));

                            foreach (var suggestedText in filteredSuggestions)
                            {
                                Suggestion suggestion = new(
                                    suggestedText,
                                    suggestedText,
                                    CompletionResultType.ParameterValue,
                                    suggestedText
                                );

                                suggestions.Add(suggestion);
                            }
                        }
                    }

                    // Optional and Parameter keys.
                    if (!listOnlyParameterValues)
                    {
                        List<SyntaxItem> availableOptions = filteredSyntaxTree
                            .Where(syntaxItem =>
                                    !(syntaxItem.type.Equals("CMD") && commandComplete)
                                    && syntaxItem.argument is not null
                                    && syntaxItem.argument.StartsWith(wordToComplete))
                            .ToList();

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
