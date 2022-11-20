
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    /// <summary>
    /// The Semantic CLI analyses text entered on the command line
    /// and generates a sequence of parsed tokens expressing its meaning. 
    /// The meaning of the text is defined in the command's syntax tree.
    /// </summary>
    internal class SemanticCLI
    {
        /// <summary>
        /// The state machine retains the state of the command line as each
        /// token is added.
        /// </summary>
        private readonly StateMachine stateMachine = new();

        internal SyntaxTree? SyntaxTree
        {
            get { return stateMachine.SyntaxTree; }
        }

        /// <summary>
        /// Add token to the list.
        /// </summary>
        /// <param name="token">Token to add to the tokeniser</param>
        internal void AddToken(SemanticToken token)
        {
            LOGGER.Write($"SEMANTIC CLI: Parse '{token.Value}' of type {token.SemanticType}.");

            List<SemanticToken> semanticTokens = stateMachine.Evaluate(token);

            LOGGER.Write($"SEMANTIC CLI: Added {semanticTokens.Count} tokens, there are now {this.stateMachine.Count} tokens.");
        }

        /// <summary>
        /// Returns the first token in the command list.
        /// </summary>
        internal string? BaseCommand
        {
            get { return stateMachine.BaseCommand; }
        }

        /// <summary>
        /// Returns the last token in the command list.
        /// </summary>
        internal SemanticToken? LastToken
        {
            get { return stateMachine.LastToken; }
        }

        /// <summary>
        /// Returns the second to last token in the command list.
        /// </summary>
        internal SemanticToken? PriorToken
        {
            get { return stateMachine.PriorToken; }
        }

        /// <summary>
        /// Returns a list of all tokens.
        /// </summary>
        internal List<SemanticToken> All
        {
            get { return stateMachine.All; }
        }

        /// <summary>
        /// Returns the count of tokens on the command line.
        /// </summary>
        internal int Count
        {
            get { return stateMachine.Count; }
        }

        /// <summary>
        /// Return the token at the index position in the list.
        /// </summary>
        /// <param name="index">Index position of required token.</param>
        /// <returns>Token at the position in the list, null if index outside of scope of list.</returns>
        internal SemanticToken? Index(int index) => stateMachine.Index(index);
    }
}
