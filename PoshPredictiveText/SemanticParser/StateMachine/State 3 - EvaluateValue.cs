
namespace PoshPredictiveText.SemanticParser
{

    /// <summary>
    /// The state machine evaluates the command line input and appends
    /// semantic information to each token.
    /// 
    /// The semantic information is sourced from the syntax tree and used
    /// to determine the token type.
    /// </summary>
    internal partial class StateMachine
    {
        internal List<SemanticToken> EvaluateValue(SemanticToken token)
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
                            break;
                        }
                }
            }
            else
            {
                token.SemanticType = SemanticToken.TokenType.PositionalValue;
                machineState.CurrentState = MachineState.State.Value;
            }

            token.IsComplete = true;
            token.ParameterSet = null;
            return new List<SemanticToken> { token };
        }
    }
}
