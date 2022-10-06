
namespace PoshPredictiveText
{
    /// <summary>
    /// Cached information for command line arguments which have already been parsed.
    /// </summary>
    internal class StateMachineState
    {
        /// <summary>
        /// State of the statemachine after the argument was parsed.
        /// </summary>
        internal StateMachine.State State { get; set; } = StateMachine.State.NoCommand;
        /// <summary>
        /// Command path after the argument was parsed.
        /// </summary>
        internal CommandPath CommandPath { get; set; } = default!;
        /// <summary>
        /// Number of parameter values to be entered after the argument if the parsed item
        /// was a parameter which expects values.
        /// </summary>
        internal int ParameterValues { get; set; } = default!;
        /// <summary>
        /// Parameter syntaxItem if the parsed item was a parameter expecting value arguments.
        /// </summary>
        internal SyntaxItem? ParameterSyntaxItem { get; set; }
        /// <summary>
        /// The list of tokens to be returned following parsing the argument.
        /// </summary>
        internal List<Token> ReturnTokens { get; set; } = new List<Token>();
    }
}
