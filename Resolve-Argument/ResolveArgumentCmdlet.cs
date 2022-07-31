namespace Resolve_Argument
{
    using System.Management.Automation;
    using System.Text;

    /// <summary>
    /// The Resolve-Argument cmdlet provides auto-complete for arguments in PowerShell.
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Resolve, "Argument")]
    [OutputType(typeof(string))]
    public class ResolveArgumentCmdlet : Cmdlet
    {
        /// <summary>
        /// Gets or sets the flag indicating List, ListCommand or L flag
        /// supported commands.This flag is mutually exclusive of all other flags.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ParameterSetName = "ListCommand",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("List", "ListCommand", "l")]
        public string? ListCommand { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ParameterSetName = "Init",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Init", "i")]
        public string? InvokeInitScript { get; set; }

        /// <summary>
        /// Gets or sets to be completed
        /// </summary>
        [Parameter(
            Mandatory = false,
            ParameterSetName = "PrintScipt",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Print", "p")]
        public string? PrintScript { get; set; }

        [Parameter(
            Position = 0,
            Mandatory = false,
            ParameterSetName = "Resolve",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true
            )]
        [Alias("Word")]
        public string? Phrase { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ParameterSetName = "Resolve",
            ValueFromPipelineByPropertyName = true
            )]
        [Alias("Repeat")]
        public int? NumberOfTimesToRepeatPhrase { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var result = new StringBuilder();
            for (int i = 0; i < NumberOfTimesToRepeatPhrase; i++)
            {
                result.Append(Phrase);
            }

            WriteObject(result.ToString()); // This is what actually "returns" output.
        }
    }
}