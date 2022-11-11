

namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;

    /// <summary>
    /// Macine state information for command line arguments which have already been parsed.
    /// </summary>
    internal class MachineState
    {
        /// <summary>
        /// State of the statemachine after the argument was parsed.
        /// </summary>
        internal StateMachine.State State { get; set; } = StateMachine.State.NoCommand;

        /// <summary>
        /// Command path after the argument was parsed.
        /// </summary>
        internal CommandPath CommandPath { get; set; } = new();

        /// <summary>
        /// Number of parameter values to be entered after the argument if the parsed item
        /// was a parameter which expects values.
        /// </summary>
        internal int ParameterValues { get; set; } = 0;

        /// <summary>
        /// Parameter syntaxItem if the parsed item was a parameter expecting value arguments.
        /// </summary>
        internal SyntaxItem? ParameterSyntaxItem { get; set; } = null;

        /// <summary>
        /// Parameter sets the are in force for this command path.
        /// </summary>
        internal List<string>? ParameterSets { get; set; } = null!;

        /// <summary>
        /// The list of tokens to be returned following parsing the argument.
        /// </summary>
        internal List<SemanticToken>? ReturnTokens { get; set; } = null;
    }
}
