
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;

    internal partial class StateMachine
    {
        /// <summary>
        /// Evaluate Parameter token.
        /// If we are Posix mode and - then:
        /// - Split single letters.
        /// - Split single letter and values.
        /// 
        /// - Other modes: Do we have a complete parameter
        /// - How many parameter values?
        /// - Set state machine to expect value.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        internal List<SemanticToken> EvaluateParameter(SemanticToken token)
        {
            // POSIX single-hyphen has a complex set of rules.
            if (ms.ParseMode == ParseMode.Posix && !token.Value.StartsWith("--"))
                EvaluatePosixOption(token);

            string enteredParameter = token.Value.ToLower();

            List<SyntaxItem> parameters = ms.SyntaxTree!.ParametersAndOptions(this.ms.CommandPath.ToString());

            List<SyntaxItem> suggestedParameters = parameters
                .Where(syntaxItem => (syntaxItem.Name?.StartsWith(enteredParameter) ?? false) ||
                                        (syntaxItem.Alias?.StartsWith(enteredParameter) ?? false))
                .ToList();

            // Issue - If we enter an alias then it may not show as complete if there is also a long form name.
            switch (suggestedParameters.Count)
            {
                case 0:
                    // If we don't identify a valid parameter then just return the token.
                    // User may have mis-spelled parameter name.
                    token.IsComplete = false;
                    this.ms.CurrentState = MachineState.State.Item;
                    break;
                case 1:
                    SyntaxItem parameter = suggestedParameters.First();

                    if (enteredParameter == parameter.Name || enteredParameter == parameter.Alias)
                    {
                        if (parameter.IsParameter)
                        {
                            // TODO [HIGH][STATEMACHINE] Calculate how many more parameter values can be entered.
                            this.ms.ParameterValues = parameter.MaxCount ?? -1;
                            this.ms.ParameterSyntaxItem = parameter;
                            this.ms.CurrentState = MachineState.State.Value;
                        }
                        else // IsOption
                        {
                            this.ms.ParameterValues = 0;
                            this.ms.ParameterSyntaxItem = null;
                            this.ms.CurrentState = MachineState.State.Item;
                        }
                        token.IsComplete = true;
                    }
                    else // Partial completion
                    {
                        token.SuggestedSyntaxItems = suggestedParameters;
                        token.IsComplete = false;
                        this.ms.CurrentState = MachineState.State.Item;
                    }

                    break;
                default:
                    token.SuggestedSyntaxItems = suggestedParameters;
                    token.IsComplete = false;
                    this.ms.CurrentState = MachineState.State.Item;
                    break;
            }

            return new List<SemanticToken> { token };
        }
    }
}
