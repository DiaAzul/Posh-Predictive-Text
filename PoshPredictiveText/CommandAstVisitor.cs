
namespace PoshPredictiveText
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
    internal record Token
    {
        internal string Value { get; init; } = default!;
        internal Type Type { get; init; } = typeof(StringConstantExpressionAst);

        /// <summary>
        /// True if the token is a command.
        /// </summary>
        internal bool IsCommand
        {
            get { return Type == typeof(CommandAst); }
        }

        /// <summary>
        /// True if the token is a command expression.
        /// </summary>
        internal bool IsCommandExpression
        {
            get { return Type == typeof(CommandExpressionAst); }
        }

        /// <summary>
        /// True if the token is a command parameter.
        /// </summary>
        internal bool IsCommandParameter
        {
            get { return Type == typeof(CommandParameterAst); }
        }

        /// <summary>
        /// True if the token is a string constant expression.
        /// </summary>
        internal bool IsStringConstantExpression
        {
            get { return Type == typeof(StringConstantExpressionAst); }
        }
    }

    /// <summary>
    /// Implements a visitor for the PowerShell abstract syntax tree.
    /// Reads the input no the command line and creates an ordered list of
    /// tokens representing user input.
    /// </summary>
    internal class CommandAstVisitor : AstVisitor
    {
        // List of tokens the key represents the position of the token
        // on the command line.
        private readonly Dictionary<int, Token> tokens;
        // Incremented each time a new token is created.
        // Indicates position of token on the command line.
        private int commandLinePosition = 0;

        // Sets the parsing mode for the command line.
        private ParseMode? parseMode = null;

        /// <summary>
        /// Class construtor initialising the token dictionary
        /// </summary>
        internal CommandAstVisitor()
        {
            this.tokens = new Dictionary<int, Token>();
        }

        /// <summary>
        /// Returns the position of the token and updates the position count.
        /// </summary>
        private int TokenPosition
        { get { return commandLinePosition++; } }

        /// <summary>
        /// Returns the first token in the command list.
        /// </summary>
        internal string? BaseCommand
        {
            get { return this.Index(0)?.Value.ToLower(); }
        }

        /// <summary>
        /// Returns the ParseMode for the command.
        /// 
        /// Command line argument parsing varies depending upon the heritage
        /// of the command. Windows and Posix systems have different symbols
        /// and policies with respect to arguments. This continues to other
        /// commands which may have their own policies. Once the base command
        /// is identified the parseMode is set so that the visitor can implement
        /// policies which correctly parse the arguments.
        /// </summary>
        internal ParseMode ParseMode
        {
            get
            {
                if (this.parseMode is null && BaseCommand is not null)
                    parseMode = SyntaxTreesConfig.ParseMode(BaseCommand);
                return parseMode ?? ParseMode.Windows;
            }
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
            get { return this.tokens.Count; }
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

        internal void AddDefaultToken()
        {
            Token token = new()
            {
                Value = "",
                Type = typeof(string)
            };
            this.tokens.Add(this.TokenPosition, token);
        }

        /// <summary>
        /// Returns the command path given a list of unique commands.
        /// 
        /// Command line tools often have sub-commands which are tokens entered after the
        /// main command. To identify options specific to each command/sub-command a
        /// command path is generated based upon unique commands known to the parser and
        /// command tokens entered at the command line. The command path is a dot notation
        /// identifier for each command/sub-command combination.
        /// </summary>
        /// <param name="uniqueCommands">List of unique commands/sub-commands.</param>
        /// <returns>Dot notation path identifying the command/sub-command.</returns>
        internal (string, int) CommandPath(List<string> uniqueCommands)
        {

            StringBuilder commandPath = new(capacity: 64);
            int tokensInCommand = 0;
            string delimeter = "";
            foreach (var (position, commandToken) in this.tokens)
            {
                string tokenText = commandToken.Value.ToLower();
                if (uniqueCommands.Contains(tokenText))
                {
                    commandPath.Append(delimeter);
                    commandPath.Append(tokenText);
                    delimeter = ".";
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
        /// For any provided syntaxItem identify whether it can be used on the command line.
        /// The parameter cannot be used if it is a single use parameter and has already
        /// been used on the command line.
        /// </summary>
        /// <param name="syntaxItem">Returns true if a paramter can be used.</param>
        /// <returns></returns>
        internal bool CanUse(SyntaxItem syntaxItem)
        {
            // If item can be used multiple times then we do not need to
            // check that it has been used on the command line already.
            if (syntaxItem.MultipleUse) return true;

            bool match = true;
            foreach (Token token in this.tokens.Values)
            {
                if (syntaxItem.Argument is not null && token.Value == syntaxItem.Argument)
                {
                    match = false;
                    break;
                }
            }
            return match;
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
            //this.tokens.Add(this.TokenPosition, token);
            var t = ast.GetType();
#if DEBUG
            LOGGER.Write($"Default (not tokenised): {token.Value}, {t}");
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
                Type = commandExpressionAst.GetType(),
            };
            this.tokens.Add(this.TokenPosition, token);
#if DEBUG
            LOGGER.Write($"Command expression: {token.Value}, {token.Type}");
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
            // TODO [ ][VISITOR] Resolve git parameters with integer values appended e.g. -U5
            // Regex "^-([a-zA-z])([0-9]+)*$" First capture group is argument, second is value.
            // Beware, this is Git specific and may cause problems with other commands which do not require splitting.
            // Perhaps we need to record first command visited so that we can change visitor behaviour as we
            // process tokens (check token zero).
            // NOTE: git-am with -S option, the GPG key must be directly appended to the option (no space). GPG Key-ids are
            // 16 hex character (long) or 8 hex character (short). "^-S[0-9a-fA-F]{8|16}$".

            if (ParseMode == ParseMode.Posix)
            {
                // In a Posix system multiple parameters may be concatenated after the single dash.
                // (e.g. -abc is -a -b -c). It is also possible to concatenate the parameters value directly
                // after the parameter flag. (e.g. -p0 is the same as -p 0).
            }
            Token token = new()
            {
                Value = commandParameterAst.ToString(),
                Type = commandParameterAst.GetType(),
            };
            this.tokens.Add(this.TokenPosition, token);
#if DEBUG
            LOGGER.Write($"Command parameter: {token.Value}, {token.Type}");
#endif
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
                Type = constantExpressionAst.GetType(),
            };
            this.tokens.Add(this.TokenPosition, token);
#if DEBUG
            LOGGER.Write($"Constant expression: {token.Value}, {token.Type}");
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
            string Value = stringConstantExpressionAst.ToString();
            Type type = stringConstantExpressionAst.StaticType;
            // Double dashed parameters are parsed by PowerShell as String Constant Expressions.
            // Reclassify them as CommandParameters.
            // TODO [ ][VISITOR] Need to consider additional flag options such as + - * > >>.
            try
            {
                if (Value[..2] == "--")
                {
                    type = typeof(CommandParameterAst);
                }
            }
            catch (ArgumentOutOfRangeException) { }
            Token token = new()
            {
                Value = Value,
                Type = type
            };
            this.tokens.Add(this.TokenPosition, token);
#if DEBUG
            LOGGER.Write($"String constant expression: {token.Value}, {token.Type}");
#endif
            return AstVisitAction.Continue;
        }
    }
}