
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
        /// Name of the syntax tree.
        /// </summary>
        private string? syntaxTreeName = null;

        /// <summary>
        /// Syntax tree, loaded once the base command is identified.
        /// </summary>
        private SyntaxTree? syntaxTree = null;

        /// <summary>
        /// Parse mode for the command.
        /// </summary>
        private ParseMode parseMode = ParseMode.Windows;

        /// <summary>
        /// Machine state. Held as a cacheable object.
        /// </summary>
        private MachineState ms = new();

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
            this.syntaxTreeName=syntaxTreeName;
            this.syntaxTree=syntaxTree;
            this.parseMode=parseMode;
            this.ms.State = state;
            this.ms.CommandPath=commandPath;
        }

        /// <summary>
        /// Name of the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal string? SyntaxTreeName => this.syntaxTreeName;

        /// <summary>
        /// Get the SyntaxTree used to tokenise the command line.
        /// </summary>
        internal SyntaxTree? SyntaxTree => this.syntaxTree;

        /// <summary>
        /// Get the Command Path for command tokens entered on the command line.
        /// </summary>
        internal CommandPath CommandPath => this.ms.CommandPath;

        /// <summary>
        /// Get the current state of the state machine.
        /// </summary>
        internal State CurrentState => this.ms.State;

        /// <summary>
        /// Get the Parameter sets available for this command path
        /// at this point in the command line.
        /// </summary>
        internal List<string>? ParameterSets => this.ms.ParameterSets;

        /// <summary>
        /// Reset the state machine to the initial state.
        /// </summary>
        internal void Reset()
        {
            this.ms = new();
            this.syntaxTreeName = null;
            this.syntaxTree = null;
            MachineStateCache.Reset();
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
        internal List<SemanticToken> Evaluate(SemanticToken token)
        {
            LOGGER.Write($"STATE MACHINE: Evaluate {token.Value}.");

            List<SemanticToken> returnTokens;
            string cacheKey = this.ms.CommandPath + "+" + token.Value;

            // If the result is already in the cache return early.
            if (MachineStateCache.TryGetValue(cacheKey, out MachineState cachedMachineState))
            {
                LOGGER.Write("STATE MACHINE: Use cached token results.");
                ms = cachedMachineState;
                return this.ms.ReturnTokens ?? new List<SemanticToken>();
            }

            // Otherwise parse the argument and add semantic information.
            LOGGER.Write("STATE MACHINE: Parse argument.");
            returnTokens = this.ms.State switch
            {
                State.NoCommand => NoCommand(token),
                State.Item => EvaluateItem(token),
                State.Value => EvaluateValue(token),
                State.Inert => new List<SemanticToken> { token },
                _ => new List<SemanticToken> { token },
            };

            // TODO [HIGH][STATEMACHINE] Update Parameter sets.
            if (returnTokens.Count > 0 && returnTokens.First().IsComplete)
            {
                foreach (var returnToken in returnTokens)
                {
                    if (returnToken.IsCommand || returnToken.IsParameter || returnToken.IsPositionalValue)
                    {

                    }
                }
            }

            // Cache the result if the argument is complete.
            if ((syntaxTree is not null) && returnTokens.Count > 0 && returnTokens.First().IsComplete)
            {
                switch (returnTokens.First().SemanticType)
                {
                    // Do not cache parameter and positional values.
                    case SemanticToken.TokenType.ParameterValue:
                    case SemanticToken.TokenType.PositionalValue:
                        break;
                    default:
                        LOGGER.Write("STATE MACHINE: Cache parsed token results.");
                        ms.ReturnTokens = returnTokens;
                        MachineStateCache.Add(cacheKey, ms);
                        break;
                }
            }

            LOGGER.Write($"STATE MACHINE: Complete {token.Value} evaluation.");
            return returnTokens;
        }

        // ******** END EVALUATE ***

        // ******** STATE 0 ********
        private List<SemanticToken> NoCommand(SemanticToken token)
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
                this.ms.CommandPath = new(SyntaxTreeName!);
                LOGGER.Write($"STATE MACHINE: Command path={this.ms.CommandPath}");
                token.SemanticType = SemanticToken.TokenType.Command;
                token.IsComplete = true;
                this.ms.State = State.Item;
            }
            else
            {
                // See if this is a partial command and add suggested completion.
                List<string> suggestedCommands = SyntaxTreesConfig.SuggestedCommands(token.Value);
                if (suggestedCommands.Count == 0) return new List<SemanticToken> { token };

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
        internal List<SemanticToken> EvaluateItem(SemanticToken token)
        {
            return token.SemanticType switch
            {
                SemanticToken.TokenType.Parameter => EvaluateParameter(token),
                SemanticToken.TokenType.Redirection => EvaluateRedirection(token),
                SemanticToken.TokenType.StringConstant => EvaluateStringConstant(token),
                _ => new List<SemanticToken> { token },
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
        internal List<SemanticToken> EvaluateParameter(SemanticToken token)
        {
            // POSIX single-hyphen has a complex set of rules.
            if (parseMode == ParseMode.Posix && !token.Value.StartsWith("--"))
                EvaluatePosixOption(token);

            string enteredParameter = token.Value.ToLower();

            List<SyntaxItem> parameters = syntaxTree!.ParametersAndOptions(this.ms.CommandPath.ToString());

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
                    this.ms.State = State.Item;
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
                            this.ms.State = State.Value;
                        }
                        else // IsOption
                        {
                            this.ms.ParameterValues = 0;
                            this.ms.ParameterSyntaxItem = null;
                            this.ms.State = State.Item;
                        }
                        token.IsComplete = true;
                    }
                    else // Partial completion
                    {
                        token.SuggestedSyntaxItems = suggestedParameters;
                        token.IsComplete = false;
                        this.ms.State = State.Item;
                    }

                    break;
                default:
                    token.SuggestedSyntaxItems = suggestedParameters;
                    token.IsComplete = false;
                    this.ms.State = State.Item;
                    break;
            }

            return new List<SemanticToken> { token };
        }

        /// <summary>
        /// Redirection token may include the file name.
        /// The Ast will provide this in the next token, so
        /// we can remove the file name from this token.
        /// </summary>
        /// <param name="token">Token to enhance.</param>
        /// <returns>Enhanced token.</returns>
        internal List<SemanticToken> EvaluateRedirection(SemanticToken token)
        {
            // Remove any text after the symbols. Note the Ast extracts the path and provides
            // it as a value in the next token.
            string redirectSymbol = "";
            if (token.AstType == typeof(FileRedirectionAst)) redirectSymbol = ">";
            if (token.AstType == typeof(MergingRedirectionAst)) redirectSymbol = ">>";

            SemanticToken redirectionToken = new()
            {
                Value = redirectSymbol,
                AstType = token.AstType,
                LowerExtent = token.LowerExtent,
                UpperExtent = token.LowerExtent + redirectSymbol.Length - 1,
                SemanticType = SemanticToken.TokenType.Redirection,
                IsComplete = true,
            };

            // NOTE [LOW][STATEMACHINE] Redirection assumed to path only, not between streams (&3 > &1)
            // See: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_redirection
            this.ms.ParameterValues = 1;
            this.ms.ParameterSyntaxItem = new SyntaxItem() { ItemType = SyntaxItemType.REDIRECTION, Name="PATH" };
            this.ms.State = State.Value;
            return new List<SemanticToken> { redirectionToken };
        }

        /// <summary>
        /// Evaluate string constant expression. This is most likely
        /// to evaluate to either a command or positional value.
        /// </summary>
        /// <param name="token">Token to evaluate.</param>
        /// <returns>Enhanced token.</returns>
        internal List<SemanticToken> EvaluateStringConstant(SemanticToken token)
        {
            string enteredValue = token.Value.ToLower();
            List<SyntaxItem> subCommands = syntaxTree!.SubCommands(this.ms.CommandPath.ToString());

            List<SyntaxItem> suggestedCommands = subCommands
                .Where(syntaxItem => syntaxItem.Name?.StartsWith(enteredValue) ?? false)
                .ToList();

            List<SemanticToken> resultTokens;
            switch (suggestedCommands.Count)
            {
                // If we don't recognise a command or partial command perhaps this is a positional value.
                case 0:
                    this.ms.State = State.Value;
                    token.SemanticType = SemanticToken.TokenType.PositionalValue;
                    resultTokens = EvaluateValue(token);
                    break;
                // When there is one suggestion and it is an exact match.
                case 1 when enteredValue == suggestedCommands.First().Name:
                    this.ms.CommandPath.Add(enteredValue);
                    token.IsComplete = true;
                    token.SemanticType = SemanticToken.TokenType.Command;
                    this.ms.State = State.Item;
                    resultTokens = new() { token };
                    break;
                // Otherwise add suggestions to response.
                default:
                    token.SuggestedSyntaxItems = suggestedCommands;
                    token.IsComplete = false;
                    this.ms.State = State.Item;
                    resultTokens = new() { token };
                    break;
            }
            return resultTokens;
        }


        // ******** STATE 2 ********
        internal List<SemanticToken> EvaluatePosixOption(SemanticToken token)
        {
            // TODO [HIGH][STATEMACHINE] Implement evaluate posix options.
            return new List<SemanticToken> { token };
        }

        // ******** STATE 3 ********

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
                            this.ms.State = State.Item;
                            break;
                        }
                    default:
                        {
                            this.ms.State = State.Value;
                            break;
                        }
                }
            }
            else
            {
                token.SemanticType = SemanticToken.TokenType.PositionalValue;
                this.ms.State = State.Value;
            }

            token.IsComplete = true;
            return new List<SemanticToken> { token };
        }
    }
}
