
namespace PoshPredictiveText.Test
{
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Test the SyntaxItem records.
    /// 
    /// Perform basic testing of the syntaxItem record to
    /// ensure that it can be created and the various
    /// properties function as expected.
    /// 
    /// Risks:
    /// 1.[Low] Only one configuration is tested. This may not
    /// catch all issues that could arise.
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
                CommandPath = "conda.env",
                Type = "PRM",
                Argument = "--parameter",
                Alias = "-p",
                MultipleUse = false,
                Parameter = "ENVIRONMENT",
                MultipleParameterValues = false,
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