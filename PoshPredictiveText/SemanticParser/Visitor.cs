
namespace PoshPredictiveText.SemanticParser
{
    using System.Management.Automation.Language;

    /// <summary>
    /// Implements a visitor for the PowerShell abstract syntax tree.
    /// Reads the input no the command line and creates an ordered list of
    /// tokens representing user input.
    /// </summary>
    internal class Visitor : AstVisitor
    {
        /// <summary>
        /// The semanticCLI holds the list of items on the command line
        /// with semantic data for each item.
        /// </summary>
        private readonly SemanticCLI semanticCLI;

        /// <summary>
        /// Class construtor initialising the token dictionary
        /// </summary>
        internal Visitor()
        {
            this.semanticCLI = new();
        }

        /// <summary>
        /// Tokenised command line.
        /// </summary>
        internal SemanticCLI SemanticCLI
        {
            get { return this.semanticCLI; }
        }

        /// <summary>
        /// Adds a blank token when there is a space after the last
        /// token in the CLI which does not get parsed by the visitor.
        /// 
        /// This is used in the state machine to trigger suggestions for
        /// the next token.
        /// </summary>
        public void BlankVisit(string tokenValue, int lowerExtent, int upperExtent)
        {
            SemanticToken token = new()
            {
                Value = tokenValue,
                AstType = typeof(StringConstantExpressionAst),
                LowerExtent = lowerExtent,
                UpperExtent = upperExtent,
                SemanticType = SemanticToken.TokenType.Space,
            };
            this.semanticCLI.AddToken(token);
        }

        /// <summary>
        /// Default action if the token has not been processed by a
        /// more specific token handler. Adds the token text with a
        /// generic text type.
        /// </summary>
        /// <param name="ast">Node in the abstract syntax tree.</param>
        /// <returns>Flag to continue processing the abstract syntax tree.</returns>
        public override AstVisitAction DefaultVisit(Ast ast)
        {
            // Keep the following, even though not used often. Useful when the calling
            // ast structure changes and we need to find out what types we are NOT
            // explicitly capturing.
#if DEBUG
            LOGGER.Write($"VISITOR: Caught in default, no handler for: {ast}, {ast.GetType()}");
#endif
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process commandAst node in the abstract syntax tree. Does not 
        /// process this node and skips to next node in the tree.
        /// </summary>
        /// <param name="commandAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            // Do not process top level node, skip to children.
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process CommandExpressionAst node in the abstract syntax tree.
        /// Add a token to the list of tokens on the command line.
        /// </summary>
        /// <param name="commandExpressionAst"></param>
        /// <returns>Continue to next node</returns>
        public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
        {
            SemanticToken token = new()
            {
                Value = commandExpressionAst.ToString(),
                AstType = commandExpressionAst.GetType(),
                LowerExtent = commandExpressionAst.Extent.StartOffset,
                UpperExtent = commandExpressionAst.Extent.EndOffset,
                SemanticType = SemanticToken.TokenType.Command,
            };
            this.semanticCLI.AddToken(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process CommandParameterAst node in the abstract syntax tree.
        /// Add token to the list of tokens on the command line.
        /// </summary>
        /// <param name="commandParameterAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
        {
            SemanticToken token = new()
            {
                Value = commandParameterAst.ToString(),
                AstType = commandParameterAst.GetType(),
                LowerExtent = commandParameterAst.Extent.StartOffset,
                UpperExtent = commandParameterAst.Extent.EndOffset,
                SemanticType = SemanticToken.TokenType.Parameter,
            };
            this.semanticCLI.AddToken(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process ConstantExpressionAst node in the abstract syntax tree.
        /// Add token to the list of tokens on the command line.
        /// </summary>
        /// <param name="constantExpressionAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitConstantExpression(ConstantExpressionAst constantExpressionAst)
        {
            SemanticToken token = new()
            {
                Value = constantExpressionAst.ToString(),
                AstType = constantExpressionAst.GetType(),
                LowerExtent = constantExpressionAst.Extent.StartOffset,
                UpperExtent = constantExpressionAst.Extent.EndOffset,
                SemanticType = SemanticToken.TokenType.Constant,
            };
            this.semanticCLI.AddToken(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process stringConstantExpressionAst node in the abstract syntax tree.
        /// 
        /// Identifies Gnu/Posix formatted parameters which start with a double dash and
        /// reclassifies them as CommandParameterAst type.
        /// 
        /// Adds token to the list of tokens on the command line.
        /// </summary>
        /// <param name="stringConstantExpressionAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)
        {
            string Value = stringConstantExpressionAst.ToString();
            Type astType = stringConstantExpressionAst.StaticType;
            SemanticToken.TokenType semanticType = SemanticToken.TokenType.StringConstant;
            // Record strings starting with double-dash (--) as parameters.
            try
            {
                if (Value[..2] == "--")
                {
                    astType = typeof(CommandParameterAst);
                    semanticType = SemanticToken.TokenType.Parameter;
                }
            }
            catch (ArgumentOutOfRangeException) { }
            SemanticToken token = new()
            {
                Value = CommonTasks.Decapsulate(Value),
                AstType = astType,
                LowerExtent = stringConstantExpressionAst.Extent.StartOffset,
                UpperExtent = stringConstantExpressionAst.Extent.EndOffset,
                SemanticType = semanticType,
            };
            this.semanticCLI.AddToken(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process token which redirect output to file.
        /// </summary>
        /// <param name="redirectionAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst)
        {
            SemanticToken token = new()
            {
                Value = redirectionAst.ToString(),
                AstType = redirectionAst.GetType(),
                LowerExtent = redirectionAst.Extent.StartOffset,
                UpperExtent = redirectionAst.Extent.EndOffset,
                SemanticType = SemanticToken.TokenType.Redirection,
            };
            this.semanticCLI.AddToken(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process token which appends output to file.
        /// </summary>
        /// <param name="redirectionAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst redirectionAst)
        {
            SemanticToken token = new()
            {
                Value = redirectionAst.ToString(),
                AstType = redirectionAst.GetType(),
                LowerExtent = redirectionAst.Extent.StartOffset,
                UpperExtent = redirectionAst.Extent.EndOffset,
                SemanticType = SemanticToken.TokenType.Redirection,
            };
            this.semanticCLI.AddToken(token);
            return AstVisitAction.Continue;
        }
    }
}