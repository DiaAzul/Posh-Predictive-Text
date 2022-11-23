using PoshPredictiveText.SyntaxTrees;

namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Management.Automation;

    internal partial class StateMachine
    {

        /// <summary>
        /// Evaluates positional item and returns suggested completions.
        /// </summary>
        /// <param name="token">Token parsed from the command line.</param>
        /// <returns>Suggested completions.</returns>
        internal List<SemanticToken> EvaluatePositionalValue(SemanticToken token)
        {
            // TODO [HIGH][STATEMACHINE] Test for parameter value when searching positional items.
            List<SyntaxItem> positionalSyntaxItems = machineState.SyntaxTree!
                                                    .FilteredByCommandPath(machineState.CommandPath.ToString())
                                                    .Where(syntaxItem => syntaxItem.IsPositional)
                                                    .ToList();

            Dictionary<SyntaxItem, int> priorTokens = machineState.PositionalTokensAlreadyEnteredWithCount;

            SyntaxItem? syntaxItem = null;

            LOGGER.Write($"POSITIONAL VALUE: Number of positional items available {positionalSyntaxItems.Count}.");
            switch (positionalSyntaxItems.Count)
{
                case 1:
                    syntaxItem= positionalSyntaxItems[0];
                    int? maxCount = syntaxItem.MaxCount;

                    // Create a local scope to contain the variable entered values (which otherwise clashes with
                    // the case > 1 instance of this variable.
                    {
                        if (maxCount is not null && priorTokens.TryGetValue(syntaxItem, out int enteredValues))
                        {
                            if (enteredValues > maxCount) goto default;
                        }
                    }
                    break;

                case > 1:
                    foreach (SyntaxItem positionalSyntaxItem in positionalSyntaxItems)
                    {
                        bool positionalSyntaxItemUsed = priorTokens.TryGetValue(positionalSyntaxItem, out int syntaxItemUses);
                        if (!positionalSyntaxItemUsed || positionalSyntaxItem.MaxCount is null || syntaxItemUses < positionalSyntaxItem.MaxCount)
                        {
                            syntaxItem = positionalSyntaxItem;
                            break;
                        }
                    }
                    goto default;

                default:
                    return new List<SemanticToken> { token };
            }

            if (syntaxItem is null) return new List<SemanticToken> { token };
            LOGGER.Write($"POSITIONAL VALUE: Positional item selectino {syntaxItem.Value}.");
            token.SyntaxItem = syntaxItem;

            switch (syntaxItem.Choices?.Count ?? 0)
            {
                case > 0:
                    
                    token.Suggestions = syntaxItem.GetChoices
                                .Where(choice => !choice.StartsWith("<") && choice.StartsWith(token.Value))
                                .Select(choice => new Suggestion
                                {
                                    CompletionText = choice,
                                    ListText = choice,
                                    Type = CompletionResultType.ParameterValue,
                                    ToolTip = syntaxItem.ToolTip ?? ""
                                })
                                .ToList();
                    LOGGER.Write($"POSITIONAL VALUE: Choice with {token.Suggestions.Count} suggestions.");
                    break;

               default:
                    token.Suggestions = SyntaxTreeHelpers
                                            .GetParamaterValues(
                                                command: machineState.SyntaxTreeName!,
                                                parameterName: syntaxItem!.Value??"",
                                                wordToComplete: token.Value);
                    LOGGER.Write($"POSITIONAL VALUE: Calculated with {token.Suggestions.Count} suggestions.");
                    break;
            }

            if (token.Suggestions.Select(suggestion => suggestion.CompletionText).Contains(token.Value))
            {
                token.IsExactMatch= true;
            };

            LOGGER.Write($"POSITIONAL VALUE: Is an exact match {token.IsExactMatch}.");

            return new List<SemanticToken> { token };
        }
    }
}
