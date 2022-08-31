
namespace PoshPredictiveText.Test
{
    using System.Management.Automation;
    using System.Xml.Linq;
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

    /// <summary>
    /// Syntax tree tests using a mock syntax tree.
    /// </summary>
    public class SyntaxTreesTests
    {
        // Name of the syntax tree used across all tests.
        const string syntaxTreeName = "Test";

        /// <summary>
        /// Create a mock SyntaxTree.
        /// </summary>
        public SyntaxTreesTests()
        {
            // Arrange
            // Need three records: Two same commands and one different.
            // The biggest test is UniqueCommands.
            if (!SyntaxTrees.Exists(syntaxTreeName))
            {
                List<SyntaxItem> syntaxTree = new()
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
                SyntaxTrees.Add(syntaxTreeName, syntaxTree);
            };
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
        /// Tet cound of items in the syntax tree.
        /// </summary>
        [Fact]
        public void CountItemsInSyntaxTreeTest()
        {
            // Act
            int count = SyntaxTrees.Count(syntaxTreeName);
            // Assert
            Assert.Equal(3, count);
        }

        /// <summary>
        /// Tests that the syntax tree is returned.
        /// </summary>
        [Fact]
        public void GetSyntaxTreeTest()
        {
            // Act & Assert
            var syntaxTree = SyntaxTrees.Get(syntaxTreeName);
            Assert.IsType<List<SyntaxItem>>(syntaxTree);

            int itemsInTree = syntaxTree.Count;          
            Assert.Equal(3, itemsInTree);

            SyntaxItem syntaxItem = syntaxTree[2];
            Assert.Equal("TT0115", syntaxItem.ToolTip);
        }

        /// <summary>
        /// Test that we get the correct list of unique commands.
        /// </summary>
        [Fact]
        public void UniqueCommandsTest()
        {
            // Act & Assert
            var uniqueCommnads = SyntaxTrees.UniqueCommands(syntaxTreeName);
            Assert.IsType<List<string>>(uniqueCommnads);
            Assert.Equal(2, uniqueCommnads.Count);
        }
    }
}


