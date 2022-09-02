



namespace PoshPredictiveText
{
    using System.Collections;
    using System.Management.Automation;
    using System.Management.Automation.Language;


    /// <summary>
    /// Completer command
    /// https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.registerargumentcompletercommand?view=powershellsdk-7.0.0
    /// </summary>
    public class PoshPredictiveTextArgumentCompleter : RegisterArgumentCompleterCommand
    {

        /// <summary>
        /// Posh Predictive Text argument completer.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="parameterName"></param>
        /// <param name="wordToComplete"></param>
        /// <param name="commandAst"></param>
        /// <param name="fakeBoundParameters"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// 
        public static IEnumerable<CompletionResult> CompleteArgument(
            string commandName,
            string parameterName,
            string wordToComplete,
            CommandAst commandAst,
            IDictionary fakeBoundParameters)
        {
            List<CompletionResult> cmdletSuggestions = new();
            if (commandAst is null) return cmdletSuggestions;

            // Convert the CommandAst to a list of tokens which will be used to evaluate
            // which options are avaialble for the next parameter.
            var enteredTokens = new CommandAstVisitor();
            commandAst.Visit(enteredTokens);

            // Get suggested tab-completions. Not input parameters use null coalescing operator to gate nulls.
            List<Suggestion> suggestions = new();
            try
            {
                suggestions = Resolver.Suggestions(
                    wordToComplete: wordToComplete ?? "",
                    enteredTokens: enteredTokens,
                    cursorPosition: commandAst.ToString().Length
                );
            }
            // Process any errors raised. Raise an error if DEBUG build,
            // for RELEASE builds swallow excpetions (better to do nothing than
            // interrupt entry at the prompt when the user can do nothing to fix
            // the problem).
            catch (Exception ex)
            {
                LOGGER.Write(ex.ToString(), LOGGER.LOGLEVEL.ERROR);
            }

            // Repackage suggestions for PowerShell.

            foreach (var suggestion in suggestions)
            {
                CompletionResult cmdletSuggestion = new(
                    completionText: suggestion.CompletionText,
                    listItemText: suggestion.ListText,
                    resultType: suggestion.Type,
                    toolTip: suggestion.ToolTip
                    );

                cmdletSuggestions.Add(cmdletSuggestion);
            };

            return cmdletSuggestions;
        }
    }
}
