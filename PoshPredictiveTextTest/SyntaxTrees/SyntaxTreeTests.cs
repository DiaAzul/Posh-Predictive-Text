﻿
namespace PoshPredictiveText.Test
{
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
                CommandPath = "conda",
                Argument = "activate",
                Alias = null,
                MultipleUse = false,
                Parameter = null,
                MultipleParameterValues = null,
                Type = "CMD",
                ToolTip = "TT0001"
            },
            new SyntaxItem {
                Command = "conda",
                CommandPath = "conda" ,
                Argument = "install",
                Alias = null,
                MultipleUse = false,
                Parameter  = null,
                MultipleParameterValues = null,
                Type = "CMD",
                ToolTip = "TT0059"
            },
            new SyntaxItem {
                Command = "activate",
                CommandPath = "conda.activate",
                Argument = "--help",
                Alias = "-h",
                MultipleUse = false,
                Parameter = null,
                MultipleParameterValues = null,
                Type = "OPT",
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
            Assert.Empty(tooltip);
        }
    }
}


