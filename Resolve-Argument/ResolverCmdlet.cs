
namespace ResolveArgument
{
    using Resolve_Argument;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Text;

    /// <summary>
    /// The Resolve-Argument cmdlet provides tab-completion for command arguments in PowerShell.
    /// </summary>
    [Cmdlet(
        VerbsDiagnostic.Resolve,
        "Argument",
        DefaultParameterSetName = "Resolve")]
    [OutputType(typeof(string))]
    [OutputType(typeof(CompletionResult))]
    public class ResolverCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the switch indicating List, ListCommand or L flag
        /// supported commands.
        /// </summary>
        [Parameter(
            ParameterSetName = "ListCommands",
            HelpMessage = "List commands supported with tab-expansion of arguments")]
        [Alias("List", "l")]
        public SwitchParameter ListCommands { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [Parameter(
            ParameterSetName = "Initialise",
            HelpMessage = "Initialise tab-expansion of arguments")]
        [Alias("Init", "i")]
        public SwitchParameter Initialise { get; set; }

        /// <summary>
        /// Gets or sets the file to which messages will be written.
        /// </summary>
        [Parameter(
            ParameterSetName = "Initialise",
            HelpMessage = "Enable loging to log file.")]
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
            HelpMessage = "Print PowerShell script used to initialise tab-expansion of arguments")]
        [Alias("Print", "p")]
        public SwitchParameter PrintScript { get; set; }

        /// <summary>
        /// Gets or sets the partial word provided before the user pressed Tab.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = "Resolve",
            HelpMessage = "Value provided by the user before they pressed tab")]
        public string? WordToComplete { get; set; }

        /// <summary>
        /// Gets or sets the Abstract Syntax Tree (AST) for the current input line.
        /// </summary>
        [Parameter(
            Position = 1,
            ParameterSetName = "Resolve",
            HelpMessage = "Abstract Syntax Tree for current input line")]
        public CommandAst? CommandAst { get; set; }

        /// <summary>
        /// Gets or sets the position of the cursor when tab was pressed.
        /// </summary>
        [Parameter(
            Position = 2,
            ParameterSetName = "Resolve",
            HelpMessage = "Command enered by user at the prompt")]
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
                case "ListCommands":
                    result.Append(Resolve_Argument.UIStrings.LIST_OF_COMMANDS);
                    WriteObject(result.ToString());
                    break;

                case "Initialise":
                    // Initialise logging to file if requested.
                    try
                    {
                        LOGGER.Initialise(LogFile, LogLevel);
                    }
                    // For tab-completions exceptions are silently suppressed and errors
                    // reported to a log file to facilitate debugging. However, prior to
                    // successful creation of the log file exceptions are captured and
                    // rethrown as a LoggerException which is then output on the cmdlet
                    // error stream.
                    catch (LoggerException ex)
                    {
                        WriteError(new ErrorRecord(
                        ex,
                        "Resolve-Argument-Logger-Error",
                        ErrorCategory.InvalidArgument,
                        LogFile));
                    }

                    // [ ][CONDAHELPER] Remove test code in cmdlet.
                    var list = Resolve_Argument.Helpers.CondaHelpers.GetEnvironments2();
                    var outString = string.Join(',', list);
                    LOGGER.Write($"Conda environments: {outString}");

                    // Return the initialisation script -> output should be piped to Invoke-Expression to activate module.
                    var init_script = Resolve_Argument.UIStrings.REGISTER_COMMAND_SCRIPT.Replace("$cmdNames", "conda");
                    result.Append(init_script);
                    WriteObject(result.ToString());

                    break;

                case "PrintScript":
                    var print_script = Resolve_Argument.UIStrings.REGISTER_COMMAND_SCRIPT.Replace("$cmdNames", "conda");
                    result.Append(print_script);
                    WriteObject(result.ToString());
                    break;

                case "Resolve":
                    // The algorithm uses the command abstract syntrax tree to tokenise the input text. 
                    // If it is not available then return no values.
                    if (CommandAst is not null)
                    {
                        // Convert the CommandAst to a list of tokens which will be used to evaluate
                        // which options are avaialble for the next parameter.
                        var commandTokens = new CommandAstVisitor();
                        CommandAst.Visit(commandTokens);
#if DEBUG
                        LOGGER.Write("Resolving word: " + WordToComplete??"");
                        LOGGER.Write("Resolving AST: " + CommandAst);
                        LOGGER.Write($"Base Command: {commandTokens.BaseCommand?.text ?? "Caught null"}");
                        LOGGER.Write($"Last Command: {commandTokens.LastToken?.text ?? "Caught null"}");
                        LOGGER.Write($"Prior Command: {commandTokens.PriorToken?.text ?? "Does not exist."}");
#endif
                        // Get suggested tab-completions. Not input parameters use null coalescing operator to gate nulls.
                        List<Suggestion> suggestions = new();
                        try
                        {
                            suggestions = Resolver.Suggestions(
                                wordToComplete: WordToComplete ?? "",
                                commandTokens: commandTokens,
                                cursorPosition: CursorPosition ?? CommandAst.ToString().Length
                            );
                        }
                        // A syntax tree exception is raised when the syntax tree resources cannot be loaded.
                        catch (Exception ex)
                        {
                            switch (ex)
                            {
                                case SyntaxTreeException:
                                    WriteError(new ErrorRecord(
                                        ex,
                                        "Error-loading-syntax-tree",
                                        ErrorCategory.ObjectNotFound,
                                        commandTokens));
                                    break;
                                default:
                                    WriteError(new ErrorRecord(
                                        ex,
                                        "Error-processing-record",
                                        ErrorCategory.InvalidOperation,
                                        commandTokens));
                                    break;
                            }
                        }

                        // Repackage results for Tab-Completions.
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
                    // TODO: [USERINTERFACE] All cases where we don't have a parameter set.
                    result.Append("Default we should print help text.");
                    WriteObject(result.ToString());
                    break;
            }
        }
    }
}