
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;

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
            string enteredValue = token.Value.ToLower();

            List<SyntaxItem> subCommands = machineState.SyntaxTree!.SubCommands(machineState.CommandPath.ToString());

            List<SyntaxItem> syntaxItems = subCommands
                .Where(syntaxItem => syntaxItem.Name?.StartsWith(enteredValue) ?? false)
                .ToList();

            List<SemanticToken> semanticTokens;
            // Branch execution based upon the number of matching syntaxItems returned.
            // Zero - no command was recognised, but this may be a positional value.
            // One - Direct match of a command name.
            // More than one - Provide suggested completions.
            switch (syntaxItems.Count)
            {
                case 0:
                    machineState.CurrentState = MachineState.State.Value;
                    token.SemanticType = SemanticToken.TokenType.PositionalValue;
                    semanticTokens = EvaluateValue(token);
                    break;

                case 1 when enteredValue == syntaxItems.First().Name:
                    // Update the command path and reset the parameter set.
                    machineState.CommandPath.Add(enteredValue);
                    machineState.ParameterSet = null; // TODO AAAARRRRRGGGH!
                    token.IsComplete = true;
                    token.SemanticType = SemanticToken.TokenType.Command;
                    token.ParameterSet = syntaxItems.First().ParameterSet;
                    machineState.CurrentState = MachineState.State.Item;
                    semanticTokens = new() { token };
                    break;

                default:
                    token.SuggestedSyntaxItems = syntaxItems;
                    token.IsComplete = false;
                    machineState.CurrentState = MachineState.State.Item;
                    semanticTokens = new() { token };
                    break;
            }
            return semanticTokens;
        }
    }
}
