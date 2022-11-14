
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
            this.ms.SyntaxTreeName=syntaxTreeName;
            this.ms.SyntaxTree=syntaxTree;
            this.ms.ParseMode=parseMode;
            this.ms.CurrentState = currentState;
            this.ms.CommandPath=commandPath;
        }

        /// <summary>
        /// Name of the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal string? SyntaxTreeName => this.ms.SyntaxTreeName;

        /// <summary>
        /// Get the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal SyntaxTree? SyntaxTree => this.ms.SyntaxTree;

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
            this.ms.SyntaxTreeName = null;
            this.ms.SyntaxTree = null;
            this.ms.ParseMode= ParseMode.Windows;
            this.ms = new();
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
        /// <returns>Tokens enhanced with semantic information.</returns>
        internal List<SemanticToken> Evaluate(SemanticToken token)
        {
            List<SemanticToken> semanticTokens;
            string cacheKey = this.ms.CommandPath + "+" + token.Value;

            LOGGER.Write($"STATE MACHINE: Evaluate {token.Value}.");

            // If the result is in the cache return early.
            if (MachineStateCache.TryGetValue(cacheKey, out MachineState cachedMachineState))
            {
                LOGGER.Write($"STATE MACHINE: Use cached token results with key {cacheKey}.");
                ms = cachedMachineState.DeepCopy();
                return this.ms.SemanticTokens ?? new List<SemanticToken>();
            }

            // Parse the token and add semantic information.
            LOGGER.Write("STATE MACHINE: Parsing the token.");
            semanticTokens = this.ms.CurrentState switch
            {
                MachineState.State.NoCommand => NoCommand(token),
                MachineState.State.Item => EvaluateItem(token),
                MachineState.State.Value => EvaluateValue(token),
                MachineState.State.Inert => new List<SemanticToken> { token },
                _ => new List<SemanticToken> { token },
            };

            // Update parameter sets.
            if (semanticTokens.Count > 0 && semanticTokens.First().IsComplete)
            {
                foreach (var semanticToken in semanticTokens)
                {
                    if (semanticToken.IsCommand || semanticToken.IsParameter || semanticToken.IsPositionalValue)
                    {
                        if (ms.ParameterSets is null)
                        {
                            ms.ParameterSets = semanticToken.ParameterSet;
                        }
                        else if (semanticToken.ParameterSet is not null)
                        {
                            ms.ParameterSets = (List<string>?)ms.ParameterSets.Intersect(semanticToken.ParameterSet);
                        }
                    }
                }
                LOGGER.Write($"STATE MACHINE: Resultant parameter set is {ms.ParameterSets}.");
            }

            // Cache the result if the argument is complete.
            if (semanticTokens.Count > 0
                && semanticTokens.First().IsComplete
                && !(semanticTokens.First().SemanticType == SemanticToken.TokenType.Parameter
                    || semanticTokens.First().SemanticType == SemanticToken.TokenType.PositionalValue))
            {
                LOGGER.Write($"STATE MACHINE: Caching parsed token results with key {cacheKey}.");
                ms.SemanticTokens = semanticTokens;
                MachineStateCache.Add(cacheKey, ms.DeepCopy());
            }

            LOGGER.Write($"STATE MACHINE: Returning {semanticTokens.Count} semantic tokens.");
            return semanticTokens;
        }
    }
}
