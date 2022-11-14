
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
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
        /// <summary>
        /// Redirection token may include the file name.
        /// The Ast will provide this in the next token, so
        /// we can remove the file name from this token.
        /// </summary>
        /// <param name="token">Token to enhance.</param>
        /// <returns>Enhanced token.</returns>
        internal List<SemanticToken> EvaluateRedirection(SemanticToken token)
        {
            // Remove any text after the symbols. Note the Ast extracts the path and provides
            // it as a value in the next token.
            string redirectSymbol = "";
            if (token.AstType == typeof(FileRedirectionAst)) redirectSymbol = ">";
            if (token.AstType == typeof(MergingRedirectionAst)) redirectSymbol = ">>";

            SemanticToken redirectionToken = new()
            {
                Value = redirectSymbol,
                AstType = token.AstType,
                LowerExtent = token.LowerExtent,
                UpperExtent = token.LowerExtent + redirectSymbol.Length - 1,
                SemanticType = SemanticToken.TokenType.Redirection,
                IsComplete = true,
            };

            // NOTE [LOW][STATEMACHINE] Redirection assumed to path only, not between streams (&3 > &1)
            // See: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_redirection
            this.ms.ParameterValues = 1;
            this.ms.ParameterSyntaxItem = new SyntaxItem() { ItemType = SyntaxItemType.REDIRECTION, Name="PATH" };
            this.ms.CurrentState = MachineState.State.Value;
            return new List<SemanticToken> { redirectionToken };
        }
    }
}
