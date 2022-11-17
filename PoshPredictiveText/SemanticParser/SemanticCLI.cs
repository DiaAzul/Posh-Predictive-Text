
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
    /// <summary>
    /// The Semantic CLI analyses text entered on the command line
    /// and generates a sequence of parsed tokens expressing its meaning. 
    /// The meaning of the text is defined in the command's syntax tree.
    /// </summary>
    internal class SemanticCLI
    {
        /// <summary>
        /// List of tokens, the key represents the order of the token
        /// on the command line.
        /// </summary>
        private readonly Dictionary<int, SemanticToken> tokens = new();

        /// <summary>
        /// The state machine retains the state of the command line as each
        /// token is added.
        /// </summary>
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
        internal void AddToken(SemanticToken token)
        {
            LOGGER.Write($"SEMANTIC CLI: Parse '{token.Value}' of type {token.SemanticType}.");

            List<SemanticToken> semanticTokens = stateMachine.Evaluate(token);

            // TODO: [MED][SemanticCLI] The list of tokens is in the state machine, this can be removed.
            foreach (SemanticToken semanticToken in semanticTokens)
            {
                this.tokens.Add(this.TokenPosition, semanticToken);
            }

            LOGGER.Write($"SEMANTIC CLI: Added {semanticTokens.Count} tokens, there are now {this.tokens.Count} tokens.");
        }

        /// <summary>
        /// Add default token to the list.
        /// </summary>
        internal void Add()
        {
            SemanticToken token = new()
            {
                Value = "",
                SemanticType = SemanticToken.TokenType.Space
            };
            this.AddToken(token);
        }

        /// <summary>
        /// Resets the SemanticCLI to the initial state.
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
        internal SemanticToken? FirstToken
        {
            get { return this.Index(0); }
        }

        /// <summary>
        /// Returns the last token in the command list.
        /// </summary>
        internal SemanticToken? LastToken
        {
            get { return this.Index(this.tokens.Count - 1); }
        }

        /// <summary>
        /// Returns the second to last token in the command list.
        /// </summary>
        internal SemanticToken? PriorToken
        {
            get { return this.Index(this.tokens.Count - 2); }
        }

        /// <summary>
        /// Returns a list of all tokens.
        /// </summary>
        internal Dictionary<int, SemanticToken> All
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
        internal Dictionary<int, SemanticToken> CommandParameters
        {
            get
            {
                return this.tokens?.Where(item => item.Value.IsParameter)
                            .ToDictionary(item => item.Key, item => item.Value)
                            ?? new Dictionary<int, SemanticToken>();
            }
        }

        /// <summary>
        /// Return the token at the index position in the list.
        /// </summary>
        /// <param name="index">Index position of required token.</param>
        /// <returns>Token at the position in the list, null if index outside of scope of list.</returns>
        internal SemanticToken? Index(int index)
        {
            SemanticToken? token;
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
            // TODO [HIGH[TOKENISER] Calculate how many times used and whether still can do
            if (syntaxItem.MaxCount is null) return true;

            bool match = true;
            foreach (SemanticToken token in this.tokens.Values)
            {
                if (syntaxItem.Name is not null && token.Value == syntaxItem.Name)
                {
                    match = false;
                    break;
                }
            }
            return match;
        }
    }
}
