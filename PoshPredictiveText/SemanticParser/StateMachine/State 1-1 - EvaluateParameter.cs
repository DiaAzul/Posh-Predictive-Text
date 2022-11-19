
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

            List<SyntaxItem> parameters = machineState.SyntaxTree!.ParametersAndOptions(machineState.CommandPath.ToString());
            List<SyntaxItem> exactMatch = parameters
                .Where(syntaxItem => (syntaxItem.Name?.Contains(token.Value, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                        (syntaxItem.Alias?.Contains(token.Value, StringComparison.OrdinalIgnoreCase)?? false))
                .ToList();

            if (exactMatch.Count > 0)
            {
                SyntaxItem syntaxItem = exactMatch.First();

                if (syntaxItem.IsParameter)
                {
                    machineState.ParameterValues = syntaxItem.MaxCount ?? -1;
                    machineState.ParameterSyntaxItem = syntaxItem;
                    machineState.CurrentState = MachineState.State.Value;
                    LOGGER.Write($"STATE MACHINE: Parameter {token.Value} complete. Sets - {String.Join(", ", syntaxItem.ParameterSet)}.");
                }

                if (syntaxItem.IsOptionParameter)
                {
                    machineState.ParameterValues = 0;
                    machineState.ParameterSyntaxItem = null;
                    machineState.CurrentState = MachineState.State.Item;
                    LOGGER.Write($"STATE MACHINE: Optional {token.Value} complete. Sets - {String.Join(", ", syntaxItem.ParameterSet)}.");
                }
                token.SemanticType = SemanticToken.TokenType.Parameter;
                token.ParameterSet = syntaxItem.ParameterSet;
                token.IsExactMatch = true;
            }

            return AddSuggestionsForTokenCompletion(token);
        }
    }
}
