/* Resolve-Arguments
 * (VerbsDiagnostic.Resolve)
 * 
 * Cmdlet to resolves arguments for a third-party command.
 * 
 * --List, --ListCommands, -l : Lists Commands supported by the cmdlet.
 * 
 * --Init -i : PowerShell script to register as an Argument-Resolver.
 * 
 * --Print -p : Print the PowerShell script that initialises the command.
 * 
 * Processing needs the following arguments:
 * --CommandName $commandName (Position 0) - This parameter is set to the name of the
 * command for which the script block is providing tab completion.
 * --ParameterName $parameterName (Position 1) - This parameter is set to the parameter
 *  whose value requires tab completion.
 * --WordToComplete $wordToComplete (Position 2) - This parameter is set to value the
 * user has provided before they pressed Tab. Your script block should use this value to
 * determine tab completion values.
 * --CommandAst $commandAst (Position 3) - This parameter is set to the Abstract Syntax
 * Tree (AST) for the current input line.
 * --FakeBoundParameters $fakeBoundParameters (Position 4) - This parameter is set to a
 * hashtable containing the $PSBoundParameters for the cmdlet, before the user pressed
 * Tab.
*/

using System.Management.Automation;
using System.Text;


namespace PowerShellResolveArgument
{
    [Cmdlet(VerbsDiagnostic.Resolve, "Argument")]
    [OutputType(typeof(string))]
    public class ResolveArgumentCmdlet : Cmdlet
    {
        /// <summary>
        /// The List, ListCommand or L flag request the cmdlet to return a list of
        /// supported commands.This flag is mutually exclusive of all other flags.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ParameterSetName = "ListCommand",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("List", "ListCommand", "L")]
        public string? ListCommand { get; set; }

        /// <summary>
        /// The Init command generates and invokes a PowerShell script to add the 
        /// cmdlet as an Argument Resolver in PowerShell.
        /// </summary>
        [Parameter(
            Mandatory = false,
            ParameterSetName = "InitCommand",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [Alias("Init", "I")]
        public string? InvokeInitScript { get; set; }


        [Parameter(
            Position = 0,
            Mandatory = false,
            ParameterSetName = "CommandToResolve",
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true
            )]
        [Alias("Word")]
        public string? Phrase { get; set; }

        [Parameter(
            Position = 1,
            Mandatory = false,
            ParameterSetName = "CommandToResolve",
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