
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using System.Management.Automation;

    /// <summary>
    /// The state machine evaluates the command line input and appends
    /// semantic information to each token.
    /// 
    /// The semantic information is sourced from the syntax tree and used
    /// to determine the token type.
    /// </summary>
    internal partial class StateMachine
    {
        /// <summary>
        /// Add suggestions for token completion.
        /// </summary>
        /// <param name="token">Token to which next suggestions will be added.</param>
        internal List<SemanticToken> EvaluateSpace(SemanticToken token)
        {
            List<SyntaxItemType> syntaxItemTypeFilter = new() {
                SyntaxItemType.PARAMETER,
                SyntaxItemType.COMMAND,
                SyntaxItemType.POSITIONAL,
            };
            return SuggestNextToken(token, syntaxItemTypeFilter);
        }
    }
}