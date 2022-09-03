// TODO [ ][POWERSHELL] Raise issue: The HelpMessageBaseName/ResourceId does not generate help text.

namespace PoshPredictiveText
{
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Text;


    /// <summary>
    /// The Posh Predictive Text cmdlet provides suggested completions for
    /// command arguments in PowerShell.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Install,
        "PredictiveText",
        DefaultParameterSetName = "Resolve")]
    [OutputType(typeof(string))]
    [OutputType(typeof(CompletionResult))]
    public class PoshPredictiveTextCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the parameter requesting the version to be display.
        /// </summary>
        [Parameter(
            ParameterSetName = "Version",
            HelpMessage = "Returns the version.")]
        [Alias("Ver", "v")]
        public SwitchParameter Version { get; set; }

        /// <summary>
        /// Gets or sets the switch indicating List, ListCommand or L flag
        /// supported commands.
        /// </summary>
        [Parameter(
            ParameterSetName = "ListCommands",
            HelpMessage = "List commands supported with predictive text.",
            HelpMessageBaseName = "PoshPredictiveText.UIStrings",
            HelpMessageResourceId = "HELP_LIST_COMMANDS")]
        [Alias("List", "l")]
        public SwitchParameter ListCommands { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [Parameter(
            ParameterSetName = "Initialise",
            HelpMessage = "Initialise predictive texts.")]
        [Alias("Init", "i")]
        public SwitchParameter Initialise { get; set; }

        /// <summary>
        /// Gets or sets the file to which messages will be written.
        /// </summary>
        [Parameter(
            ParameterSetName = "Initialise",
            HelpMessage = "Enable logging to log file.")]
        public string? LogFile { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [ValidateSet("INFO", "WARN", "ERROR", IgnoreCase = true)]
        [Parameter(
            ParameterSetName = "Initialise",
            HelpMessage = "Level of information to log (INFO, WARN, ERROR). Default: ERROR.")]
        public string? LogLevel { get; set; }

        /// <summary>
        /// Gets or sets a flag to print the PowerShell script used to initialise tab-exapansion
        /// of arguments.
        /// </summary>
        [Parameter(
            ParameterSetName = "PrintScript",
            HelpMessage = "Print PowerShell script used to initialise Powershell Predictive Text.")]
        [Alias("Print", "p")]
        public SwitchParameter PrintScript { get; set; }

        /// <summary>
        /// Gets or sets the partial word provided before the user pressed Tab.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = "Resolve",
            HelpMessage = "Value provided by the user before they pressed tab.")]
        public string? WordToComplete { get; set; }

        /// <summary>
        /// Gets or sets the Abstract Syntax Tree (AST) for the current input line.
        /// </summary>
        [Parameter(
            Position = 1,
            ParameterSetName = "Resolve",
            HelpMessage = "Abstract Syntax Tree for current input line.")]
        public CommandAst? CommandAst { get; set; }

        /// <summary>
        /// Gets or sets the position of the cursor when tab was pressed.
        /// </summary>
        [Parameter(
            Position = 2,
            ParameterSetName = "Resolve",
            HelpMessage = "Command enered by user at the prompt.")]
        public int? CursorPosition { get; set; }

        /// <summary>
        /// Main entry point for the cmdlet.
        /// 
        /// Resolves input parameters and initiates further action.
        /// 
        /// Returns response to calling application.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var result = new StringBuilder();

            switch (ParameterSetName)
            {
                case "Version":
                    result.Append(UI.Resource("VERSION"));
                    WriteObject(result.ToString());
                    break;

                case "ListCommands":
                    result.Append(UI.Resource("LIST_OF_COMMANDS"));
                    WriteObject(result.ToString());
                    break;

                case "Initialise":
                    // For predictive text exceptions are silently suppressed and errors
                    // reported to a log file to facilitate debugging. However, prior to
                    // successful creation of the log file exceptions are captured and
                    // re-thrown as a LoggerException which is then output on the cmdlet
                    // error stream.
                    try
                    {
                        LOGGER.Initialise(LogFile, LogLevel);
                    }

                    catch (LoggerException ex)
                    {
                        WriteError(new ErrorRecord(
                        ex,
                        "Install-PoshPRedictiveText-Logger-Error",
                        ErrorCategory.InvalidArgument,
                        LogFile));
                    }

                    var init_script = UI.Resource("REGISTER_COMMAND_SCRIPT")
                                        .Replace("$cmdNames", SyntaxTreesConfig.SupportedCommands());
                    try
                    {
                        InvokeCommand.InvokeScript(init_script);
                    }
                    catch(Exception ex) when (
                    ex is ParseException
                    || ex is RuntimeException
                    || ex is FlowControlException)
                    {
                        WriteError(new ErrorRecord(
                        ex,
                        "Install-PoshPRedictiveText-InitialiseScript-Error",
                        ErrorCategory.InvalidOperation,
                        LogFile));
                    }

                    string? finalResult = result.ToString();
                    if (!string.IsNullOrWhiteSpace(finalResult))
                        WriteObject(finalResult);

                    break;

                case "PrintScript":
                    var print_script = UI.Resource("REGISTER_COMMAND_SCRIPT")
                                         .Replace("$cmdNames", SyntaxTreesConfig.SupportedCommands());
                    result.Append(print_script);
                    WriteObject(result.ToString());
                    break;

                case "Resolve":
                    // The algorithm uses the command abstract syntrax tree to tokenise the input text. 
                    // If it is not available then return no values.
                    LOGGER.Write("Processing...");
                    if (CommandAst is not null)
                    {
                        // Convert the CommandAst to a list of tokens which will be used to evaluate
                        // which options are avaialble for the next parameter.
                        var enteredTokens = new CommandAstVisitor();
                        CommandAst.Visit(enteredTokens);
#if DEBUG
                        LOGGER.Write("Resolving word: " + WordToComplete??"");
                        LOGGER.Write("Resolving AST: " + CommandAst);
                        LOGGER.Write($"Base Command: {enteredTokens.BaseCommand ?? "Caught null"}");
                        LOGGER.Write($"Last Command: {enteredTokens.LastToken?.Value ?? "Caught null"}");
                        LOGGER.Write($"Prior Command: {enteredTokens.PriorToken?.Value ?? "Does not exist."}");
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
                            LOGGER.Write(ex.ToString(), LOGGER.LOGLEVEL.ERROR);
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
                    break;

                default:
                    // TODO [ ][USERINTERFACE] All cases where we don't have a parameter set.
                    result.Append("Default we should print help text.");
                    WriteObject(result.ToString());
                    break;
            }
        }
    }
}