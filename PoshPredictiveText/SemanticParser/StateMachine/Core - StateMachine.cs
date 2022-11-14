
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
        private MachineState machineState = new();

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
        /// <param name="syntaxTreeName">Name of the syntax tree</param>
        /// <param name="syntaxTree">Syntax tree</param>
        /// <param name="parseMode">Parse mode (Windows, Posix, Python)</param>
        /// <param name="commandPath">Command Path</param>
        internal StateMachine(MachineState.State currentState, string? syntaxTreeName, SyntaxTree? syntaxTree, ParseMode parseMode, CommandPath commandPath)
        {
            this.machineState.SyntaxTreeName = syntaxTreeName;
            this.machineState.SyntaxTree = syntaxTree;
            this.machineState.ParseMode = parseMode;
            this.machineState.CurrentState = currentState;
            this.machineState.CommandPath = commandPath;
        }

        /// <summary>
        /// Name of the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal string? SyntaxTreeName => this.machineState.SyntaxTreeName;

        /// <summary>
        /// Get the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal SyntaxTree? SyntaxTree => this.machineState.SyntaxTree;

        /// <summary>
        /// Get the Command Path for command tokens entered on the command line.
        /// </summary>
        internal CommandPath CommandPath => this.machineState.CommandPath;

        /// <summary>
        /// Get the current state of the state machine.
        /// </summary>
        internal MachineState.State CurrentState => this.machineState.CurrentState;

        /// <summary>
        /// Get the Parameter sets available for this command path
        /// at this point in the command line.
        /// </summary>
        internal List<string>? ParameterSets => this.machineState.ParameterSet;

        /// <summary>
        /// Reset the state machine to the initial state.
        /// </summary>
        internal void Reset()
        {
            this.machineState.SyntaxTreeName = null;
            this.machineState.SyntaxTree = null;
            this.machineState.ParseMode= ParseMode.Windows;
            this.machineState = new();
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
            string cacheKey = this.machineState.CommandPath + "+" + token.Value;

            LOGGER.Write($"STATE MACHINE: Evaluate {token.Value}.");

            // If the result is in the cache return early.
            if (MachineStateCache.TryGetValue(cacheKey, out MachineState cachedMachineState))
            {
                LOGGER.Write($"STATE MACHINE: Use cached token results with key {cacheKey}.");
                machineState = cachedMachineState.DeepCopy();
                return this.machineState.SemanticTokens ?? new List<SemanticToken>();
            }

            // Parse the token and add semantic information.
            LOGGER.Write("STATE MACHINE: Parsing the token.");
            semanticTokens = this.machineState.CurrentState switch
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
                        if (machineState.ParameterSet is null)
                        {
                            machineState.ParameterSet = semanticToken.ParameterSet;
                        }
                        else if (semanticToken.ParameterSet is not null)
                        {
                            LOGGER.Write($"STATE MACHINE: machine state parameter set is {String.Join(", ",machineState.ParameterSet)}.");
                            LOGGER.Write($"STATE MACHINE: semantic token parameter set is {String.Join(", ", semanticToken.ParameterSet)}.");
                            machineState.ParameterSet = machineState.ParameterSet.Intersect(semanticToken.ParameterSet).ToList();
                        }
                    }
                }
                LOGGER.Write($"STATE MACHINE: Resultant parameter set is {String.Join(", ", machineState.ParameterSet ?? new List<string>())}.");
            }

            // Cache the result if the argument is complete.
            if (semanticTokens.Count > 0
                && semanticTokens.First().IsComplete
                && !(semanticTokens.First().SemanticType == SemanticToken.TokenType.Parameter
                    || semanticTokens.First().SemanticType == SemanticToken.TokenType.PositionalValue))
            {
                LOGGER.Write($"STATE MACHINE: Caching parsed token results with key {cacheKey}.");
                machineState.SemanticTokens = semanticTokens;
                MachineStateCache.Add(cacheKey, machineState.DeepCopy());
            }

            LOGGER.Write($"STATE MACHINE: Returning {semanticTokens.Count} semantic tokens.");
            return semanticTokens;
        }
    }
}
