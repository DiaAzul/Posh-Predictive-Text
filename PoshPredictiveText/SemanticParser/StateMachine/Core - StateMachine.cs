
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
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
        /// Name of the syntax tree.
        /// </summary>
        private string? syntaxTreeName = null;

        /// <summary>
        /// Syntax tree, loaded once the base command is identified.
        /// </summary>
        private SyntaxTree? syntaxTree = null;

        /// <summary>
        /// Parse mode for the command.
        /// </summary>
        private ParseMode parseMode = ParseMode.Windows;

        /// <summary>
        /// Machine state. Held as a cacheable object.
        /// </summary>
        private MachineState ms = new();

        /// <summary>
        /// StateMachine adds semantic information to command line tokens.
        /// 
        /// Initialise an empty state machine.
        /// </summary>
        internal StateMachine() { }

        /// <summary>
        /// StateMachine adds semantic information to command line tokens. 
        /// 
        /// Initialise a state machine with a given state (used in testing).
        /// </summary>
        /// <param name="state">State of the machine</param>
        /// <param name="syntaxTreeName">Name of syntax tree</param>
        /// <param name="syntaxTree">Syntax tree</param>
        /// <param name="parseMode">Parse mode (Windows, Posix, Python)</param>
        /// <param name="commandPath">Command Path</param>
        internal StateMachine(MachineState.State currentState, string? syntaxTreeName, SyntaxTree? syntaxTree, ParseMode parseMode, CommandPath commandPath)
        {
            this.syntaxTreeName=syntaxTreeName;
            this.syntaxTree=syntaxTree;
            this.parseMode=parseMode;
            this.ms.CurrentState = currentState;
            this.ms.CommandPath=commandPath;
        }

        /// <summary>
        /// Name of the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal string? SyntaxTreeName => this.syntaxTreeName;

        /// <summary>
        /// Get the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal SyntaxTree? SyntaxTree => this.syntaxTree;

        /// <summary>
        /// Get the Command Path for command tokens entered on the command line.
        /// </summary>
        internal CommandPath CommandPath => this.ms.CommandPath;

        /// <summary>
        /// Get the current state of the state machine.
        /// </summary>
        internal MachineState.State CurrentState => this.ms.CurrentState;

        /// <summary>
        /// Get the Parameter sets available for this command path
        /// at this point in the command line.
        /// </summary>
        internal List<string>? ParameterSets => this.ms.ParameterSets;

        /// <summary>
        /// Reset the state machine to the initial state.
        /// </summary>
        internal void Reset()
        {
            this.ms = new();
            this.syntaxTreeName = null;
            this.syntaxTree = null;
            MachineStateCache.Reset();
        }

        /// <summary>
        /// Main entry point to the state machine.
        /// 
        /// The routing function evaluates tokens depending upon the state
        /// of the machine and the token value.
        /// 
        /// The machine may split tokens and return multiple semantically enhanced
        /// tokens for a given input token.
        /// </summary>
        /// <param name="tokens">Input tokens to which semantic information will be added</param>
        /// <returns>Tokens with semantic information added.</returns>
        internal List<SemanticToken> Evaluate(SemanticToken token)
        {
            LOGGER.Write($"STATE MACHINE: Evaluate {token.Value}.");

            List<SemanticToken> returnTokens;
            string cacheKey = this.ms.CommandPath + "+" + token.Value;
            bool syntaxTreeLoaded = syntaxTree is not null;

            // If the result is already in the cache return early.
            if (MachineStateCache.TryGetValue(cacheKey, out MachineState cachedMachineState))
            {
                LOGGER.Write("STATE MACHINE: Use cached token results.");
                ms = cachedMachineState.DeepCopy();
                return this.ms.ReturnTokens ?? new List<SemanticToken>();
            }

            // Otherwise parse the argument and add semantic information.
            LOGGER.Write("STATE MACHINE: Parse argument.");
            returnTokens = this.ms.CurrentState switch
            {
                MachineState.State.NoCommand => NoCommand(token),
                MachineState.State.Item => EvaluateItem(token),
                MachineState.State.Value => EvaluateValue(token),
                MachineState.State.Inert => new List<SemanticToken> { token },
                _ => new List<SemanticToken> { token },
            };

            //// TODO [HIGH][STATEMACHINE] Update Parameter sets.
            //if (returnTokens.Count > 0 && returnTokens.First().IsComplete)
            //{
            //    foreach (var returnToken in returnTokens)
            //    {
            //        if (returnToken.IsCommand || returnToken.IsParameter || returnToken.IsPositionalValue)
            //        {

            //        }
            //    }
            //}

            // Cache the result if the argument is complete.
            if (
                syntaxTreeLoaded
                && returnTokens.Count > 0
                && returnTokens.First().IsComplete)
            {
                switch (returnTokens.First().SemanticType)
                {
                    // Do not cache parameter and positional values.
                    case SemanticToken.TokenType.ParameterValue:
                    case SemanticToken.TokenType.PositionalValue:
                        break;
                    default:
                        LOGGER.Write("STATE MACHINE: Cache parsed token results.");
                        ms.ReturnTokens = returnTokens;
                        MachineStateCache.Add(cacheKey, ms.DeepCopy());
                        break;
                }
            }

            LOGGER.Write($"STATE MACHINE: Complete {token.Value} evaluation.");
            return returnTokens;
        }
    }
}
