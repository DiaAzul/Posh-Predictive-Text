
namespace PoshPredictiveText.SemanticParser
{
    internal partial class StateMachine
    {
        /// <summary>
        /// Evaluate an item to determine its semantic type.
        /// 
        /// The PowerShell abstract syntax tree identifies items
        /// matching the Windows understanding of command line
        /// arguments. It identifies redirection and parameters,
        /// but everything else is identified as a string constant.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        internal List<SemanticToken> EvaluateItem(SemanticToken token)
        {
            return token.SemanticType switch
            {
                SemanticToken.TokenType.Parameter => EvaluateParameter(token),
                SemanticToken.TokenType.Redirection => EvaluateRedirection(token),
                SemanticToken.TokenType.StringConstant => EvaluateStringConstant(token),
                SemanticToken.TokenType.Space => EvaluateSpace(token),
                _ => new List<SemanticToken> { token },
            }; ;
        }
    }
}
