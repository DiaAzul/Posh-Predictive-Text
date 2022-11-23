
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
        /// Add suggestions for token completion.
        /// 
        /// If there are no PARAMETER or COMMAND suggestions then consider that this may be a POSITIONAL value.
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

            var suggestions = machineState.SyntaxTree!.FilteredByCommandPath(machineState.CommandPath.ToString())
                                            .Where(syntaxItem => syntaxItemTypeFilter.Contains(syntaxItem.ItemType)
                                                                 && (syntaxItem.Name.StartsWith(token.Value)
                                                                   || syntaxItem.Name.StartsWith(token.Value)));

            // Filter by parameter sets.
            if (machineState.ParameterSet is not null)
            {
                suggestions = suggestions
                    .Where(syntaxItem => syntaxItem.ParameterSet.Intersect(machineState.ParameterSet).Any());
            }

            // Filter any entries that have already been entered by the maximum number of times they can be used.
            // Creates a dictionary of tokens on the command line with count for the number of times that they 
            // have been used, then filters the suggestions using that list against maximum times token can appear.
            if (machineState.SemanticTokens is not null)
            {
                Dictionary<string, int> tokensAlreadyEnteredWithCount = machineState.TokensAlreadyEnteredWithCount;
                suggestions = suggestions
                    .Where(syntaxItem =>
                        syntaxItem.MaxUses is null
                        || !(tokensAlreadyEnteredWithCount.GetValueOrDefault(syntaxItem.Name, 0) >= syntaxItem.MaxUses));
            }

            // If we don't match a command or parameter assume a positional value.
            List<SyntaxItem> positionalItems = suggestions
                                                        .Where(syntaxItem => syntaxItem.IsPositional)
                                                        .ToList();

            if (positionalItems.Count > 0)
            {
                token = EvaluatePositionalValue(token).First();
            }

            List<Suggestion> parameterSuggestions = suggestions
                                    .Where(syntaxItem => syntaxItem.IsParameter || syntaxItem.IsCommand)
                                    .Select(syntaxItem => new Suggestion
                                    {
                                        CompletionText = syntaxItem.Name,
                                        ListText = syntaxItem.Name,
                                        Type = CompletionResultType.Command,
                                        ToolTip = syntaxItem.ToolTip ?? ""
                                    }).ToList();

            if (token.Suggestions is not null && token.Suggestions.Count > 0)
            {
                token.Suggestions.AddRange(parameterSuggestions);
            }
            else
            {
                token.Suggestions = parameterSuggestions;
            }

            return  new List<SemanticToken>() { token };
        }
    }
}
