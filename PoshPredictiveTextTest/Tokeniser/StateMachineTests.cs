

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
            stateMachine.Evaluate(commandToken);

            // Assert
            Assert.Equal(1, stateMachine.CommandPath.Count);
            Assert.Equal("conda", stateMachine.CommandPath.ToString());
            Assert.Equal("conda", stateMachine.SyntaxTreeName);
            Assert.NotNull(stateMachine.SyntaxTree);
        }

        /// <summary>
        /// Add a second command to the syntax tree.
        /// 
        /// Test that the second command is recognised as a command.
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
            stateMachine.Evaluate(commandToken);
            stateMachine.Evaluate(secondCommandToken);

            // Assert
            Assert.Equal(2, stateMachine.CommandPath.Count);
            Assert.Equal("conda.env", stateMachine.CommandPath.ToString());
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
