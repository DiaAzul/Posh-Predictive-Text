// <copyright file="ResolveArgumentCmdlet.cs" company="Tanzo Creative Ltd">
// Copyright (c) Tanzo Creative Ltd. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace Resolve_Argument
{
    using System.Management.Automation;
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
            ValueFromPipelineByPropertyName = true)]
        [Alias("List", "l")]
        public SwitchParameter ListCommand { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [Parameter(
            ParameterSetName = "Initialise",
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Init", "i")]
        public SwitchParameter Initialise { get; set; }

        /// <summary>
        /// Gets or sets to be completed.
        /// TODO Complete documentation for PrintScipt parameter.
        /// </summary>
        [Parameter(
            ParameterSetName = "PrintScript",
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Print", "p")]
        public SwitchParameter PrintScript { get; set; }

        /// <summary>
        /// Gets or sets to be completed.
        /// TODO Implement Resolve-Argument processing parameters.
        /// </summary>
        [Parameter(
            Position = 0,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Word")]
        public string? Phrase { get; set; }

        /// <summary>
        /// Gets or sets to be completed.
        /// </summary>
        [Parameter(
            Position = 1,
            ParameterSetName = "Resolve",
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Repeat")]
        public int? NumberOfTimesToRepeatPhrase { get; set; }

        /// <summary>
        /// Process record when all options are defined.
        /// - Validate input
        /// - Process record
        /// - Output object.
        /// TODO Complete documentation for ProcessRecord.
        /// </summary>
        protected override void ProcessRecord()
        {
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
                    CompletionResult response = new CompletionResult(
                        "Command",
                        "ListItem",
                        CompletionResultType.ParameterValue,
                        "ToolTip");
                    // result.Append(response);
                    this.WriteObject(response);
                    break;
                case "Resolve":
                    for (int i = 0; i < this.NumberOfTimesToRepeatPhrase; i++)
                    {
                        result.Append(this.Phrase);
                    }
                    this.WriteObject(result.ToString());
                    break;
                default:
                    result.Append("Yo. I'm the default.");
                    this.WriteObject(result.ToString());
                    break;
            }
        }
    }
}