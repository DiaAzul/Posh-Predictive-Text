
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
        /// Evaluate string constant expression. This is most likely
        /// to evaluate to either a command or positional value.
        /// </summary>
        /// <param name="token">Token to evaluate.</param>
        /// <returns>Enhanced token.</returns>
        internal List<SemanticToken> EvaluateStringConstant(SemanticToken token)
        {
            string enteredValue = token.Value.ToLower();

            List<SyntaxItem> subCommands = syntaxTree!.SubCommands(this.ms.CommandPath.ToString());

            List<SyntaxItem> suggestedCommands = subCommands
                .Where(syntaxItem => syntaxItem.Name?.StartsWith(enteredValue) ?? false)
                .ToList();

            List<SemanticToken> resultTokens;
            switch (suggestedCommands.Count)
            {
                // If we don't recognise a command or partial command perhaps this is a positional value.
                case 0:
                    this.ms.CurrentState = MachineState.State.Value;
                    token.SemanticType = SemanticToken.TokenType.PositionalValue;
                    resultTokens = EvaluateValue(token);
                    break;
                // When there is one suggestion and it is an exact match.
                case 1 when enteredValue == suggestedCommands.First().Name:
                    this.ms.CommandPath.Add(enteredValue);
                    token.IsComplete = true;
                    token.SemanticType = SemanticToken.TokenType.Command;
                    this.ms.CurrentState = MachineState.State.Item;
                    resultTokens = new() { token };
                    break;
                // Otherwise add suggestions to response.
                default:
                    token.SuggestedSyntaxItems = suggestedCommands;
                    token.IsComplete = false;
                    this.ms.CurrentState = MachineState.State.Item;
                    resultTokens = new() { token };
                    break;
            }
            return resultTokens;
        }
    }
}
