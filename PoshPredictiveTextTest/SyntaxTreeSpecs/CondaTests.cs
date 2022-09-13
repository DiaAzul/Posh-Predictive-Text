
namespace PoshPredictiveText.SyntaxTreeSpecs.Test
{
    using PoshPredictiveText.Test;
    using Xunit;

    /// <summary>
    /// Test conda syntax trees
    /// </summary>
    public class CondaTests
    {
        /// <summary>
        /// Basic test to return suggestions for conda
        /// 
        /// The inline data must have the following format:
        /// - inputString - text as entered on the command line, the final character
        /// must be a space if the previous word is complete.
        /// - expectedSuggestions - The number of expected suggestions.
        /// </summary>
        [Theory]
        [InlineData("conda ", 19)]
        [InlineData("conda env remove ", 9)]
        [InlineData("conda i", 3)]
        [InlineData("conda list --name --md5 ", 12)]
        [InlineData("conda activate -", 3)]

        public void CondaSuggestionsTest(string inputString, int expectedSuggestions)
        {
            // Arrange
            // WordToComplete. CommandAstVisitor. CursorPosition.
            string wordToComplete = ""; 
            if (inputString[inputString.Length - 1] != ' ')
                wordToComplete = inputString.Split(' ').ToList().Last();
                
            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            int cursorPosition = commandAst.Extent.EndOffset;

            // Act
            CommandAstVisitor enteredTokens = new();
            commandAst.Visit(enteredTokens);
            var suggestions = Resolver.Suggestions(wordToComplete, enteredTokens, cursorPosition);

            // Assert
            Assert.Equal(expectedSuggestions, suggestions.Count);
        }
    }
}
