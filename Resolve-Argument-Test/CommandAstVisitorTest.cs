

namespace ResolveArgument.Tests
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
            ResolveArgument.Token testToken = new()
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
}



// internal class CommandAstVisitor : AstVisitor
//internal CommandAstVisitor()
//private int TokenPosition
//internal string? BaseCommand
//internal Token? LastToken
//internal Token? PriorToken
//internal Dictionary<int, Token> All
//internal int Count
//internal Dictionary<int, Token> CommandParameters
//internal Token? Index(int index)
//internal (string, int) CommandPath(List<string> uniqueCommands)
//internal bool CanUse(SyntaxItem syntaxItem)
//public override AstVisitAction DefaultVisit(Ast ast)
//public override AstVisitAction VisitCommand(CommandAst commandAst)
//public override AstVisitAction VisitCommandExpression(CommandExpressionAst commandExpressionAst)
//public override AstVisitAction VisitCommandParameter(CommandParameterAst commandParameterAst)
//public override AstVisitAction VisitStringConstantExpression(StringConstantExpressionAst stringConstantExpressionAst)

