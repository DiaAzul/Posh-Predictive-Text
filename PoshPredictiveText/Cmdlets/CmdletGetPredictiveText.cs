
namespace PoshPredictiveText.Cmdlets
{
    using PoshPredictiveText.SemanticParser;
    using PoshPredictiveText.SyntaxTrees;
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
    public class GetPredictiveText : PSCmdlet
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
        /// Returns a list of suggested completions.
        /// </summary>
        protected override void EndProcessing()
        {
            Write("CMDLET: Processing.");
            if (CommandAst is null)
            {
                WriteObject(new List<CompletionResult>());
                base.EndProcessing();
                LOGGER.Write("CMDLET: Aborted. No abstract syntax tree.");
                return;
            }

            List<CompletionResult> cmdletSuggestions = new();
            using (SemanticCLICache semanticCLICache = new())
            if (semanticCLICache.Acquired)
            {
                LOGGER.Write("CMDLET: Acquired cached tokeniser.");
                SemanticCLI? semanticCLI = SemanticCLICache.Get(CommandAst.ToString());
                if (semanticCLI is null)
                {
                    LOGGER.Write("CMDLET: Creating tokeniser from cmdlet CommandAst");
                    Visitor visitor = new();
                    CommandAst.Visit(visitor);
                    if (WordToComplete == string.Empty)
                    {
                        visitor.BlankVisit(WordToComplete, CommandAst.Extent.EndColumnNumber, CommandAst.Extent.EndColumnNumber);
                    }
                    semanticCLI = visitor.SemanticCLI;
                    LOGGER.Write($"CMDLET: Finished visiting CommandAst. {semanticCLI.Count} tokens.");
                }
                else
                {
                    LOGGER.Write("CMDLET: Cmdlet using cached tokeniser.");
                }

                LOGGER.Write("CMDLET: Resolving word: " + WordToComplete??"");
                LOGGER.Write("CMDLET: Resolving AST: " + CommandAst);
                LOGGER.Write($"CMDLET: Base Command: {semanticCLI.BaseCommand ?? "Caught null"}");

                try
                {
                    if (semanticCLI?.LastToken?.Suggestions is not null)
                    {
                        cmdletSuggestions = semanticCLI.LastToken.Suggestions
                            .Select(suggestion => new CompletionResult(
                                completionText: suggestion.CompletionText,
                                listItemText: suggestion.ListText,
                                resultType: suggestion.Type,
                                toolTip: semanticCLI.SyntaxTree?.Tooltip(suggestion.ToolTip) ?? ""
                            ))
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    LOGGER.Write(ex.ToString(), LOGGER.LOGLEVEL.ERROR);
                    switch (ex)
                    {
                        case SyntaxTreeException:
                            WriteError(new ErrorRecord(
                                ex,
                                "Error-loading-syntax-tree",
                                ErrorCategory.ObjectNotFound,
                                semanticCLI));
                            break;
                        default:
                            WriteError(new ErrorRecord(
                                ex,
                                "Error-processing-record",
                                ErrorCategory.InvalidOperation,
                                semanticCLI));
                            break;
                    }
#endif
                }
            }
        
            LOGGER.Write($"CMDLET: Writing {cmdletSuggestions.Count} suggestions.");
            WriteObject(cmdletSuggestions);
            base.EndProcessing();
        }
    }
}