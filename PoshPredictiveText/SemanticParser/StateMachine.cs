
namespace PoshPredictiveText.SemanticParser
{
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Management.Automation.Language;

    /// <summary>
    /// The state machine evaluates the command line input and appends
    /// semantic information to each token.
    /// 
    /// The semantic information is sourced from the syntax tree and used
    /// to determine the token type.
    /// </summary>
    internal class StateMachine
    {
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
        /// State of the state machine.
        /// </summary>
        private State state = State.NoCommand;

        /// <summary>
        /// Name of the syntax tree once the base command is identified.
        /// </summary>
        private string? syntaxTreeName = null;

        /// <summary>
        /// Instance of syntax tree once the base command
        /// is identified.
        /// </summary>
        private SyntaxTree? syntaxTree = null;

        /// <summary>
        /// Parse mode for the command.
        /// </summary>
        private ParseMode parseMode = ParseMode.Windows;

        /// <summary>
        /// Command path at this point in the evaluation.
        /// </summary>
        private CommandPath commandPath = new();

        /// <summary>
        /// Number of parameter values still to be entered.
        /// </summary>
        private int parameterValues = 0;

        /// <summary>
        /// Syntax item for which values are being entered.
        /// </summary>
        private SyntaxItem? parameterSyntaxItem = null;

        /// <summary>
        /// StateMachine adds semantic information to command line tokens.
        /// 
        /// Initialise an empty state machine.
        /// </summary>
        internal StateMachine() { }

        /// <summary>
        /// StateMachine adds semantic information to command line tokens. 
        /// 
        /// Initialise a state machine with a given state (used in testing).
        /// </summary>
        /// <param name="state">State of the machine</param>
        /// <param name="syntaxTreeName">Name of syntax tree</param>
        /// <param name="syntaxTree">Syntax tree</param>
        /// <param name="parseMode">Parse mode (Windows, Posix, Python)</param>
        /// <param name="commandPath">Command Path</param>
        internal StateMachine(State state, string? syntaxTreeName, SyntaxTree? syntaxTree, ParseMode parseMode, CommandPath commandPath)
        {
            this.state=state;
            this.syntaxTreeName=syntaxTreeName;
            this.syntaxTree=syntaxTree;
            this.parseMode=parseMode;
            this.commandPath=commandPath;
        }

        /// <summary>
        /// Name of the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal string? SyntaxTreeName
        {
            get { return syntaxTreeName; }
        }

        /// <summary>
        /// Get the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal SyntaxTree? SyntaxTree
        {
            get { return syntaxTree; }
        }

        /// <summary>
        /// Get the Command Path for command tokens entered on the command line.
        /// </summary>
        internal CommandPath CommandPath
        {
            get { return commandPath; }
        }

        /// <summary>
        /// Get the current state of the state machine.
        /// </summary>
        internal State CurrentState
        {
            get { return state; }
        }

        /// <summary>
        /// Reset the state machine to the initial state.
        /// </summary>
        internal void Reset()
        {
            state = State.NoCommand;
            syntaxTreeName = null;
            syntaxTree = null;
            StateMachineItemCache.Reset();
        }

        // ******** STATE MACHINE ********
        /// <summary>
        /// Main entry point to the state machine.
        /// 
        /// The routing function evaluates tokens depending upon the state
        /// of the machine and the token value.
        /// 
        /// The machine may split tokens and return multiple semantically enhanced
        /// tokens for a given input token.
        /// </summary>
        /// <param name="tokens">Input tokens to which semantic information will be added</param>
        /// <returns>Tokens with semantic information added.</returns>
        internal List<Token> Evaluate(Token token)
        {
            LOGGER.Write($"STATE MACHINE: Evaluate {token.Value}.");
            List<Token> returnTokens;

            string cacheKey = commandPath + "+" + token.Value;
            bool doNotCache = syntaxTree is null;
            StateMachineState? stateMachineState = StateMachineItemCache.Get(cacheKey);

            // If we have already processed and cached this argument use the cached version.
            if (stateMachineState is not null)
            {
                LOGGER.Write("STATE MACHINE: Use cached token results.");
                state = stateMachineState.State;
                // Clone command path on way out of the cache to mitigate side-effects
                // which arise when additional items are added to the mutable list.
                commandPath = new(stateMachineState.CommandPath);
                parameterValues = stateMachineState.ParameterValues;
                parameterSyntaxItem = stateMachineState.ParameterSyntaxItem;
                returnTokens = stateMachineState.ReturnTokens;
            }
            // Otherwise parse the argument and add semantic information.
            else
            {
                LOGGER.Write("STATE MACHINE: Parse argument.");
                returnTokens = state switch
                {
                    State.NoCommand => NoCommand(token),
                    State.Item => EvaluateItem(token),
                    State.Value => EvaluateValue(token),
                    State.Inert => new List<Token> { token },
                    _ => new List<Token> { token },
                };

                // Cache the result if the argument is complete.
                if (!doNotCache && returnTokens.Count > 0 && returnTokens.First().IsComplete)
                {
                    switch (returnTokens.First().SemanticType)
                    {
                        // Do not cache parameter and positional values.
                        case Token.TokenType.ParameterValue:
                        case Token.TokenType.PositionalValue:
                            break;
                        default:
                            LOGGER.Write("STATE MACHINE: Cache parsed token results.");
                            StateMachineState newCacheItem = new()
                            {
                                State = state,
                                // Clone command path on way into the cache to mitigate side-effects
                                // which arise when additional items are added to the mutable list.
                                CommandPath = new(commandPath),
                                ParameterValues = parameterValues,
                                ParameterSyntaxItem = parameterSyntaxItem,
                                ReturnTokens = returnTokens
                            };

                            StateMachineItemCache.Add(cacheKey, newCacheItem);
                            break;
                    }
                }
            }
            LOGGER.Write($"STATE MACHINE: Complete {token.Value} evaluation.");
            return returnTokens;
        }

        // ******** STATE 0 ********
        internal List<Token> NoCommand(Token token)
        {
            string command = token.Value.ToLower();

            // If this is a supported command then load syntax tree.
            if (SyntaxTreesConfig.IsSupportedCommand(command))
            {
                LOGGER.Write($"STATE MACHINE: Supported command: {command}");
                syntaxTreeName = SyntaxTreesConfig.CommandFromAlias(command);
                LOGGER.Write($"STATE MACHINE: Syntax Tree Name: {syntaxTreeName}");
                try
                {
                    syntaxTree = SyntaxTrees.Tree(syntaxTreeName);
                    if (syntaxTree != null)
                    {
                        LOGGER.Write("STATE MACHINE: Loaded Syntax Tree.");
                    }
                    else
                    {
                        LOGGER.Write("STATE MACHINE: SYNTAX TREE NOT LOADED!");
                    }
                }
                catch (SyntaxTreeException)
                {
                    LOGGER.Write("STATE MACHINE: ERROR LOADING SYNTAX TREE!");
                }

                parseMode = SyntaxTreesConfig.ParseMode(syntaxTreeName);
                commandPath = new(SyntaxTreeName!);
                LOGGER.Write($"STATE MACHINE: Command path={commandPath}");
                token.SemanticType = Token.TokenType.Command;
                token.IsComplete = true;
                state = State.Item;
            }
            else
            {
                // See if this is a partial command and add suggested completion.
                List<string> suggestedCommands = SyntaxTreesConfig.SuggestedCommands(token.Value);
                if (suggestedCommands.Count == 0) return new List<Token> { token };

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
            return new List<Token> { token };
        }

        // ******** STATE 1 ********
        /// <summary>
        /// Evaluate an item to determine its semantic type.
        /// 
        /// The PowerShell abstract syntax tree identifies items
        /// matching the Windows understanding of command line
        /// arguments. It identifies redirection and parameters,
        /// but everything else is identified as a string constant.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        internal List<Token> EvaluateItem(Token token)
        {
            return token.SemanticType switch
            {
                Token.TokenType.Parameter => EvaluateParameter(token),
                Token.TokenType.Redirection => EvaluateRedirection(token),
                Token.TokenType.StringConstant => EvaluateStringConstant(token),
                _ => new List<Token> { token },
            };
        }

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
        internal List<Token> EvaluateParameter(Token token)
        {
            // POSIX single-hyphen has a complex set of rules.
            if (parseMode == ParseMode.Posix && !token.Value.StartsWith("--"))
                EvaluatePosixOption(token);

            string enteredParameter = token.Value.ToLower();

            List<SyntaxItem> parameters = syntaxTree!.ParametersAndOptions(commandPath.ToString());

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
                    state = State.Item;
                    break;
                case 1:
                    SyntaxItem parameter = suggestedParameters.First();

                    if (enteredParameter == parameter.Name || enteredParameter == parameter.Alias)
                    {
                        if (parameter.IsParameter)
                        {
                            // TODO [HIGH][STATEMACHINE] Calculate how many more parameter values can be entered.
                            parameterValues = parameter.MaxCount ?? -1;
                            parameterSyntaxItem = parameter;
                            state = State.Value;
                        }
                        else // IsOption
                        {
                            parameterValues = 0;
                            parameterSyntaxItem = null;
                            state = State.Item;
                        }
                        token.IsComplete = true;
                    }
                    else // Partial completion
                    {
                        token.SuggestedSyntaxItems = suggestedParameters;
                        token.IsComplete = false;
                        state = State.Item;
                    }

                    break;
                default:
                    token.SuggestedSyntaxItems = suggestedParameters;
                    token.IsComplete = false;
                    state = State.Item;
                    break;
            }

            return new List<Token> { token };
        }

        /// <summary>
        /// Redirection token may include the file name.
        /// The Ast will provide this in the next token, so
        /// we can remove the file name from this token.
        /// </summary>
        /// <param name="token">Token to enhance.</param>
        /// <returns>Enhanced token.</returns>
        internal List<Token> EvaluateRedirection(Token token)
        {
            // Remove any text after the symbols. Note the Ast extracts the path and provides
            // it as a value in the next token.
            string redirectSymbol = "";
            if (token.AstType == typeof(FileRedirectionAst)) redirectSymbol = ">";
            if (token.AstType == typeof(MergingRedirectionAst)) redirectSymbol = ">>";

            Token redirectionToken = new()
            {
                Value = redirectSymbol,
                AstType = token.AstType,
                LowerExtent = token.LowerExtent,
                UpperExtent = token.LowerExtent + redirectSymbol.Length - 1,
                SemanticType = Token.TokenType.Redirection,
                IsComplete = true,
            };

            parameterValues = 1;
            parameterSyntaxItem = new SyntaxItem() { Type = SyntaxItemType.REDIRECTION, Name="PATH" };
            state = State.Value;
            return new List<Token> { redirectionToken };
        }

        /// <summary>
        /// Evaluate string constant expression. This is most likely
        /// to evaluate to either a command or positional value.
        /// </summary>
        /// <param name="token">Token to evaluate.</param>
        /// <returns>Enhanced token.</returns>
        internal List<Token> EvaluateStringConstant(Token token)
        {
            string enteredValue = token.Value.ToLower();
            List<SyntaxItem> subCommands = syntaxTree!.SubCommands(commandPath.ToString());

            List<SyntaxItem> suggestedCommands = subCommands
                .Where(syntaxItem => syntaxItem.Name?.StartsWith(enteredValue) ?? false)
                .ToList();

            List<Token> resultTokens;
            switch (suggestedCommands.Count)
            {
                // If we don't recognise a command or partial command perhaps this is a positional value.
                case 0:
                    state = State.Value;
                    token.SemanticType = Token.TokenType.PositionalValue;
                    resultTokens = EvaluateValue(token);
                    break;
                // When there is one suggestion and it is an exact match.
                case 1 when enteredValue == suggestedCommands.First().Name:
                    commandPath.Add(enteredValue);
                    token.IsComplete = true;
                    token.SemanticType = Token.TokenType.Command;
                    state = State.Item;
                    resultTokens = new() { token };
                    break;
                // Otherwise add suggestions to response.
                default:
                    token.SuggestedSyntaxItems = suggestedCommands;
                    token.IsComplete = false;
                    state = State.Item;
                    resultTokens = new() { token };
                    break;
            }
            return resultTokens;
        }


        // ******** STATE 2 ********
        internal List<Token> EvaluatePosixOption(Token token)
        {
            // TODO [HIGH][STATEMACHINE] Implement evaluate posix options.
            return new List<Token> { token };
        }

        // ******** STATE 3 ********

        internal List<Token> EvaluateValue(Token token)
        {
            if (parameterSyntaxItem is not null && parameterValues != 0)
            {
                token.SemanticType = Token.TokenType.ParameterValue;
                token.ParameterValueName = parameterSyntaxItem.Value;
                if (parameterValues > 0) parameterValues--;

                switch (parameterValues)
                {
                    case 0:
                        {
                            parameterSyntaxItem = null;
                            state = State.Item;
                            break;
                        }
                    default:
                        {
                            state = State.Value;
                            break;
                        }
                }
            }
            else
            {
                token.SemanticType = Token.TokenType.PositionalValue;
                state = State.Value;
            }

            token.IsComplete = true;
            return new List<Token> { token };
        }
    }
}
