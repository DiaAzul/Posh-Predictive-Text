
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
        // listItemText: syntaxItem.Name,
        // resultType: syntaxItem.ResultType,
        // toolTip: syntaxItem.ToolTip) ?? ""

        internal List<SemanticToken> EvaluateParameterValue(SemanticToken token)
        {
            if (machineState.ParameterSyntaxItem is not null && machineState.ParameterValues != 0)
            {
                token.SemanticType = SemanticToken.TokenType.ParameterValue;
                token.ParameterValueName = machineState.ParameterSyntaxItem.Value;
                if (machineState.ParameterValues > 0) machineState.ParameterValues--;

                switch (machineState.ParameterValues)
                {
                    case 0:
                        {
                            machineState.ParameterSyntaxItem = null;
                            machineState.CurrentState = MachineState.State.Item;
                            break;
                        }
                    default:
                        {
                            token.Suggestions = SyntaxTreeHelpers
                                                    .GetParamaterValues(token.ParameterValueName??"", token.Value);
                        break;
                        }
                }
            }
            else
            {
                token.SemanticType = SemanticToken.TokenType.PositionalValue;
                machineState.CurrentState = MachineState.State.Value;
            }

            token.IsExactMatch = true;
            token.ParameterSet = null;
            return new List<SemanticToken> { token };
        }
    }
}
