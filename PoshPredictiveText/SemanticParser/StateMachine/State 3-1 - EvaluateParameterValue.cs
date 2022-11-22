
using PoshPredictiveText.SyntaxTrees;

namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    /// <summary>
    /// The state machine evaluates the command line input and appends
    /// semantic information to each token.
    /// 
    /// The semantic information is sourced from the syntax tree and used
    /// to determine the token type.
    /// </summary>
    internal partial class StateMachine
    {
        internal List<SemanticToken> EvaluateParameterValue(SemanticToken token)
        {
            machineState.LastParameterSyntaxItem(out SyntaxItem? syntaxItem, out int priorOccurances);

            if (syntaxItem is not null && (syntaxItem.MaxCount is null || priorOccurances < syntaxItem.MaxCount))
            {
                token.SemanticType = SemanticToken.TokenType.ParameterValue;
                token.ParameterValueName = syntaxItem.Value;

                token.Suggestions = SyntaxTreeHelpers
                                        .GetParamaterValues(
                                            command: machineState.SyntaxTreeName!,
                                            parameterName: token.ParameterValueName??"",
                                            wordToComplete: token.Value);

                if (token.Suggestions.Select(suggestion => suggestion.CompletionText).Contains(token.Value))
                {
                    token.IsExactMatch= true;
                };

                machineState.CurrentState = (priorOccurances + 1 >= syntaxItem.MaxCount) 
                                                ? MachineState.State.Item
                                                : MachineState.State.Value;
            }
            else
            {
                token.SemanticType = SemanticToken.TokenType.PositionalValue;
                machineState.CurrentState = MachineState.State.Value;
            }

            return new List<SemanticToken> { token };
        }
    }
}
