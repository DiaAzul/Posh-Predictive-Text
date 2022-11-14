﻿
namespace PoshPredictiveText.Test.SyntaxTrees
{
    using PoshPredictiveText.SyntaxTrees;
    using Xunit;

    /// <summary>
    /// Syntax tree tests using a mock syntax tree.
    /// </summary>
    public class SyntaxTreesTests
    {
        // Name of the syntax tree used across all tests.
        const string syntaxTreeName = "Test";

        private readonly List<SyntaxItem> syntaxItems = new()
        {
            new SyntaxItem
            {
                Command  = "conda",
                Path = "conda",
                Name = "activate",
                Alias = null,
                MaxUses = 1,
                ParameterSets = new List<string>() {"1"},
                Value = null,
                MinCount = null,
                MaxCount = null,
                ItemType = SyntaxItemType.COMMAND,
                ToolTip = "TT0001"
            },
            new SyntaxItem {
                Command = "conda",
                Path = "conda" ,
                Name = "install",
                Alias = null,
                MaxUses = 1,
                ParameterSets = new List<string>() {"2"},
                Value  = null,
                MinCount = null,
                MaxCount = null,
                ItemType = SyntaxItemType.COMMAND,
                ToolTip = "TT0059"
            },
            new SyntaxItem {
                Command = "activate",
                Path = "conda.activate",
                Name = "--help",
                Alias = "-h",
                MaxUses = 1,
                ParameterSets = new List<string>() {"1"},
                Value = null,
                MinCount = 0,
                MaxCount = 0,
                ItemType = SyntaxItemType.PARAMETER,
                ToolTip = "TT0115"
            }
        };

        /// <summary>
        /// Create a mock SyntaxTree.
        /// </summary>
        public SyntaxTreesTests()
        {
            // Arrange
            // Need three records: Two same commands and one different.
            // The biggest test is UniqueCommands.

            if (SyntaxTrees.Exists(syntaxTreeName)) return;

            SyntaxTree syntaxTree = new(syntaxTreeName, syntaxItems);

            SyntaxTrees.Add(syntaxTree);
        }

        /// <summary>
        /// Test that the syntax tree exists.
        /// </summary>
        [Fact]
        public void SyntaxTreeExistsTest()
        {
            // Act
            bool exists = SyntaxTrees.Exists(syntaxTreeName);
            // Assert
            Assert.True(exists);
        }

        /// <summary>
        /// Test that a syntax tree can be removed from the database.
        /// </summary>
        [Fact]
        public void RemoveSyntaxTreeTest()
        {
            // Arrange
            Assert.True(SyntaxTrees.Exists(syntaxTreeName));
            // Act
            SyntaxTrees.Remove(syntaxTreeName);
            // Assert
            Assert.False(SyntaxTrees.Exists(syntaxTreeName));
        }

        /// <summary>
        /// Tests that a syntax tree is loaded when it is 
        /// first referenced.
        /// </summary>
        [Fact]
        public void LoadByFirstReferenceTests()
        {
            // Arrange - ensure syntax tree not already in database.
            if (SyntaxTrees.Exists("conda"))
            {
                SyntaxTrees.Remove("conda");
            }

            // Act
            var syntaxTree = SyntaxTrees.Tree("conda");

            Assert.NotNull(syntaxTree);
            Assert.Equal("conda", syntaxTree.Name);
        }

        /// <summary>
        /// Attempt to access a non-existent tree should throw an exception.
        /// </summary>
        [Fact]
        public void AccessNonExistentTreeTest()
        {
            // Arrange
            string nonExistentTree = "non-existent-tree";

            // Assert
            Assert.Throws<SyntaxTreeException>(() => SyntaxTrees.Tree(nonExistentTree));
        }

        /// <summary>
        /// Adding a tree with the same name should throw an error.
        /// </summary>
        [Fact]
        public void AttemptToAddAgainThrowsErrorTest()
        {
            // Arrange
            Assert.True(SyntaxTrees.Exists(syntaxTreeName));
            var duplicateTree = new SyntaxTree(syntaxTreeName, syntaxItems);

            // Act and Assert
            Assert.Throws<SyntaxTreeException>(() => SyntaxTrees.Add(duplicateTree));
        }

        /// <summary>
        /// Tests that the syntax tree is returned.
        /// </summary>
        [Fact]
        public void GetSyntaxTreeTest()
        {
            // Act & Assert
            var syntaxTree = SyntaxTrees.Tree(syntaxTreeName);
            Assert.NotNull(syntaxTree);
            Assert.IsType<SyntaxTree>(syntaxTree);

            int itemsInTree = syntaxTree.Count;
            Assert.Equal(3, itemsInTree);
        }
    }
}


