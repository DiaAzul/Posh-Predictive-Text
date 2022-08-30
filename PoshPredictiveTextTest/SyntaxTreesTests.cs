﻿
using System.Management.Automation;
using Xunit;

namespace PoshPredictiveText.Test
{

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
        private SyntaxItem syntaxItem;

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


    // TODO [ ][TEST] Syntax Trees - How do we do this? Per Command?
    public class SyntaxTreesTests
    {
    }
}


// internal static class SyntaxTrees
//private static readonly Dictionary<string, List<SyntaxItem>> syntaxTrees = new();
//internal static bool Exists(string syntaxTreeName)
//internal static int Count(string syntaxTreeName)
//internal static List<SyntaxItem> Get(string syntaxTreeName)
//internal static List<string> UniqueCommands(string syntaxTreeName)
//internal static void Load(string syntaxTreeName)
//internal static string AsString(XElement? element)
//internal static string? AsNullableString(XElement? element)
//internal static bool AsBool(XElement? element, string trueValue = "TRUE")
//internal static bool? AsNullableBool(XElement? element, string trueValue = "TRUE")
//internal static string Tooltip(string syntaxTreeName, string? toolTipRef)

