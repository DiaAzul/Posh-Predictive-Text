// <copyright file="ResolveArgumentCmdlet.cs" company="Tanzo Creative Ltd">
// Copyright (c) Tanzo Creative Ltd. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ResolveArgument
{
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
    public class ResolveArgumentCmdlet : PSCmdlet
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
        /// Process record when all options are defined.
        /// - Validate input
        /// - Process record
        /// - Output object.
        /// TODO Complete documentation for ProcessRecord.
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
                    result.Append(Resolve_Argument.UIStrings.POSH_INIT_SCRIPT);
                    WriteObject(result.ToString());
                    break;
                case "PrintScript":
                    var localised_script = Resolve_Argument.UIStrings.REGISTER_COMMAND_SCRIPT.Replace("$cmdNames", "conda");
                    result.Append(localised_script);
                    WriteObject(result.ToString());
                    break;
                case "Resolve":
                    // CompletionResultType: https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.completionresulttype?view=powershellsdk-7.0.0

                    // The algorithm requires the command abstract syntrax tree from which tokens are extracted.
                    if (CommandAst != null)
                    {
                        var commandTokens = new CommandAstVisitor();
                        CommandAst.Visit(commandTokens);

                        string checkedWordToComplete = WordToComplete == null ? "" : WordToComplete;
                        int checkedCursorPosition = CursorPosition == null ? CommandAst.ToString().Length : (int)CursorPosition;
 
                        LOGGER.Write("Resolving word: " + WordToComplete);
                        LOGGER.Write("Resolving AST: " + CommandAst);
                        LOGGER.Write($"First Command: {commandTokens.FirstCommand?.Value}");
                        LOGGER.Write($"Last Command: {commandTokens.LastCommand?.Value}");
                        LOGGER.Write($"Prior Command: {commandTokens.PriorCommand?.Value}");

                        var suggestions = ArgumentResolver.Suggestions(checkedWordToComplete, commandTokens, checkedCursorPosition);

                        WriteObject(suggestions);
                    }

                    break;
                default:
                    result.Append("Default we should print help text.");
                    WriteObject(result.ToString());
                    break;
            }
        }
    }
}