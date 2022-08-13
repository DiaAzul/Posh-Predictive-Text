
namespace ResolveArgument
{
    using System.Management.Automation.Language;

    /// <summary>
    /// Token representing each distinct item entered by the 
    /// user after the command prompt. The value of token is
    /// the text entered by the user. Type is the abstract
    /// syntax tree node type and used to identify parameters
    /// from other inputs.
    /// </summary>
    internal struct Token
    {
        internal string text;
        internal Type type;
    }

    /// <summary>
    /// Implements a visitor for the PowerShell abstract syntax tree.
    /// Reads the input no the command line and creates an ordered list of
    /// tokens representing user input.
    /// </summary>
    internal class CommandAstVisitor : AstVisitor
    {
        private readonly List<Token> tokens;

        internal CommandAstVisitor()
        {
            this.tokens = new List<Token>();
        }

        /// <summary>
        /// Returns the first token in the command list.
        /// </summary>
        internal Token? BaseCommand
        {
            get { return this.Index(0);}
        }

        /// <summary>
        /// Returns the last token in the command list.
        /// </summary>
        internal Token? LastToken
        {
            get { return this.Index(this.tokens.Count - 1); }
        }

        /// <summary>
        /// Returns the second to last token in the command list.
        /// </summary>
        internal Token? PriorToken
        {
            get { return this.Index(this.tokens.Count - 2); }
        }

        /// <summary>
        /// Return the token at the index position in the list.
        /// </summary>
        /// <param name="index">Index position of required token.</param>
        /// <returns>Token at the position in the list, null if index outside of scope of list.</returns>
        internal Token? Index(int index)
        {
            Token? token;
            try
            {
                token = this.tokens[index];
            }
            catch (IndexOutOfRangeException)
            {
                token = null;
            }
            return token;
        }

        /// <summary>
        /// Returns a list of all tokens.
        /// </summary>
        internal List<Token> All
        {
            get { return this.tokens; }
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
            Token token = new()
            {
                text = ast.ToString(),
                type = typeof(string)
            };
            this.tokens.Add(token);
#if DEBUG
            LOGGER.Write($"Default: {token.text}, {token.type}");
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
                text = commandExpressionAst.ToString(),
                type = commandExpressionAst.GetType(),
            };
            this.tokens.Add(token);
#if DEBUG
            LOGGER.Write($"Command expression: {token.text}, {token.type}");
#endif
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
                text = commandParameterAst.ToString(),
                type = commandParameterAst.GetType(),
            };
            this.tokens.Add(token);
#if DEBUG
            LOGGER.Write($"Command parameter: {token.text}, {token.type}");
#endif
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
            Token token = new()
            {
                text = stringConstantExpressionAst.ToString(),
                type = stringConstantExpressionAst.StaticType
            };

            // Double dashed parameters are parsed by PowerShell as String Constant Expressions.
            // Reclassify them as CommandParameters.
            try
            {
                if (token.text[..2] == "--")
                {
                    token.type = typeof(CommandParameterAst);
                }
            }
            catch (ArgumentOutOfRangeException) { }

            this.tokens.Add(token);
#if DEBUG
            LOGGER.Write($"String constant expression: {token.text}, {token.type}");
#endif
            return AstVisitAction.Continue;
        }
    }
}