

namespace PoshPredictiveText.Test
{
    using System.Management.Automation.Language;
    using Xunit;

    // TODO [ ][TEST] CommandAstVisitor

    /// <summary>
    /// Test CommandAstVisitor Token records
    /// 
    /// Basic tests to create a token and test its value and properties.
    /// 
    /// Risks:
    /// 1. [Low] Only testing one type and limited properties.
    /// 2. [Low] Token does not constrain values or type, there is a
    /// possibilty of error if an unexpected type is assigned to token
    /// and other code relies on Token having known types.
    /// </summary>
    public class CommandAstVisitorTokenTest
    {
        /// <summary>
        /// Test Token create, store and immutability.
        /// </summary>
        [Fact]
        public void CommandTokenTest()
        {
            // Arrange
            PoshPredictiveText.Token testToken = new()
            {
                Value = "TestValue",
                Type = typeof(CommandAst)
            };
            // Act
            bool isCommand = testToken.IsCommand;
            bool IsCommandExpression = testToken.IsCommandExpression;

            // Assert
            Assert.Equal("TestValue", testToken.Value);
            Assert.True(isCommand);
            Assert.False(IsCommandExpression);
        }
    }

    /// <summary>
    /// Test CommandAstVisitor
    /// 
    /// Class passed to the CommandAst.Visit() method. The visit method call 
    /// method defined within the CommandAstVisitor class for each token 
    /// parsed on the command line. Thevisitor class comprises two parts:
    /// 1. Methods called when the visitor is passed to CommandAst.Visit(), these
    /// method start with the verb 'Visit'.
    /// 2. Methods and properties used to query the parsed command line tokens.
    /// </summary>
    public class CommandAstVisitorVisitTest
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
        public void TestCommandAstVisitorVisitTest()
        {
            // Arrange
            CommandAstVisitor visitor = new CommandAstVisitor();
            string promptText = "conda env -parameter1 --parameter2 value1 12";
            CommandAst ast = AstHelper.CreateCommandAst(promptText);

            // Act
            ast.Visit(visitor);
            var tokens = visitor.All;

            // Assert
            Assert.Equal(6, tokens.Count);

            Assert.Equal("conda", tokens[0].Value);
            Assert.Equal(typeof(string), tokens[0].Type);

            Assert.Equal("env", tokens[1].Value);
            Assert.Equal(typeof(string), tokens[0].Type);

            Assert.Equal("-parameter1", tokens[2].Value);
            Assert.Equal(typeof(CommandParameterAst), tokens[2].Type);

            Assert.Equal("--parameter2", tokens[3].Value);
            Assert.Equal(typeof(CommandParameterAst), tokens[3].Type);

            Assert.Equal("value1", tokens[4].Value);
            Assert.Equal(typeof(string), tokens[4].Type);

            Assert.Equal("12", tokens[5].Value);
            Assert.Equal(typeof(string), tokens[5].Type);
        }
    }

    /// <summary>
    /// Test the retrieval of information from the CommandAstVisitor assuming
    /// it has sccessfully retrieved values from the prompt. 
    /// </summary>
    public class CommandAstVisitorValueTest
    {
        /// <summary>
        /// The tokenised input is created when the class is instantiated and 
        /// then used across all the tests in this class.
        /// </summary>
        private readonly CommandAstVisitor tokenisedInput = new();

        /// <summary>
        /// Initialise the Commandast visitor with the input string.
        /// </summary>
        public CommandAstVisitorValueTest()
        {
            // Arrange
            const string inputText = "conda env -parameter1 --parameter2 value1 12";
            CommandAst ast = AstHelper.CreateCommandAst(inputText);
            ast.Visit(tokenisedInput);
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
            PoshPredictiveText.Token? token = tokenisedInput.LastToken;
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
            PoshPredictiveText.Token? token = tokenisedInput.PriorToken;
            // Assert
            Assert.NotNull(token);
            Assert.Equal("value1", token.Value);
        }
    } 
}




//internal Dictionary<int, Token> All
//internal int Count
//internal Dictionary<int, Token> CommandParameters
//internal Token? Index(int index)
//internal (string, int) CommandPath(List<string> uniqueCommands)
//internal bool CanUse(SyntaxItem syntaxItem)


