
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
        public string Value;
        public Type Type;
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
        internal Token? FirstCommand
        {
            get
            {
                if (tokens.Count > 0)
                {
                    return this.tokens[0];
                }
                else
                {
                    return null;
                 }
            }
        }

        /// <summary>
        /// Returns the last token in the command list.
        /// </summary>
        internal Token? LastCommand
        {
            get
            {
                if (tokens.Count > 1)
                {
                    return this.tokens [this.tokens.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the second to last token in the command list.
        /// </summary>
        internal Token? PriorCommand
        {
            get
            {
                if (tokens.Count > 2)
                {
                    return this.tokens[this.tokens.Count - 2];
                }
                else
                {
                    return null;
                }
            }
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
                Value = ast.ToString(),
                Type = typeof(string)
            };
            this.tokens.Add(token);
            LOGGER.Write($"Default: {token.Value}, {token.Type}");

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
                Type = commandExpressionAst.GetType(),
            };
            this.tokens.Add(token);
            LOGGER.Write($"Command expression: {token.Value}, {token.Type}");

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
                Type = commandParameterAst.GetType(),
            };
            this.tokens.Add(token);
            LOGGER.Write($"Command parameter: {token.Value}, {token.Type}");

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
                Value = stringConstantExpressionAst.ToString(),
                Type = stringConstantExpressionAst.StaticType
            };

            // Double dashed parameters are parsed by PowerShell as String Constant Expressions.
            // Reclassify them as CommandParameters.
            if ((token.Value.Length > 2) & (token.Value[..2] == "--"))
            {
                token.Type = typeof(CommandParameterAst);
            }

            this.tokens.Add(token);
            LOGGER.Write($"String constant expression: {token.Value}, {token.Type}");

            return AstVisitAction.Continue;
        }
    }
}