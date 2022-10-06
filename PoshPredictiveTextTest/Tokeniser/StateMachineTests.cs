

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
                SemanticType = Token.TokenType.Parameter,
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
