// TODO [ ][POWERSHELL] Raise issue: The HelpMessageBaseName/ResourceId does not generate help text.

namespace PoshPredictiveText
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Management.Automation;

    /// <summary>
    /// The Posh Predictive Text cmdlet provides suggested completions for
    /// command arguments in PowerShell.
    /// </summary>
    [Cmdlet(
        VerbsLifecycle.Install,
        "PredictiveText")]
    [OutputType(typeof(string))]
    [OutputType(typeof(CompletionResult))]
    public class InstallPredictiveText : PSCmdlet
    {
        /// <inheritdoc/>
        protected override void EndProcessing()
        {
            var init_script = UIstring.Resource("REGISTER_COMMAND_SCRIPT")
                                .Replace("$cmdNames", SyntaxTreesConfig.SupportedCommands());
            try
            {
                var scriptBlock = InvokeCommand.NewScriptBlock(init_script);
                InvokeCommand.InvokeScript(false, scriptBlock, null, null);
            }
            catch (Exception ex) when (
            ex is ParseException
            || ex is RuntimeException
            || ex is FlowControlException)
            {
                WriteError(new ErrorRecord(
                ex,
                "Install-PoshPRedictiveText-InitialiseScript-Error",
                ErrorCategory.InvalidOperation,
                ""));
            }
            base.EndProcessing();
        }
    }
}