

namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using PoshPredictiveText.SemanticParser;

    /// <summary>
    /// Machine state information for command line arguments which have already been parsed.
    /// </summary>
    internal class MachineState
    {
        /// <summary>
        /// Name of the syntax tree.
        /// </summary>
        internal string? SyntaxTreeName = null;

        /// <summary>
        /// Syntax tree, loaded once the base command is identified.
        /// </summary>
        internal SyntaxTree? SyntaxTree = null;

        /// <summary>
        /// Parse mode for the command.
        /// </summary>
        internal ParseMode ParseMode = ParseMode.Windows;

        /// <summary>
        /// Permissible states within the state machine
        /// </summary>
        internal enum State
        {
            NoCommand, // No command has been identified.
            Item, // Process the token as command, option, parameter.
            Value, // Process the token as a value
            Inert, // No further processing required.
        }

        /// <summary>
        /// State of the statemachine after the argument was parsed.
        /// </summary>
        internal State CurrentState { get; set; } = State.NoCommand;

        /// <summary>
        /// Command path after the argument was parsed.
        /// </summary>
        internal CommandPath CommandPath { get; set; } = new();

        /// <summary>
        /// Number of parameter values to be entered after the argument if the parsed item
        /// was a parameter which expects values.
        /// </summary>
        internal int ParameterValues { get; set; } = 0;

        /// <summary>
        /// Parameter syntaxItem if the parsed item was a parameter expecting value arguments.
        /// </summary>
        internal SyntaxItem? ParameterSyntaxItem { get; set; } = null;

        /// <summary>
        /// Parameter sets that are in force for this command path.
        /// </summary>
        internal List<string>? ParameterSet { get; set; } = null!;

        /// <summary>
        /// The list of semantic tokens to be returned once input token parsed.
        /// </summary>
        internal List<SemanticToken> SemanticTokens { get; set; } = new();

        /// <summary>
        /// Hitory of semantic tokens added to the command line.
        /// </summary>
        internal List<SemanticToken> CLISemanticTokens { get; set; } = new()!;

        /// <summary>
        /// Return a clone with a deep copy of CommandPath and SemanticTokens
        /// if it is defined.
        /// 
        /// Do not deepcopy SyntaxTree as it is immutable.
        /// </summary>
        /// <returns>Clone of the machine state.</returns>
        internal MachineState DeepCopy()
        {
            MachineState newState = (MachineState)MemberwiseClone();
            newState.CommandPath = CommandPath.DeepCopy();
            newState.SemanticTokens= new(SemanticTokens);
            newState.CLISemanticTokens= new(CLISemanticTokens);

            return newState;
        }

        /// <summary>
        /// Return the last parameter name and the number of parameter values
        /// already entered.
        /// </summary>
        /// <param name="parameterName">Last parameter name.</param>
        /// <param name="priorOccurances">Number of parameter values already entered.</param>
        internal void LastParameterName(out string? parameterName, out int priorOccurances)
        {
            priorOccurances = 0;
            if (CLISemanticTokens.Count == 0)
            {
                parameterName = null;
                return;
            }

            parameterName = null;
            for (int index = CLISemanticTokens.Count - 1; index >=0; index--)
            {
                if (CLISemanticTokens[index].IsParameter)
                {
                    parameterName = CLISemanticTokens[index].Value;
                    break;
                }
                if (CLISemanticTokens[index].SemanticType != SemanticToken.TokenType.ParameterValue)
                {
                    break;
                }
                priorOccurances++;
            }
            return;
        }

        /// <summary>
        /// Get the prior occurances of a parameter on the command line.
        /// 
        /// If the command can be used multiple times on the command line, then iterate backwards to
        /// determine how many time the command has been used (at that point on the command path).
        /// The search terminates once a command is reached, implying that the command path changed
        /// at that point.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="priorOccurances"></param>
        /// <returns></returns>
        internal bool HasParameterAlreadyBeenUsed(in string parameterName, out int priorOccurances)
        {
            priorOccurances = 0;

            for (int index = CLISemanticTokens.Count - 1; index >=0; index--)
            {
                if (CLISemanticTokens[index].IsParameter && CLISemanticTokens[index].Value == parameterName)
                {
                    priorOccurances++;
                }
                if (CLISemanticTokens[index].SemanticType != SemanticToken.TokenType.Command)
                {
                    break;
                }
            }
            return priorOccurances > 0;
        }

        /// <summary>
        /// Get a list of parameter and positional value tokens already entered on the command line with count
        /// of coccurances.
        /// </summary>
        internal Dictionary<string, int> TokensAlreadyEnteredWithCount => CLISemanticTokens
                                                      .Where(semanticToken => semanticToken.IsExactMatch
                                                                                && (semanticToken.IsCommand
                                                                || semanticToken.IsParameter
                                                                || semanticToken.IsPositionalValue))
                                                      .GroupBy(semanticToken => semanticToken.Value)
                                                      .Select(countItem => new
                                                      {
                                                          Token = countItem.Key,
                                                          Count = countItem.Count(),
                                                      })
                                                      .ToDictionary(a => a.Token, a => a.Count);


        /// <summary>
        /// Get a list of positional value tokens already entered on the command line with count
        /// of coccurances.
        /// </summary>
        internal Dictionary<string, int> PositionalTokensAlreadyEnteredWithCount => CLISemanticTokens
                                                      .Where(semanticToken => semanticToken.IsExactMatch
                                                                              && semanticToken.IsPositionalValue)
                                                      .GroupBy(semanticToken => semanticToken.Value)
                                                      .Select(countItem => new
                                                      {
                                                          Token = countItem.Key,
                                                          Count = countItem.Count(),
                                                      })
                                                      .ToDictionary(a => a.Token, a => a.Count);
    }
}
