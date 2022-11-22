
namespace PoshPredictiveText.Test.SyntaxTrees
{
    using PoshPredictiveText.SyntaxTrees;
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Test the Syntax Tree.
    /// </summary>
    public class SyntaxItemTests
    {
        /// <summary>
        /// Test the properties of a Parameter SyntaxItem.
        /// </summary>
        [Fact]
        public void ParameterSyntaxItem()
        {
            // Arrange
            SyntaxItem syntaxItem = new()
            {
                Command = "env",
                Path = "conda.env",
                ItemType = SyntaxItemType.PARAMETER,
                Name = "--parameter",
                Alias = "-p",
                ParameterSet = new List<string> { "1" },
                MaxUses = 1,
                Value = "ENVIRONMENT",
                MinCount = 1,
                MaxCount = 1,
                ToolTip = "TT0001"
            };

            // Act
            CompletionResultType resultsType = syntaxItem.ResultType;

            // Assert
            Assert.NotNull(syntaxItem);

            Assert.False(syntaxItem.IsCommand);
            Assert.True(syntaxItem.IsParameter);
            Assert.False(syntaxItem.IsPositional);
            Assert.True(syntaxItem.HasAlias);

            Assert.Equal(CompletionResultType.ParameterName, resultsType);
        }

        /// <summary>
        /// Test the properties of a Position SyntaxItem.
        /// </summary>
        [Fact]
        public void PositionalSyntaxItem()
        {
            // Arrange
            SyntaxItem syntaxItem = new()
            {
                Command = "install",
                Path = "conda.install",
                ItemType = SyntaxItemType.POSITIONAL,
                Name = "*1",
                ParameterSet = new List<string> { "1" },
                MaxUses = null,
                Value = "PACKAGE",
                MinCount = null,
                MaxCount = null,
                ToolTip = "TT0001"
            };

            // Act
            CompletionResultType resultsType = syntaxItem.ResultType;

            // Assert
            Assert.NotNull(syntaxItem);
            Assert.True(syntaxItem.IsPositional);
            Assert.False(syntaxItem.HasAlias);
            Assert.Equal(1, syntaxItem.PositionalIndex);

            Assert.Equal(CompletionResultType.ParameterValue, resultsType);
        }

        /// <summary>
        /// Test the that an unparseable positional index returns zero.
        /// </summary>
        [Fact]
        public void PositionalUnparseableIndex()
        {
            // Arrange
            SyntaxItem syntaxItem = new()
            {
                Command = "install",
                Path = "conda.install",
                ItemType = SyntaxItemType.POSITIONAL,
                Name = "*Unparseable",
                ParameterSet = new List<string> { "1" },
                MaxUses = null,
                Value = "PACKAGE",
                MinCount = null,
                MaxCount = null,
                ToolTip = "TT0001"
            };

            // Act

            // Assert
            Assert.Null(syntaxItem.PositionalIndex);
        }
    }
}