

namespace PoshPredictiveText.StateMachine.Test
{
    using PoshPredictiveText;
    using PoshPredictiveText.Test;
    using Xunit;
    using static PoshPredictiveText.Token;

    /// <summary>
    /// StateMachine tests.
    /// </summary>
    public class StateMachineTests
    {
        /// <summary>
        /// Add a single command to the syntax tree.
        /// 
        /// => Added semantic information.
        /// => IsComplete, IsCommand.
        /// => Added syntaxTreeName and syntaxTree.
        /// </summary>
        [Fact]
        public void AddCommand()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "conda";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = commandToAdd.Length,
                SemanticType = TokenType.StringConstant,
            };

            // Act
            var result = stateMachine.Evaluate(commandToken);

            // Assert
            Assert.Single(result);
            Assert.True(result.First().IsComplete);
            Assert.True(result.First().IsCommand);
            Assert.Equal(1, stateMachine.CommandPath.Count);
            Assert.Equal("conda", stateMachine.CommandPath.ToString());
            Assert.Equal("conda", stateMachine.SyntaxTreeName);
            Assert.NotNull(stateMachine.SyntaxTree);
            Assert.Equal(StateMachine.State.Item, stateMachine.CurrentState);
        }

        /// <summary>
        /// Add a partial base command to the syntax tree.
        /// 
        /// Test result => Suggested base command.
        /// </summary>
        [Fact]
        public void AddPartialBaseCommand()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "con";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = commandToAdd.Length,
                SemanticType = TokenType.StringConstant,
            };

            // Act
            var result = stateMachine.Evaluate(commandToken);

            // Assert
            Assert.Single(result);
            Assert.False(result.First().IsComplete);
            Assert.False(result.First().IsCommand);
            var suggestedItems = result.First().SuggestedSyntaxItems;
            Assert.NotNull(suggestedItems);
            Assert.Single(suggestedItems);
            Assert.Equal("conda", suggestedItems.First().Command);
            Assert.Equal(0, stateMachine.CommandPath.Count);
            Assert.Equal("", stateMachine.CommandPath.ToString());
            Assert.Null(stateMachine.SyntaxTreeName);
            Assert.Null(stateMachine.SyntaxTree);
            Assert.Equal(StateMachine.State.NoCommand, stateMachine.CurrentState);
        }

        /// <summary>
        /// Add a second command to the syntax tree.
        /// 
        /// Test that the second command is recognised as a command.
        /// 
        /// </summary>
        [Fact]
        public void AddTwoCommands()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "conda";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = commandToAdd.Length,
                SemanticType = TokenType.StringConstant,
            };
            string secondCommandToAdd = "env";
            Token secondCommandToken = new()
            {
                Value = secondCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = commandToAdd.Length + 1,
                UpperExtent = commandToAdd.Length + 1 + secondCommandToAdd.Length,
                SemanticType = TokenType.StringConstant,
            };

            // Act
            var result1 = stateMachine.Evaluate(commandToken);
            var result2 = stateMachine.Evaluate(secondCommandToken);

            // Assert
            Assert.Single(result1);
            Assert.True(result1.First().IsComplete);
            Assert.True(result1.First().IsCommand);
            Assert.Equal("conda", result1.First().Value);
            Assert.Single(result2);
            Assert.True(result2.First().IsComplete);
            Assert.True(result2.First().IsCommand);
            Assert.Equal("env", result2.First().Value);
            Assert.Equal("conda.env", stateMachine.CommandPath.ToString());
            Assert.Equal(2, stateMachine.CommandPath.Count);
            Assert.Equal("conda", stateMachine.SyntaxTreeName);
            Assert.NotNull(stateMachine.SyntaxTree);
        }

        /// <summary>
        /// Add a second partial command to the syntax tree.
        /// 
        /// Test for suggestions.
        /// </summary>
        [Fact]
        public void AddPartialCommand()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "conda";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = commandToAdd.Length,
                SemanticType = TokenType.StringConstant,
            };
            string secondCommandToAdd = "i";
            Token secondCommandToken = new()
            {
                Value = secondCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = commandToAdd.Length + 1,
                UpperExtent = commandToAdd.Length + 1 + secondCommandToAdd.Length,
                SemanticType = TokenType.StringConstant,
            };

            // Act
            var result1 = stateMachine.Evaluate(commandToken);
            var result2 = stateMachine.Evaluate(secondCommandToken);

            // Assert
            Assert.Single(result1);
            Assert.True(result1.First().IsComplete);
            Assert.True(result1.First().IsCommand);
            Assert.Equal(1, stateMachine.CommandPath.Count);
            Assert.Equal("conda", stateMachine.CommandPath.ToString());
            Assert.Equal("conda", stateMachine.SyntaxTreeName);
            Assert.NotNull(stateMachine.SyntaxTree);

            Assert.Single(result2);
            Assert.False(result2.First().IsComplete);
            var syntaxItems = result2.First().SuggestedSyntaxItems;
            Assert.NotNull(syntaxItems);
            Assert.Equal(3, syntaxItems.Count);
        }


        /// <summary>
        /// Test that a option parameter is identified
        /// and doesn't expect a value after.
        /// 
        /// Test string => conda --help
        /// 
        /// Expect:
        /// CurrentState => Item after --help
        /// 
        /// </summary>
        [Fact]
        public void AddParameterOption()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "conda";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = 5,
                SemanticType = TokenType.StringConstant,
            };
            string thirdCommandToAdd = "--help";
            Token thirdCommandToken = new()
            {
                Value = thirdCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.CommandParameterAst),
                LowerExtent = 14,
                UpperExtent = 19,
                SemanticType = TokenType.Parameter,
            };

            // Act
            var result1 = stateMachine.Evaluate(commandToken);
            var result3 = stateMachine.Evaluate(thirdCommandToken);
            var stateAfterParameter = stateMachine.CurrentState;

            // Assert
            Assert.Single(result3);
            Assert.True(result3.First().IsParameter);
            Assert.True(result3.First().IsComplete);
            Assert.Equal(TokenType.Parameter, result3.First().SemanticType);
            Assert.Equal(StateMachine.State.Item, stateAfterParameter);
        }


        /// <summary>
        /// Test that a partial parameter is identified and that
        /// appropriate suggestions are made.
        /// 
        /// Test string => conda --
        /// 
        /// Expect:
        /// Two suggestions --help, --version
        /// 
        /// </summary>
        [Fact]
        public void AddPartialParameterOption()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "conda";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = 5,
                SemanticType = TokenType.StringConstant,
            };
            string secondCommandToAdd = "--";
            Token secondCommandToken = new()
            {
                Value = secondCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.CommandParameterAst),
                LowerExtent = 14,
                UpperExtent = 19,
                SemanticType = TokenType.Parameter,
            };

            // Act
            var result1 = stateMachine.Evaluate(commandToken);
            var result2 = stateMachine.Evaluate(secondCommandToken);
            var stateAfterParameter = stateMachine.CurrentState;

            // Assert
            Assert.Single(result2);
            var firstItem = result2.First();
            Assert.True(firstItem.IsParameter);
            Assert.False(firstItem.IsComplete);
            Assert.Equal(TokenType.Parameter, firstItem.SemanticType);
            Assert.Equal(StateMachine.State.Item, stateAfterParameter);
            Assert.NotNull(firstItem.SuggestedSyntaxItems);
            List<SyntaxItem> syntaxItems = firstItem.SuggestedSyntaxItems;
            Assert.Equal(2, syntaxItems.Count);
            List<string> suggestions = new();
            foreach (SyntaxItem item in syntaxItems)
            {
                suggestions.Add(item.Argument!);
            }
            Assert.Contains("--help", suggestions);
            Assert.Contains("--version", suggestions);
        }

        /// <summary>
        /// Test that a parameter expects a value and then
        /// expects more parameters after the parameter value
        /// entered.
        /// 
        /// Test string => conda create --name pyEnv
        /// 
        /// Expect:
        /// CurrentState => Value after --name
        /// CurrentState => Item after pyEnv
        /// 
        /// </summary>
        [Fact]
        public void AddParameterExpectingValue()
        {
            // Arrange
            StateMachine stateMachine = new();
            string commandToAdd = "conda";
            Token commandToken = new()
            {
                Value = commandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 1,
                UpperExtent = 5,
                SemanticType = TokenType.StringConstant,
            };
            string secondCommandToAdd = "create";
            Token secondCommandToken = new()
            {
                Value = secondCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 7,
                UpperExtent = 12,
                SemanticType = TokenType.StringConstant,
            };
            string thirdCommandToAdd = "--name";
            Token thirdCommandToken = new()
            {
                Value = thirdCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.CommandParameterAst),
                LowerExtent = 14,
                UpperExtent = 19,
                SemanticType = TokenType.Parameter,
            };
            string fourthCommandToAdd = "pyEnv";
            Token fourthCommandToken = new()
            {
                Value = fourthCommandToAdd,
                AstType = typeof(System.Management.Automation.Language.StringConstantExpressionAst),
                LowerExtent = 21,
                UpperExtent = 25,
                SemanticType = TokenType.StringConstant,
            };

            // Act
            var result1 = stateMachine.Evaluate(commandToken);
            var result2 = stateMachine.Evaluate(secondCommandToken);
            var result3 = stateMachine.Evaluate(thirdCommandToken);
            var stateAfterParameter = stateMachine.CurrentState;
            var result4 = stateMachine.Evaluate(fourthCommandToken);
            var stateAfterParameterValue = stateMachine.CurrentState;

            // Assert
            Assert.Single(result3);
            Assert.True(result3.First().IsParameter);
            Assert.True(result3.First().IsComplete);
            Assert.Equal(TokenType.Parameter, result3.First().SemanticType);
            Assert.Equal(StateMachine.State.Value, stateAfterParameter);
            Assert.Single(result4);
            Assert.True(result4.First().IsComplete);
            Assert.Equal(TokenType.ParameterValue, result4.First().SemanticType);
            Assert.Equal("ENVIRONMENT", result4.First().ParameterValueName);
            Assert.Equal(StateMachine.State.Item, stateAfterParameterValue);
        }

        /// <summary>
        /// Test resolution of parameter tokens.
        /// </summary>
        [Fact]
        public void EvaluateParameterTest()
        {

            // Arrange
            StateMachine stateMachine = new(
                StateMachine.State.Item,
                "conda",
                new SyntaxTree("conda"),
                SyntaxTreeSpecs.ParseMode.Posix,
                new CommandPath("conda")
                );

            Token token = new()
            {
                Value = "--",
                AstType = typeof(System.Management.Automation.Language.CommandParameterAst),
                LowerExtent = 1,
                UpperExtent = 6,
                SemanticType = TokenType.Parameter,
            };
            // Act
            List<Token> result = stateMachine.EvaluateParameter(token);

            // Assert
            Assert.Single(result);
            Token resultToken = result.First();
            Assert.NotNull(resultToken.SuggestedSyntaxItems);
            Assert.Equal(2, resultToken.SuggestedSyntaxItems.Count);
        }

        /// <summary>
        /// Test State Machine by passing input string.
        /// </summary>
        [Fact]
        public void InputString1Test()
        {
            // Arrange
            string inputString = "conda create --name py35 python=3.5";
            List<string> tokenValues = new()
            {
                "conda",
                "create",
                "--name",
                "py35",
                "python=3.5"
            };
            List<TokenType> tokenTypes = new()
            {
                TokenType.Command,
                TokenType.Command,
                TokenType.Parameter,
                TokenType.ParameterValue,
                TokenType.PositionalValue
            };

            // Act
            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            Visitor visitor = new();
            commandAst.Visit(visitor);
            Tokeniser enteredTokens = visitor.Tokeniser;
            var returnedTokens = enteredTokens.All;

            // Assert
            Assert.Equal(tokenValues.Count, returnedTokens.Count);
            foreach (int i in Enumerable.Range(0, tokenValues.Count))
            {
                Assert.Equal(tokenValues[i], returnedTokens[i].Value);
                Assert.Equal(tokenTypes[i], returnedTokens[i].SemanticType);
            }
        }


        /// <summary>
        /// Test State Machine by passing input string.
        /// </summary>
        [Fact]
        public void InputString2Test()
        {
            // Arrange
            string inputString = "conda list --explicit > bio-env.txt";
            List<string> tokenValues = new()
            {
                "conda",
                "list",
                "--explicit",
                ">",
                "bio-env.txt"
            };
            List<TokenType> tokenTypes = new()
            {
                TokenType.Command,
                TokenType.Command,
                TokenType.Parameter,
                TokenType.Redirection,
                TokenType.ParameterValue
            };

            // Act
            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            Visitor visitor = new();
            commandAst.Visit(visitor);
            Tokeniser enteredTokens = visitor.Tokeniser;
            var returnedTokens = enteredTokens.All;

            // Assert
            Assert.Equal(tokenValues.Count, returnedTokens.Count);
            foreach (int i in Enumerable.Range(0, tokenValues.Count))
            {
                Assert.Equal(tokenValues[i], returnedTokens[i].Value);
                Assert.Equal(tokenTypes[i], returnedTokens[i].SemanticType);
            }
        }
    }
}
