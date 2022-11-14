
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;

    internal partial class StateMachine
    {
        /// <summary>
        /// Process tokens when no command is identified.
        /// 
        /// If a command is identified then load the syntax tree and set the next state
        /// to expect a command item.
        /// 
        /// If a partial command is identified then return suggested command completions.
        /// </summary>
        /// <param name="token">Token parsed by Visitor</param>
        /// <returns>List of semantic tokens.</returns>
        /// <exception cref="SyntaxTreeException">Raised if the syntax tree cannot be identified or loaded.</exception>
        private List<SemanticToken> NoCommand(SemanticToken token)
        {
            string command = token.Value.ToLower();

            if (SyntaxTreesConfig.IsSupportedCommand(command))
            {
                ms.SyntaxTreeName = SyntaxTreesConfig.CommandFromAlias(command);

                LOGGER.Write($"STATE MACHINE: Supported command: {command}");
                LOGGER.Write($"STATE MACHINE: Syntax Tree Name: {ms.SyntaxTreeName}");
                try
                {
                    ms.SyntaxTree = SyntaxTrees.Tree(ms.SyntaxTreeName);
                }
#if DEBUG
                catch (SyntaxTreeException ex)
                {
                    LOGGER.Write("STATE MACHINE: ERROR LOADING SYNTAX TREE!");
                    throw new SyntaxTreeException("STATE MACHINE: Error loading syntax tree: ", ex);
                }
#else
                catch (SyntaxTreeException)
                {
                    LOGGER.Write("STATE MACHINE: ERROR LOADING SYNTAX TREE!");
                }
#endif

                ms.ParseMode = SyntaxTreesConfig.ParseMode(ms.SyntaxTreeName);
                ms.CommandPath = new(SyntaxTreeName!);
                token.SemanticType = SemanticToken.TokenType.Command;
                token.IsComplete = true;
                ms.CurrentState = MachineState.State.Item;

                LOGGER.Write("STATE MACHINE: Loaded Syntax Tree.");
                LOGGER.Write($"STATE MACHINE: Command path={ms.CommandPath}");

                return new List<SemanticToken> { token };
            }

            List<string> suggestedCommands = SyntaxTreesConfig.SuggestedCommands(token.Value);
            if (suggestedCommands.Count > 0)
            {
                List<SyntaxItem> suggestions = new();
                foreach (string suggestedCommand in suggestedCommands)
                {
                    SyntaxItem suggestion = new()
                    {
                        Command = suggestedCommand
                    };
                    suggestions.Add(suggestion);
                }
                token.SuggestedSyntaxItems = suggestions;
            }

            return new List<SemanticToken> { token };
        }
    }
}
