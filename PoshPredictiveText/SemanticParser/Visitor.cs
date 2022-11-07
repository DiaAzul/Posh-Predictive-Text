
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
        /// The tokeniser holds the list of items on the command line
        /// with semantic data for each item.
        /// </summary>
        private readonly Tokeniser tokeniser;

        /// <summary>
        /// Class construtor initialising the token dictionary
        /// </summary>
        internal Visitor()
        {
            this.tokeniser = new();
        }

        /// <summary>
        /// Tokenised command line.
        /// </summary>
        internal Tokeniser Tokeniser
        {
            get { return this.tokeniser; }
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
            // LOGGER.Write($"Default (not tokenised): {ast.ToString()}, {ast.GetType()}");
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
            Token token = new()
            {
                Value = commandExpressionAst.ToString(),
                AstType = commandExpressionAst.GetType(),
                LowerExtent = commandExpressionAst.Extent.StartOffset,
                UpperExtent = commandExpressionAst.Extent.EndOffset,
                SemanticType = Token.TokenType.Command,
            };
            this.tokeniser.Add(token);
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
            Token token = new()
            {
                Value = commandParameterAst.ToString(),
                AstType = commandParameterAst.GetType(),
                LowerExtent = commandParameterAst.Extent.StartOffset,
                UpperExtent = commandParameterAst.Extent.EndOffset,
                SemanticType = Token.TokenType.Parameter,
            };
            this.tokeniser.Add(token);
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
            Token token = new()
            {
                Value = constantExpressionAst.ToString(),
                AstType = constantExpressionAst.GetType(),
                LowerExtent = constantExpressionAst.Extent.StartOffset,
                UpperExtent = constantExpressionAst.Extent.EndOffset,
                SemanticType = Token.TokenType.Constant,
            };
            this.tokeniser.Add(token);
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
            Token.TokenType semanticType = Token.TokenType.StringConstant;
            // Record strings starting with double-dash (--) as parameters.
            try
            {
                if (Value[..2] == "--")
                {
                    astType = typeof(CommandParameterAst);
                    semanticType = Token.TokenType.Parameter;
                }
            }
            catch (ArgumentOutOfRangeException) { }
            Token token = new()
            {
                Value = CommonTasks.Decapsulate(Value),
                AstType = astType,
                LowerExtent = stringConstantExpressionAst.Extent.StartOffset,
                UpperExtent = stringConstantExpressionAst.Extent.EndOffset,
                SemanticType = semanticType,
            };
            this.tokeniser.Add(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process token which redirect output to file.
        /// </summary>
        /// <param name="redirectionAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitFileRedirection(FileRedirectionAst redirectionAst)
        {
            Token token = new()
            {
                Value = redirectionAst.ToString(),
                AstType = redirectionAst.GetType(),
                LowerExtent = redirectionAst.Extent.StartOffset,
                UpperExtent = redirectionAst.Extent.EndOffset,
                SemanticType = Token.TokenType.Redirection,
            };
            this.tokeniser.Add(token);
            return AstVisitAction.Continue;
        }

        /// <summary>
        /// Process token which appends output to file.
        /// </summary>
        /// <param name="redirectionAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue to next node.</returns>
        public override AstVisitAction VisitMergingRedirection(MergingRedirectionAst redirectionAst)
        {
            Token token = new()
            {
                Value = redirectionAst.ToString(),
                AstType = redirectionAst.GetType(),
                LowerExtent = redirectionAst.Extent.StartOffset,
                UpperExtent = redirectionAst.Extent.EndOffset,
                SemanticType = Token.TokenType.Redirection,
            };
            this.tokeniser.Add(token);
            return AstVisitAction.Continue;
        }
    }
}