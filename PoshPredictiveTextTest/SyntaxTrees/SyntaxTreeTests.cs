
namespace PoshPredictiveText.Test.SyntaxTrees
{
    using PoshPredictiveText.SyntaxTrees;
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Syntax tree tests using a mock syntax tree.
    /// </summary>
    public class SyntaxTreeTests
    {
        // Name of the syntax tree used across all tests.
        const string syntaxTreeName = "Test";

        private static readonly List<SyntaxItem> syntaxItems = new()
        {
        new SyntaxItem
            {
                Command  = "conda",
                Path = "conda",
                ItemType = SyntaxItemType.COMMAND,
                Name = "activate",
                Alias = null,
                ParameterSet = new List<string>() {"1"},
                MaxUses = null,
                Value = null,
                MinCount = null,
                MaxCount = null,
                ToolTip = "TT0001"
            },
            new SyntaxItem {
                Command = "conda",
                Path = "conda" ,
                ItemType = SyntaxItemType.COMMAND,
                Name = "install",
                Alias = null,
                ParameterSet = new List<string>() { "2" },
                MaxUses = null,
                Value  = null,
                MinCount = null,
                MaxCount =null,

                ToolTip = "TT0059"
            },
            new SyntaxItem {
                Command = "activate",
                Path = "conda.activate",
                ItemType = SyntaxItemType.PARAMETER,
                Name = "--help",
                Alias = "-h",
                ParameterSet = new List<string>() { "2" },
                MaxUses = 1,
                Value = null,
                MinCount = 0,
                MaxCount = 0,
                ToolTip = "TT0115"
            }
        };

        readonly SyntaxTree syntaxTree = new(syntaxTreeName, syntaxItems);

        /// <summary>
        /// Test count of items in the syntax tree.
        /// </summary>
        [Fact]
        public void CountItemsInSyntaxTreeTest()
        {
            // Act
            int count = syntaxTree.Count;
            // Assert
            Assert.Equal(3, count);
        }

        /// <summary>
        /// Tests that the syntax tree is returned.
        /// </summary>
        [Fact]
        public void GetItemFromSyntaxTreeTest()
        {
            // Act
            SyntaxItem syntaxItem = syntaxTree.GetItems[2];
            // Assert
            Assert.Equal("TT0115", syntaxItem.ToolTip);
        }

        /// <summary>
        /// Test that we get the correct list of unique commands.
        /// </summary>
        [Fact]
        public void UniqueCommandsTest()
        {
            // Act & Assert
            var uniqueCommnads = syntaxTree.UniqueCommands;
            Assert.IsType<List<string>>(uniqueCommnads);
            Assert.Equal(2, uniqueCommnads.Count);
        }

        /// <summary>
        /// Test returns empty when null refernence passed to tooltip.
        /// </summary>
        [Fact]
        public void NullTooltipTest()
        {
            // Act
            var tooltip = syntaxTree.Tooltip(null);
            // Assert
            Assert.Empty(tooltip);
        }

        /// <summary>>
        /// Test returns empty when no tooltip definition file.
        /// </summary>
        [Fact]
        public void NoDefinitionFileTest()
        {
            // Act
            var tooltip = syntaxTree.Tooltip(syntaxTree.GetItems[2].ToolTip);
            // Assert
            Assert.Equal("", tooltip);
        }

        /// <summary>
        /// Test count of sub commands.
        /// </summary>
        [Fact]
        public void CountOfSubCommandsTest()
        {
            // Arrange
            // Act
            var countOfSubCommands = syntaxTree.CountOfSubCommands("conda");

            // Assert
            Assert.Equal(2, countOfSubCommands);
        }

        /// <summary>
        /// Test GetParametersAndOptions. Return parameters at a given
        /// command path location.
        /// </summary>
        [Fact]
        public void GetParametersAndOptions()
        {
            // Arrange
            string commandPath = "conda.activate";

            // Act
            var paramsAndOptions = syntaxTree.Parameters(commandPath);

            // Assert
            Assert.Single(paramsAndOptions);
        }

        /// <summary>
        /// Test GetParameterAndOptions when a parameter set is also
        /// defined. Returns parameter only if the parameter sets match.
        /// </summary>
        [Fact]
        public void GetParametersAndOptionsSet()
        {
            // Arrange
            List<string> parameterSetsMatch = new() { "2" };
            List<string> parameterSetsNoMatch = new() { "1" };
            string commandPath = "conda.activate";

            // Act
            var paramsAndOptions = syntaxTree.Parameters(commandPath, parameterSetsMatch);
            var paramsAndOptionsNo = syntaxTree.Parameters(commandPath, parameterSetsNoMatch);

            // Assert
            Assert.Single(paramsAndOptions);
            Assert.Empty(paramsAndOptionsNo);
        }
    }
}


