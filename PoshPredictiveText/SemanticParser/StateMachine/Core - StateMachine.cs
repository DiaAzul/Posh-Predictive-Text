
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;

    /// <summary>
    /// The state machine evaluates the command line input and appends
    /// semantic information to each semanticToken.
    /// 
    /// The semantic information is sourced from the syntax tree and used
    /// to determine the semanticToken type.
    /// </summary>
    internal partial class StateMachine
    {
        /// <summary>
        /// Machine state. Held as a cacheable object.
        /// </summary>
        private MachineState machineState = new();

        /// <summary>
        /// Cached machine state.
        /// 
        /// The Predictor is called each time a new character is entered on the command
        /// line. This generates repeated calls for the state machine to process the
        /// command line. To reduce processing time, successfully parsed tokens are cached
        /// so that they can be recalled on successive calls to the state machine.
        /// 
        /// Warning: Objects stored within the cache should be deepcopied when stashed
        /// and deepcopied when pulled. This prevents side-effects where an object is changed
        /// after stashing it, which also changes elements of the stashed object, and also
        /// changing a pulled object which would also update elements of the stashed object.
        /// </summary>
        private readonly Dictionary<string, MachineState> cache = new();

        /// <summary>
        /// Initialise an empty state machine.
        /// 
        /// The state machine adds semantic information to command line tokens.
        /// </summary>
        internal StateMachine() { }

        /// <summary>
        /// Initialise a state machine with a given state (used in testing).
        /// 
        /// The state machine adds semantic information to command line tokens. 
        /// </summary>
        /// <param name="state">State of the machine</param>
        /// <param name="syntaxTreeName">Name of the syntax tree</param>
        /// <param name="syntaxTree">Syntax tree</param>
        /// <param name="parseMode">Parse mode (Windows, Posix, Python)</param>
        /// <param name="commandPath">Command Path</param>
        internal StateMachine(MachineState.State currentState, string? syntaxTreeName, SyntaxTree? syntaxTree, ParseMode parseMode, CommandPath commandPath)
        {
            machineState.SyntaxTreeName = syntaxTreeName;
            machineState.SyntaxTree = syntaxTree;
            machineState.ParseMode = parseMode;
            machineState.CurrentState = currentState;
            machineState.CommandPath = commandPath;
        }

        /// <summary>
        /// Name of the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal string? SyntaxTreeName => machineState.SyntaxTreeName;

        /// <summary>
        /// Get the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal SyntaxTree? SyntaxTree => machineState.SyntaxTree;

        /// <summary>
        /// Get the Command Path for command tokens entered on the command line.
        /// </summary>
        internal CommandPath CommandPath => machineState.CommandPath;

        /// <summary>
        /// Get the current state of the state machine.
        /// </summary>
        internal MachineState.State CurrentState => machineState.CurrentState;

        internal string? BaseCommand => machineState.CLISemanticTokens.Count > 0 ? machineState.CLISemanticTokens[0].Value : null;

        internal SemanticToken? LastToken => machineState.CLISemanticTokens.Count > 0 ? machineState.CLISemanticTokens[^1] : null;

        internal SemanticToken? PriorToken => machineState.CLISemanticTokens.Count > 1 ? machineState.CLISemanticTokens[^2] : null;

        internal List<SemanticToken> All => machineState.CLISemanticTokens;

        internal int Count => machineState.CLISemanticTokens.Count;

        /// <summary>
        /// Return the semanticToken at the index position in the list.
        /// </summary>
        /// <param name="index">Index position of required semanticToken.</param>
        /// <returns>Token at the position in the list, null if index outside of scope of list.</returns>
        internal SemanticToken? Index(int index)
        {
            SemanticToken? semanticToken;
            try
            {
                semanticToken = this.machineState.CLISemanticTokens[index];
            }
            catch (Exception ex) when (
            ex is ArgumentOutOfRangeException
            || ex is IndexOutOfRangeException
            || ex is KeyNotFoundException)
            {
                semanticToken = null;
            }
            return semanticToken;
        }

        /// <summary>
        /// Get the Parameter sets available for this command path
        /// at this point in the command line.
        /// </summary>
        internal List<string>? ParameterSets => machineState.ParameterSet;

        /// <summary>
        /// Reset the state machine to the initial state.
        /// </summary>
        internal void Reset()
        {
            machineState = new();
            cache.Clear();
        }

        /// <summary>
        /// Main entry point to the state machine.
        /// 
        /// The routing function evaluates tokens depending upon the state
        /// of the machine and the semanticToken value.
        /// 
        /// The machine may split tokens and return multiple semantically enhanced
        /// tokens for a given input semanticToken.
        /// </summary>
        /// <param name="tokens">Input tokens to which semantic information will be added</param>
        /// <returns>Tokens enhanced with semantic information.</returns>
        internal List<SemanticToken> Evaluate(SemanticToken token)
        {
            List<SemanticToken> semanticTokens;
            string cacheKey = machineState.CommandPath + "+" + token.Value;

            LOGGER.Write($"STATE MACHINE: Evaluate '{token.Value}'.");

            // If the result is in the cache return early.
            if (cache.TryGetValue(cacheKey, out MachineState? cachedMachineState))
            {
                LOGGER.Write($"STATE MACHINE: Using cached token results with key {cacheKey}.");

                machineState = cachedMachineState.DeepCopy();
                semanticTokens= machineState.SemanticTokens ?? new List<SemanticToken>();

                LOGGER.Write($"STATE MACHINE: Returning {semanticTokens.Count} semantic tokens.");
                LOGGER.Write($"STATE MACHINE: Returning {semanticTokens.First()?.Suggestions?.Count ?? 0} suggestions.");
                LOGGER.Write($"STATE MACHINE: Resultant parameter set is {string.Join(", ", machineState.ParameterSet ?? new List<string>())}.");

                return semanticTokens;
            }

            LOGGER.Write("STATE MACHINE: Parsing the token.");

            semanticTokens = machineState.CurrentState switch
            {
                MachineState.State.NoCommand => NoCommand(token),
                MachineState.State.Item => EvaluateItem(token),
                MachineState.State.Value => EvaluateParameterValue(token),
                MachineState.State.Inert => new List<SemanticToken> { token },
                _ => new List<SemanticToken> { token },
            };

            // If we have an exact match then process parameter sets and cache results if appropriate.
            if (semanticTokens.Count > 0 && semanticTokens.First().IsExactMatch)
            {
                switch (semanticTokens.First().SemanticType)
                {
                    case SemanticToken.TokenType.Parameter:
                    case SemanticToken.TokenType.PositionalValue:
                        foreach (var semanticToken in semanticTokens)
                        {
                            if (machineState.ParameterSet is null)
                            {
                                machineState.ParameterSet = semanticToken.ParameterSet;
                            }
                            else if (semanticToken.ParameterSet is not null)
                            {
                                LOGGER.Write($"STATE MACHINE: machine state parameter set is {String.Join(", ", machineState.ParameterSet)}.");
                                LOGGER.Write($"STATE MACHINE: semantic token parameter set is {String.Join(", ", semanticToken.ParameterSet)}.");
                                machineState.ParameterSet = machineState.ParameterSet.Intersect(semanticToken.ParameterSet).ToList();
                            }
                            LOGGER.Write($"STATE MACHINE: Resultant parameter set is {String.Join(", ", machineState.ParameterSet ?? new List<string>())}.");
                        }
                        goto default;

                    case SemanticToken.TokenType.Command:
                        LOGGER.Write($"STATE MACHINE: Reset parameter sets for command {semanticTokens.First().Value}");
                        machineState.ParameterSet = null;
                        goto default;

                    default:
                        LOGGER.Write($"STATE MACHINE: Caching parsed tokens with key {cacheKey}.");
                        machineState.SemanticTokens = semanticTokens;
                        cache.Add(cacheKey, machineState.DeepCopy());
                        break;
                }
            }

            LOGGER.Write($"STATE MACHINE: Returning {semanticTokens.Count} semantic tokens.");
            LOGGER.Write($"STATE MACHINE: Returning {semanticTokens.First()?.Suggestions?.Count ?? 0} suggestions.");

            machineState.CLISemanticTokens.AddRange(semanticTokens);
            LOGGER.Write($"STATE MACHINE: There are {machineState.CLISemanticTokens.Count} semantic tokens on the CLI.");

            return semanticTokens;
        }
    }
}
