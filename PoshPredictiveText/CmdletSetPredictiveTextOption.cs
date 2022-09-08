
namespace PoshPredictiveText
{
    using System.Management.Automation;
    using static PoshPredictiveText.LOGGER;


    /// <summary>
    /// The Posh Predictive Text cmdlet provides suggested completions for
    /// command arguments in PowerShell.
    /// </summary>
    [Cmdlet(
        VerbsCommon.Set,
        "PredictiveTextOption")]
    [OutputType(typeof(string))]
    [OutputType(typeof(CompletionResult))]
    public class SetPredictiveTextOption: PSCmdlet
    {
        /// <summary>
        /// Gets or sets the file to which messages will be written.
        /// </summary>
        [Parameter(
            ParameterSetName = "Logging",
            HelpMessage = "Enable logging to log file.")]
        public string? LogFile { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating Init command generates and invokes
        /// a PowerShell script to add the cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [ValidateSet("INFO", "WARN", "ERROR", IgnoreCase = true)]
        [Parameter(
            ParameterSetName = "Logging",
            HelpMessage = "Level of information to log (INFO, WARN, ERROR). Default: ERROR.")]
        public string? LogLevel { get; set; }

        /// <summary>
        /// Set options
        /// </summary>
        protected override void EndProcessing()
        {
            switch (ParameterSetName)
            {
                case "Logging":
                    // For predictive text exceptions are silently suppressed and errors
                    // reported to a log file to facilitate debugging. However, prior to
                    // successful creation of the log file exceptions are captured and
                    // re-thrown as a LoggerException which is then output on the cmdlet
                    // error stream.
                    try
                    {
                        Initialise(LogFile, LogLevel);
                    }

                    catch (LoggerException ex)
                    {
                        WriteError(new ErrorRecord(
                        ex,
                        "Install-PoshPRedictiveText-Logger-Error",
                        ErrorCategory.InvalidArgument,
                        LogFile));
                    }
                    break;

                default:
                    break;
            }

            base.ProcessRecord();
        }
    }
}