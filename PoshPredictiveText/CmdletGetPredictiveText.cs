
namespace PoshPredictiveText
{
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using static PoshPredictiveText.LOGGER;

    /// <summary>
    /// The Posh Predictive Text cmdlet provides suggested completions for
    /// command arguments in PowerShell.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        "PredictiveText")]
    [OutputType(typeof(string))]
    [OutputType(typeof(CompletionResult))]
    public class GetPredictiveText: PSCmdlet
    {
        /// <summary>
        /// Gets or sets the partial word provided before the user pressed Tab.
        /// </summary>
        [Parameter(
            Position = 0,
            HelpMessage = "Value provided by the user before they pressed tab.")]
        public string? WordToComplete { get; set; }

        /// <summary>
        /// Gets or sets the Abstract Syntax Tree (AST) for the current input line.
        /// </summary>
        [Parameter(
            Position = 1,
            HelpMessage = "Abstract Syntax Tree for current input line.")]
        public CommandAst? CommandAst { get; set; }

        /// <summary>
        /// Gets or sets the position of the cursor when tab was pressed.
        /// </summary>
        [Parameter(
            Position = 2,
            HelpMessage = "Command enered by user at the prompt.")]
        public int? CursorPosition { get; set; }

        /// <summary>
        /// Main entry point for the cmdlet.
        /// 
        /// Resolves input parameters and initiates further action.
        /// 
        /// Returns response to calling application.
        /// </summary>
        protected override void EndProcessing()
        {
            // The algorithm uses the command abstract syntrax tree to tokenise the input text. 
            // If it is not available then return no values.
            Write("Processing...");
            if (CommandAst is not null)
            {
                // Convert the CommandAst to a list of tokens which will be used to evaluate
                // which options are avaialble for the next parameter.
                var enteredTokens = new CommandAstVisitor();
                CommandAst.Visit(enteredTokens);
#if DEBUG
                Write("Resolving word: " + WordToComplete??"");
                Write("Resolving AST: " + CommandAst);
                Write($"Base Command: {enteredTokens.BaseCommand ?? "Caught null"}");
                Write($"Last Command: {enteredTokens.LastToken?.Value ?? "Caught null"}");
                Write($"Prior Command: {enteredTokens.PriorToken?.Value ?? "Does not exist."}");
#endif
                // Get suggested tab-completions. Not input parameters use null coalescing operator to gate nulls.
                List<Suggestion> suggestions = new();
                try
                {
                    suggestions = Resolver.Suggestions(
                        wordToComplete: WordToComplete ?? "",
                        enteredTokens: enteredTokens,
                        cursorPosition: CursorPosition ?? CommandAst.ToString().Length
                    );
                }
                // Process any errors raised. Raise an error if DEBUG build,
                // for RELEASE builds swallow excpetions (better to do nothing than
                // interrupt entry at the prompt when the user can do nothing to fix
                // the problem).
                catch (Exception ex)
                {
                    Write(ex.ToString(), LOGGER.LOGLEVEL.ERROR);
#if DEBUG
                    switch (ex)
                    {
                        case SyntaxTreeException:
                            WriteError(new ErrorRecord(
                                ex,
                                "Error-loading-syntax-tree",
                                ErrorCategory.ObjectNotFound,
                                enteredTokens));
                            break;
                        default:
                            WriteError(new ErrorRecord(
                                ex,
                                "Error-processing-record",
                                ErrorCategory.InvalidOperation,
                                enteredTokens));
                            break;
                    }
#endif
                }

                // Repackage suggestions for PowerShell.
                List<CompletionResult> cmdletSuggestions = new();
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

                WriteObject(cmdletSuggestions);
            }
            base.EndProcessing();
        }
    }
}