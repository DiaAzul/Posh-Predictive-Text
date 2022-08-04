// <copyright file="ResolveArgumentCmdlet.cs" company="Tanzo Creative Ltd">
// Copyright (c) Tanzo Creative Ltd. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace Resolve_Argument
{
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Text;

    /// <summary>
    /// The Resolve-Argument cmdlet provides auto-complete of command arguments in PowerShell.
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
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "List commands supported with tab-expansion of arguments")]
        [Alias("List", "l")]
        public SwitchParameter ListCommands { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [Parameter(
            ParameterSetName = "Initialise",
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Initialise tab-expansion of arguments")]
        [Alias("Init", "i")]
        public SwitchParameter Initialise { get; set; }

        /// <summary>
        /// Gets or sets a flag to print the PowerShell script used to initialise tab-exapansion
        /// of arguments.
        /// </summary>
        [Parameter(
            ParameterSetName = "PrintScript",
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Print PowerShell script used to initialise tab-expansion of arguments")]
        [Alias("Print", "p")]
        public SwitchParameter PrintScript { get; set; }

        /// <summary>
        /// Gets or sets the name of the command for which the script block is
        /// providing tab completion.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Command enered by user at the prompt")]
        public string? CommandName { get; set; }

        /// <summary>
        /// Gets or sets the parameter whose value requires tab completion.
        /// </summary>
        [Parameter(
            Position = 1,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Parameter that requires tab-completion")]
        public string? ParameterName { get; set; }

        /// <summary>
        /// Gets or sets the value the user has provided before they pressed Tab.
        /// </summary>
        [Parameter(
            Position = 2,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Value provided by the user before they pressed tab")]
        public string? WordToComplete { get; set; }

        /// <summary>
        /// Gets or sets the Abstract Syntax Tree (AST) for the current input line.
        /// </summary>
        [Parameter(
            Position = 3,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Abstract Syntax Tree for current input line")]
        public CommandAst? CommandAST { get; set; }

        /// <summary>
        /// Gets or sets the hashtable containing the $PSBoundParameters for the cmdlet,
        /// before the user pressed Tab.
        /// </summary>
        [Parameter(
            Position = 4,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Hashtable of parameters already entered on the command line")]
        public string[]? FakeBoundParameters { get; set; }

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

            switch (this.ParameterSetName)
            {
                case "ListCommands":
                    result.Append("Listicles.");
                    this.WriteObject(result.ToString());
                    break;
                case "Initialise":
                    result.Append("Initialise.");
                    this.WriteObject(result.ToString());
                    break;
                case "PrintScript":
                    result.Append("Print.");
                    this.WriteObject(result.ToString());
                    break;
                case "Resolve":
                    CompletionResult response = new(
                        "Command",
                        "ListItem",
                        CompletionResultType.ParameterValue,
                        "ToolTip");

                    this.WriteObject(response);
                    break;
                default:
                    result.Append("Yo. I'm the default.");
                    this.WriteObject(result.ToString());
                    break;
            }
        }
    }
}