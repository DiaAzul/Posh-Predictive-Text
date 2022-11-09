

namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using System.Management.Automation;

    /// <summary>
    /// Cached information for command line arguments which have already been parsed.
    /// </summary>
    internal record StateMachineState
    {
        /// <summary>
        /// State of the statemachine after the argument was parsed.
        /// </summary>
        internal StateMachine.State State { get; init; } = StateMachine.State.NoCommand;

        /// <summary>
        /// Command path after the argument was parsed.
        /// </summary>
        internal CommandPath CommandPath { get; init; } = default!;

        /// <summary>
        /// Number of parameter values to be entered after the argument if the parsed item
        /// was a parameter which expects values.
        /// </summary>
        internal int ParameterValues { get; init; } = default!;

        /// <summary>
        /// Parameter syntaxItem if the parsed item was a parameter expecting value arguments.
        /// </summary>
        internal SyntaxItem? ParameterSyntaxItem { get; init; }

        /// <summary>
        /// The list of tokens to be returned following parsing the argument.
        /// </summary>
        internal List<Token> ReturnTokens { get; init; } = new List<Token>();

        /// <summary>
        /// Parameter sets the are in force for this command path.
        /// </summary>
        internal List<string>? ParameterSets { get; init; } = null!;
    }
}
