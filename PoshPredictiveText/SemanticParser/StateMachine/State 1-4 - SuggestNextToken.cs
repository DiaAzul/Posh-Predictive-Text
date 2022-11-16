
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
        ///// <summary>
        ///// Evaluate string constant expression. This is most likely
        ///// to evaluate to either a command or positional value.
        ///// </summary>
        ///// <param name="token">Token to evaluate.</param>
        ///// <returns>Enhanced token.</returns>
        ///// 


        /// <summary>
        /// Add suggested next items to the token.
        /// </summary>
        /// <param name="token">Token to which next suggestions will be added.</param>
        /// <param name="syntaxItemTypefilter">Optional type filter comprising a list of SyntaxItemTypes which are used
        /// to filter syntax items. The list of types are included within the suggestions. The default is to include
        /// PARAMETER and COMMAND, and exclude POSITIONAL items.</param>
        /// <returns>List of semantic tokens with suggestions included in the first token.</returns>
        internal List<SemanticToken> SuggestNextToken(SemanticToken token,  List<SyntaxItemType>? syntaxItemTypefilter = null)

        {
            List<SyntaxItemType> syntaxItemTypeFilter = syntaxItemTypefilter ??
                                            new List<SyntaxItemType>() { SyntaxItemType.PARAMETER, SyntaxItemType.COMMAND };

            var syntaxItems = machineState.SyntaxTree!.FilteredByCommandPath(machineState.CommandPath.ToString())
                                            .Where(syntaxItem => syntaxItemTypeFilter.Contains(syntaxItem.ItemType));
            if (machineState.ParameterSet is not null)
            {
                syntaxItems = syntaxItems.Where(syntaxItem => syntaxItem.ParameterSet.Intersect(machineState.ParameterSet).Any());
            }

            token.SuggestedSyntaxItems = syntaxItems.ToList();
            machineState.CurrentState = MachineState.State.Item;
      
            return  new List<SemanticToken>() { token };
        }
    }
}
