
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
            if (this.ms.ParameterSyntaxItem is not null && this.ms.ParameterValues != 0)
            {
                token.SemanticType = SemanticToken.TokenType.ParameterValue;
                token.ParameterValueName = this.ms.ParameterSyntaxItem.Value;
                if (this.ms.ParameterValues > 0) this.ms.ParameterValues--;

                switch (this.ms.ParameterValues)
                {
                    case 0:
                        {
                            this.ms.ParameterSyntaxItem = null;
                            this.ms.CurrentState = MachineState.State.Item;
                            break;
                        }
                    default:
                        {
                            this.ms.CurrentState = MachineState.State.Value;
                            break;
                        }
                }
            }
            else
            {
                token.SemanticType = SemanticToken.TokenType.PositionalValue;
                this.ms.CurrentState = MachineState.State.Value;
            }

            token.IsComplete = true;
            return new List<SemanticToken> { token };
        }
    }
}
