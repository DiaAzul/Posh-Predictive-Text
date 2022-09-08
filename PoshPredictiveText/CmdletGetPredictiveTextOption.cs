
namespace PoshPredictiveText
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Management.Automation;
    using System.Text;


    /// <summary>
    /// The Posh Predictive Text cmdlet provides suggested completions for
    /// command arguments in PowerShell.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Get,
        "PredictiveTextOption")]
    [OutputType(typeof(string))]
    [OutputType(typeof(CompletionResult))]
    public class GetPredictiveTextOption: PSCmdlet
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
        /// Gets or sets a flag to print the PowerShell script used to initialise tab-exapansion
        /// of arguments.
        /// </summary>
        [Parameter(
            ParameterSetName = "PrintScript",
            HelpMessage = "Print PowerShell script used to initialise Powershell Predictive Text.")]
        [Alias("Print", "p")]
        public SwitchParameter PrintScript { get; set; }

        /// <summary>
        /// Responds with options
        /// </summary>
        protected override void EndProcessing()
        {
            var result = new StringBuilder();

            switch (ParameterSetName)
            {
                case "Version":
                    result.AppendLine(UIstring.Resource("VERSION"));
                    WriteObject(result.ToString());
                    break;

                case "ListCommands":
                    result.AppendLine(UIstring.Resource("LIST_COMMANDS"));
                    string supportedCommands = SyntaxTreesConfig.SupportedCommands();
                    result.AppendLine(supportedCommands);

                    WriteObject(result.ToString());
                    break;

                case "PrintScript":
                    var print_script = UIstring.Resource("REGISTER_COMMAND_SCRIPT")
                                         .Replace("$cmdNames", SyntaxTreesConfig.SupportedCommands());
                    result.Append(print_script);
                    WriteObject(result.ToString());
                    break;

                default:
                    break;
            }
            base.EndProcessing();
        }
    }
}