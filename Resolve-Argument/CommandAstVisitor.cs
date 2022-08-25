
namespace ResolveArgument
{
    using System.Management.Automation.Language;
    using System.Text;

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

        internal bool IsCommandParameter
        {
            get { return type == typeof(CommandParameterAst); }
        }
    }

    /// <summary>
    /// Implements a visitor for the PowerShell abstract syntax tree.
    /// Reads the input no the command line and creates an ordered list of
    /// tokens representing user input.
    /// </summary>
    internal class CommandAstVisitor : AstVisitor
    {
        private readonly Dictionary<int, Token> tokens;
        // Incremented each time a new token is created.
        // Indicates position of token on the command line.
        private int commandLinePosition = 0;

        internal CommandAstVisitor()
        {
            this.tokens = new Dictionary<int, Token>();
        }

        // Returns the position of the token and updates the position count.
        private int TokenPosition
            { get { return commandLinePosition++; } }


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
        /// Returns a list of all tokens.
        /// </summary>
        internal Dictionary<int, Token> All
        {
            get { return this.tokens; }
        }

        /// <summary>
        /// Returns the count of tokens on the command line.
        /// </summary>
        internal int Count
        {
            get { return this.tokens.Count;  }
        }

        /// <summary>
        /// Returns a dictionary containing command parameters and their
        /// location on the command line. If there are no command parameters
        /// returns an empty dictionary.
        /// </summary>
        internal Dictionary<int, Token> CommandParameters
        {
            get
            {
                return this.tokens?.Where(item => item.Value.IsCommandParameter)
                            .ToDictionary(item => item.Key, item => item.Value)
                            ?? new Dictionary<int, Token>();
            }
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
            catch (Exception ex) when (
            ex is ArgumentOutOfRangeException
            || ex is IndexOutOfRangeException
            || ex is KeyNotFoundException)
            {
                token = null;
            }
            return token;
        }

        internal (string, int) CommandPath(List<string> uniqueCommands)
        {

            StringBuilder commandPath = new(capacity: 64);
            int tokensInCommand = 0;
            foreach (var (position, commandToken) in this.tokens)
            {
                if (uniqueCommands.Contains(commandToken.text))
                {
                    if (commandPath.Length > 0)
                    {
                        commandPath.Append('.');
                    }
                    commandPath.Append(commandToken.text);
                    tokensInCommand++;
                }
                else
                {
                    break;
                }
            }

            return (commandPath.ToString() ?? "", tokensInCommand);

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
            this.tokens.Add(this.TokenPosition, token);
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
            this.tokens.Add(this.TokenPosition, token);
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
            // TODO [ ][RESOLVER] Resolve git parameters with integer values appended e.g. -U5
            // Regex "^-([a-zA-z])([0-9]+)*$" First capture group is argument, second is value.
            // Beware, this is Git specific and may cause problems with other commands which do not require splitting.
            // Perhaps we need to record first command visited so that we can change visitor behaviour as we
            // process tokens (check token zero).
            // NOTE: git-am with -S option, the GPG key must be directly appended to the option (no space). GPG Key-ids are
            // 16 hex character (long) or 8 hex character (short). "^-S[0-9a-fA-F]{8|16}$".
            Token token = new()
            {
                text = commandParameterAst.ToString(),
                type = commandParameterAst.GetType(),
            };
            this.tokens.Add(this.TokenPosition, token);
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

            this.tokens.Add(this.TokenPosition, token);
#if DEBUG
            LOGGER.Write($"String constant expression: {token.text}, {token.type}");
#endif
            return AstVisitAction.Continue;
        }
    }
}