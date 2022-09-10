
namespace PoshPredictiveText.Test
{
    using Xunit;

    /// <summary>
    /// Test conda syntax trees
    /// </summary>
    public class Conda
    {
        /// <summary>
        /// Basic test to return suggestions for conda
        /// </summary>
        [Fact]
        public void CondaSuggestionsTest()
        {
            // Arrange
            // WordToComplete. CommandAstVisitor. CursorPosition.
            string wordToComplete = "";
            var commandAst = AstHelper.CreateCommandAst("conda");
            int cursorPosition = commandAst.Extent.EndOffset;

            // Act
            CommandAstVisitor enteredTokens = new();
            commandAst.Visit(enteredTokens);
            var suggestions = Resolver.Suggestions(wordToComplete, enteredTokens, cursorPosition);

            // Assert
            Assert.Equal(19, suggestions.Count);
        }
    }
}
