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
            List<SyntaxItem> positionalSyntaxItems = machineState.SyntaxTree!
                                                    .FilteredByCommandPath(machineState.CommandPath.ToString())
                                                    .Where(syntaxItem => syntaxItem.IsPositional)
                                                    .ToList();

            Dictionary<string, int> priorTokens = machineState.PositionalTokensAlreadyEnteredWithCount;

            List<SemanticToken> semanticTokens;

            switch (positionalSyntaxItems.Count)
            {
                case 1:
                    if (positionalSyntaxItems.First().MaxCount is not null && "NEED TO WORK OUT PRIOR POSITIONAL BY NAME" == "")
                    { 

                    }
                    // TODO [WIP][STATEMACHINE] One positional item - test number of uses.
                    goto case -1;

                case > 1:
                    // TODO [WIP][STATEMACHINE] Multile positional items - test which one we are using.
                    goto case -1;

                case -1: // Continuation case for cases 1 and > 1
                    // TODO [WIP][STATEMACHINE] Get suggestions for positional item.
                    semanticTokens = new List<SemanticToken> { token };
                    break;

                default:
                    semanticTokens = new List<SemanticToken> { token };
                    break;
            }

            return semanticTokens;
        }
    }
}