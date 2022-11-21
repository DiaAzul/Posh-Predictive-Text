
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using System.Management.Automation;

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
        /// Evaluate string constant expression. This is most likely
        /// to evaluate to either a command or positional value.
        /// </summary>
        /// <param name="token">Token to evaluate.</param>
        /// <returns>Enhanced token.</returns>
        internal List<SemanticToken> EvaluateStringConstant(SemanticToken token)
        {
            List<SyntaxItem> subCommands = machineState.SyntaxTree!
                                                    .FilteredByCommandPath(machineState.CommandPath.ToString())
                                                    .Where(syntaxItem => syntaxItem.IsCommand
                                                        && (syntaxItem.Name?.StartsWith(token.Value) ?? false)
                                                            || (syntaxItem.Alias?.StartsWith(token.Value) ?? false))
                                                    .ToList();

            // Branch execution based upon the number of matching suggestions returned.
            // Zero - no command was recognised, but this may be a positional value.
            // One - Direct match of a command name.
            // More than one - Provide suggested completions.
            List<SemanticToken> semanticTokens;
            switch (subCommands.Count)
            {
                case 0:
                    machineState.CurrentState = MachineState.State.Value;
                    token.SemanticType = SemanticToken.TokenType.PositionalValue;
                    semanticTokens = EvaluateParameterValue(token);
                    break;

                case 1 when subCommands.First().Name.Equals(token.Value, StringComparison.OrdinalIgnoreCase):
                    // Update the command path and reset the parameter set.
                    machineState.CommandPath.Add(token.Value.ToLower());
                    token.ParameterSet = subCommands.First().ParameterSet;

                    token.IsExactMatch = true;
                    token.SemanticType = SemanticToken.TokenType.Command;
                    //token.Suggestions = suggestions;

                    semanticTokens = SuggestNextToken(token);
                    machineState.CurrentState = MachineState.State.Item;
                    break;

                default:
                    token.IsExactMatch = false;
                    semanticTokens = SuggestNextToken(token);
                    machineState.CurrentState = MachineState.State.Item;
                    break;
            }
            return semanticTokens;
        }
    }
}
