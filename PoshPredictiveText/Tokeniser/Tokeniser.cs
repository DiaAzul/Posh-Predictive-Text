
namespace PoshPredictiveText
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    /// <summary>
    /// The tokeniser provides semantic analysis of the text
    /// entered on the command line. The meaning of text is
    /// dependent upon the command as defined within the 
    /// syntax tree.
    /// </summary>
    internal class Tokeniser
    {
        /// <summary>
        /// List of tokens, the key represents the order of the token
        /// on the command line.
        /// </summary>
        private readonly Dictionary<int, Token> tokens = new();

        private readonly StateMachine stateMachine = new();

        /// <summary>
        /// Incremented each time a new token is created.
        /// Indicates position of token on the command line.
        /// </summary>
        private int commandLinePosition = 0;

        /// <summary>
        /// Sets the parsing mode for the command line.
        /// </summary>
        private ParseMode? parseMode = null;

        ///// <summary>
        ///// Name of the syntax tree once the base command is identified.
        ///// </summary>
        //private readonly string? syntaxTreeName = null;

        internal string? SyntaxTreeName
        {
            get { return stateMachine.SyntaxTreeName; }
        }

        internal SyntaxTree? SyntaxTree
        {
            get { return stateMachine.SyntaxTree; }
        }

        /// <summary>
        /// Add token to the list.
        /// </summary>
        /// <param name="token">Token to add to the tokeniser</param>
        internal void Add(Token token)
        {
#if DEBUG
            LOGGER.Write($"PARSE TOKEN: {token.Value}, {token.SemanticType}");
#endif
            List<Token> returnedTokens = stateMachine.Evaluate(token);

            foreach (Token returnedToken in returnedTokens)
            {
                this.tokens.Add(this.TokenPosition, returnedToken);
            }           
        }

        /// <summary>
        /// Add default token to the list.
        /// </summary>
        internal void Add()
        {
            Token token = new()
            {
                Value = "",
                SemanticType = null
            };
            this.Add(token);
        }

        /// <summary>
        /// Resets the Tokeniser to the initial state.
        /// </summary>
        internal void Reset()
        {
            tokens.Clear();
            stateMachine.Reset();
            commandLinePosition = 0;
            parseMode = null;
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
        /// Returns the first token if it exists.
        /// </summary>
        internal Token? FirstToken
        {
            get { return this.Index(0); }
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
                return this.tokens?.Where(item => item.Value.IsParameter)
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

        /// <summary>
        /// Returns the commandPath for commands
        /// entered on the command line.
        /// </summary>
        internal CommandPath CommandPath
        {
            get { return stateMachine.CommandPath; }
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
    }
}
