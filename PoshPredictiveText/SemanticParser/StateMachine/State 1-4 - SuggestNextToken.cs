
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
        internal List<SemanticToken> SuggestNextToken(SemanticToken token)
        {
            List<SyntaxItem> syntaxItems = machineState.SyntaxTree!.FilteredByCommandPath(machineState.CommandPath.ToString());
            if (machineState.ParameterSet is not null)
            {
                syntaxItems = syntaxItems.Where(syntaxItem => syntaxItem.ParameterSet.Intersect(machineState.ParameterSet).Any())
                                            .ToList();
            }

            token.SuggestedSyntaxItems = syntaxItems;
            token.IsComplete = false;
            machineState.CurrentState = MachineState.State.Item;
      
            return  new List<SemanticToken>() { token };
        }
    }
}
