
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Linq;

    internal partial class StateMachine
    {
        /// <summary>
        /// Evaluate Parameter token.
        /// If we are Posix mode and - then:
        /// - Split single letters.
        /// - Split single letter and values.
        /// 
        /// - Other modes: Do we have a complete syntaxItem
        /// - How many syntaxItem values?
        /// - Set state machine to expect value.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        internal List<SemanticToken> EvaluateParameter(SemanticToken token)
        {
            // POSIX single-hyphen has a complex set of rules.
            if (machineState.ParseMode == ParseMode.Posix && !token.Value.StartsWith("--"))
                return EvaluatePosixOption(token);

            string enteredParameter = token.Value.ToLower();

            List<SyntaxItem> parameters = machineState.SyntaxTree!.ParametersAndOptions(machineState.CommandPath.ToString());

            List<SyntaxItem> syntaxItems = parameters
                .Where(syntaxItem => (syntaxItem.Name?.StartsWith(enteredParameter) ?? false) ||
                                        (syntaxItem.Alias?.StartsWith(enteredParameter) ?? false))
                .ToList();

            // BUG [HIGH][STATEMACHINE] Mark token complete when an exact match entered. (But also provide suggestions).
            List<SyntaxItem> exactMatch = parameters
                .Where(syntaxItem => (syntaxItem.Name?.Contains(enteredParameter) ?? false) ||
                                        (syntaxItem.Alias?.Contains(enteredParameter)?? false))
                .ToList();

            // Issue - If we enter an alias then it may not show as complete if there is also a long form name.
            // Branch execution based upon the number of matching syntaxItems returned.
            // Zero syntax items implies no matches (perhaps mis-spelled entry on the CLI).
            // One syntax item implies a direct match and complete syntaxItem name.
            // More than one syntax item implies multiple suggested completions.
            switch (syntaxItems.Count)
            {
                case 0:
                    token.IsExactMatch = false;
                    machineState.CurrentState = MachineState.State.Item;
                    break;
                case 1:
                    SyntaxItem syntaxItem = syntaxItems.First();

                    if (!(enteredParameter == syntaxItem.Name || enteredParameter == syntaxItem.Alias)) goto default;

                    if (syntaxItem.IsParameter)
                    {
                        // TODO [HIGH][STATEMACHINE] Calculate how many more syntaxItem values can be entered.
                        machineState.ParameterValues = syntaxItem.MaxCount ?? -1;
                        machineState.ParameterSyntaxItem = syntaxItem;
                        machineState.CurrentState = MachineState.State.Value;
                        LOGGER.Write($"STATE MACHINE: Parameter {enteredParameter} complete. Sets - {String.Join(", ", syntaxItem.ParameterSet)}.");
                    }

                    if (syntaxItem.IsOptionParameter)
                    {
                        machineState.ParameterValues = 0;
                        machineState.ParameterSyntaxItem = null;
                        machineState.CurrentState = MachineState.State.Item;
                        LOGGER.Write($"STATE MACHINE: Optional {enteredParameter} complete. Sets - {String.Join(", ", syntaxItem.ParameterSet)}.");
                    }
                    token.SemanticType = SemanticToken.TokenType.Parameter;
                    token.ParameterSet = syntaxItem.ParameterSet;
                    token.IsExactMatch = true;
                    break;


                default:
                    token.SuggestedSyntaxItems = syntaxItems;
                    token.IsExactMatch = false;
                    machineState.CurrentState = MachineState.State.Item;
                    break;
            }

            return AddSuggestionsForTokenCompletion(token);
        }
    }
}
