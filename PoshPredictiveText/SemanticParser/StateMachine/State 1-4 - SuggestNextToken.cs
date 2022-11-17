
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
        internal List<SemanticToken> AddSuggestionsForTokenCompletion(SemanticToken token,  List<SyntaxItemType>? syntaxItemTypefilter = null)

        {
            List<SyntaxItemType> syntaxItemTypeFilter = syntaxItemTypefilter ??
                                            new List<SyntaxItemType>() { SyntaxItemType.PARAMETER, SyntaxItemType.COMMAND };

            var suggestedSyntaxItems = machineState.SyntaxTree!.FilteredByCommandPath(machineState.CommandPath.ToString())
                                            .Where(syntaxItem => syntaxItemTypeFilter.Contains(syntaxItem.ItemType)
                                                                 && syntaxItem.Name.StartsWith(token.Value));

            // Filter by parameter sets.
            if (machineState.ParameterSet is not null)
            {
                suggestedSyntaxItems = suggestedSyntaxItems
                    .Where(syntaxItem => syntaxItem.ParameterSet.Intersect(machineState.ParameterSet).Any());
            }

            // Filter any entries that have already been entered by the maximum number of times they can be used.
            // Creates a dictionary of tokens on the command line with count for the number of times that they 
            // have been used, then filters the suggestions using that list against maximum times token can appear.
            if (machineState.SemanticTokens is not null)
            {
                Dictionary<string, int> enteredTokensWithCounts = machineState.CLISemanticTokens
                                              .Where(semanticToken => semanticToken.IsExactMatch
                                                                        && (semanticToken.IsCommand
                                                        || semanticToken.IsParameter
                                                        || semanticToken.IsPositionalValue))
                                              .GroupBy(semanticToken => semanticToken.Value)
                                              .Select(countItem => new
                                              {
                                                  Token = countItem.Key,
                                                  Count = countItem.Count(),
                                              })
                                              .ToDictionary(a => a.Token, a => a.Count);

                suggestedSyntaxItems = suggestedSyntaxItems
                    .Where(syntaxItem =>
                        syntaxItem.MaxUses is null
                        || !(enteredTokensWithCounts.ContainsKey(syntaxItem.Name)
                            && enteredTokensWithCounts.GetValueOrDefault(syntaxItem.Name) >= syntaxItem.MaxUses));
            }

            token.SuggestedSyntaxItems = suggestedSyntaxItems.ToList();
      
            return  new List<SemanticToken>() { token };
        }
    }
}
