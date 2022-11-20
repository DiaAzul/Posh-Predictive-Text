﻿
namespace PoshPredictiveText.Test.StateMachine
{
    using PoshPredictiveText.SemanticParser;
    using PoshPredictiveText.SyntaxTrees;
    using System.Management.Automation.Language;
    using Xunit;
    using static PoshPredictiveText.SemanticParser.SemanticToken;

    /// <summary>
    /// Test Visitor records
    /// 
    /// Basic tests to create a token and test its value and properties.
    /// 
    /// Risks:
    /// 1. [Low] Only testing one type and limited properties.
    /// 2. [Low] Token does not constrain values or type, there is a
    /// possibilty of error if an unexpected type is assigned to token
    /// and other code relies on Token having known types.
    /// </summary>
    public class VisitorTokenTests
    {
        /// <summary>
        /// Test Token create, store and immutability.
        /// </summary>
        [Fact]
        public void CommandTokenTest()
        {
            // Arrange
            SemanticToken testToken = new()
            {
                Value = "TestValue",
                SemanticType = TokenType.Command
            };
            // Act
            bool isCommand = testToken.IsCommand;
            bool IsParameter = testToken.IsParameter;

            // Assert
            Assert.Equal("TestValue", testToken.Value);
            Assert.True(isCommand);
            Assert.False(IsParameter);
        }
    }

    /// <summary>
    /// Test Visitor
    /// 
    /// Class passed to the CommandAst.Visit() method. The visit method call 
    /// method defined within the CommandAstVisitor class for each token 
    /// parsed on the command line. Thevisitor class comprises two parts:
    /// 1. Methods called when the visitor is passed to CommandAst.Visit(), these
    /// method start with the verb 'Visit'.
    /// 2. Methods and properties used to query the parsed command line tokens.
    /// </summary>
    public class VisitorVisitTests
    {

        /// <summary>
        /// Test CommandAstVisitor and ability to parse tokens from text representing
        /// commands entered at the command prompt.
        /// 
        /// Test for specific cases:
        /// 1: CommandParameterAst using single - (-parameter)
        /// 2: CommandParameterAst using double - (--parameter)
        /// 3: Confirm that the number of tokens is as expected.
        /// 
        /// Risks:
        /// 1: [Low] A single test case will identify errors.
        /// </summary>
        [Fact]
        public void VisitTest()
        {
            // Arrange
            Visitor visitor = new();
            string promptText = "conda create --name py35 python=3.5";
            CommandAst ast = PowerShellMock.CreateCommandAst(promptText);

            // Act
            ast.Visit(visitor);
            var tokeniser = visitor.SemanticCLI;
            var tokens = tokeniser.All;

            // Assert
            Assert.Equal(5, tokens.Count);

            Assert.Equal("conda", tokens[0].Value);
            Assert.Equal(TokenType.Command, tokens[0].SemanticType);

            Assert.Equal("create", tokens[1].Value);
            Assert.Equal(TokenType.Command, tokens[0].SemanticType);

            Assert.Equal("--name", tokens[2].Value);
            Assert.Equal(TokenType.Parameter, tokens[2].SemanticType);

            Assert.Equal("py35", tokens[3].Value);
            Assert.Equal(TokenType.ParameterValue, tokens[3].SemanticType);

            Assert.Equal("python=3.5", tokens[4].Value);
            Assert.Equal(TokenType.PositionalValue, tokens[4].SemanticType);
        }
    }

    /// <summary>
    /// Test the retrieval of information from the CommandAstVisitor assuming
    /// it has sccessfully retrieved values from the prompt. 
    /// </summary>
    public class ValueTest
    {
        /// <summary>
        /// The tokenised input is created when the class is instantiated and 
        /// then used across all the tests in this class.
        /// </summary>
        private readonly SemanticCLI tokenisedInput = new();

        /// <summary>
        /// Initialise the Commandast visitor with the input string.
        /// </summary>
        public ValueTest()
        {
            // Arrange
            const string inputText = "conda env -parameter1 --parameter2 value1 12";
            CommandAst ast = PowerShellMock.CreateCommandAst(inputText);
            Visitor visitor = new();
            ast.Visit(visitor);
            tokenisedInput = visitor.SemanticCLI;
        }

        /// <summary>
        /// Test the BaseCommand method.
        /// </summary>
        [Fact]
        public void BaseCommandTest()
        {
            // Act
            string? baseCommand = tokenisedInput.BaseCommand;
            // Assert
            Assert.NotNull(baseCommand);
            Assert.Equal("conda", baseCommand);
        }

        /// <summary>
        /// Test the LastToken method.
        /// </summary>
        [Fact]
        public void LastTokenTest()
        {
            // Act
            SemanticParser.SemanticToken? token = tokenisedInput.LastToken;
            // Assert
            Assert.NotNull(token);
            Assert.Equal("12", token.Value);
        }

        /// <summary>
        /// Test the PriorToken method.
        /// </summary>
        [Fact]
        public void PriorTokenTest()
        {
            // Act
            SemanticParser.SemanticToken? token = tokenisedInput.PriorToken;
            // Assert
            Assert.NotNull(token);
            Assert.Equal("value1", token.Value);
        }

        /// <summary>
        /// Test the All tokens property.
        /// </summary>
        [Fact]
        public void AllTokensTest()
        {
            // This test is already covered in TestCommandAstVisitorVisitTest
            // So just do a quick test of count and one token.
            // Act
            List<SemanticToken> tokens = tokenisedInput.All;
            // Assert
            Assert.Equal(6, tokens.Count);
            Assert.Equal("12", tokens[5].Value);
            Assert.Equal(TokenType.PositionalValue, tokens[5].SemanticType);
        }

        /// <summary>
        /// Test the Count of tokens.
        /// </summary>
        [Fact]
        public void CountTest()
        {
            // Act
            int tokenCount = tokenisedInput.Count;
            // Assert
            Assert.Equal(6, tokenCount);
        }

        /// <summary>
        /// Test Index returns the correct token at a given index.
        /// Test for exceptions out of scope.
        /// </summary>
        [Fact]
        public void IndexTest()
        {
            // Act
            SemanticParser.SemanticToken? secondToken = tokenisedInput.Index(1);
            SemanticParser.SemanticToken? negativeIndex = tokenisedInput.Index(-1);
            SemanticParser.SemanticToken? outOfBounds = tokenisedInput.Index(10);
            // Assert
            Assert.NotNull(secondToken);
            Assert.Equal("env", secondToken.Value);
            Assert.Null(negativeIndex);
            Assert.Null(outOfBounds);
        }
    }
}







