
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Management.Automation.Language;

    /// <summary>
    /// The state machine evaluates the command line input and appends
    /// semantic information to each token.
    /// 
    /// The semantic information is sourced from the syntax tree and used
    /// to determine the token type.
    /// </summary>
    internal partial class StateMachine
    {
        internal List<SemanticToken> EvaluatePosixOption(SemanticToken token)
        {
            // TODO [HIGH][STATEMACHINE] Implement evaluate posix options.
            return new List<SemanticToken> { token };
        }
    }
}
