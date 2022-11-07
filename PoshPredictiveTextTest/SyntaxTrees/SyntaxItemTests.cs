
namespace PoshPredictiveText.Test.SyntaxTrees
{
    using System.Management.Automation;
    using PoshPredictiveText;
    using PoshPredictiveText.SyntaxTrees;
    using Xunit;

    /// <summary>
    /// Test the Syntax Tree.
    /// </summary>
    public class SyntaxItemTests
    {
        /// <summary>
        /// Syntax item to use across all tests in this class.
        /// </summary>
        private readonly SyntaxItem syntaxItem;

        /// <summary>
        /// Generate a syntax item to use across all tests.
        /// </summary>
        public SyntaxItemTests()
        {
            // Arrange
            syntaxItem = new SyntaxItem()
            {
                Command = "env",
                Path = "conda.env",
                Type = SyntaxItemType.PARAMETER,
                Name = "--parameter",
                Alias = "-p",
                Sets = new List<string> { "1" },
                MaxUses = 1,
                Value = "ENVIRONMENT",
                MinCount = 1,
                MaxCount = 1,
                ToolTip = "TT0001"
            };
        }

        /// <summary>
        /// Test the properties of a SyntaxItem.
        /// </summary>
        [Fact]
        public void SyntaxItemTest()
        {
            // Act
            CompletionResultType resultsType = syntaxItem.ResultType;

            // Assert
            Assert.NotNull(syntaxItem);

            Assert.False(syntaxItem.IsCommand);
            Assert.True(syntaxItem.IsParameter);
            Assert.False(syntaxItem.IsOptionParameter);
            Assert.False(syntaxItem.IsPositionalParameter);
            Assert.True(syntaxItem.HasAlias);

            Assert.Equal(CompletionResultType.ParameterName, resultsType);
        }
    }
}