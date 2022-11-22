

namespace PoshPredictiveText.Test.Cmdlets
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Get suggestions from predictive text.
    /// </summary>
    public class CmdletGetPredictiveTextTests
    {
        /// <summary>
        /// Get predictive text suggestions using the cmdlet.
        /// </summary>
        [Theory]
        [InlineData("conda ", 19)]
        [InlineData("conda env remove ", 10)]
        [InlineData("conda i", 3)]
        [InlineData("conda list --export --md5 ", 11)]
        [InlineData("conda activate -", 3)]
        public void CondaSuggestionsTest(string inputString, int expectedSuggestions)
        {
            // Arrange
            string wordToComplete = "";
            if (inputString[^1] != ' ')
                wordToComplete = inputString.Split(' ').ToList().Last();

            using var powerShell = PowerShellMock.GetConfiguredShell();
            var commandAst = PowerShellMock.CreateCommandAst(inputString);

            // Act
            powerShell.AddCommand("Get-PredictiveText")
                .AddParameter("WordToComplete", wordToComplete)
                .AddParameter("CommandAst", commandAst)
                .AddParameter("CursorPosition", commandAst.Extent.EndColumnNumber);

            var results = powerShell.Invoke();

            // Assert
            Assert.False(powerShell.HadErrors);
            Assert.Single(results);
            Assert.IsType<List<CompletionResult>>(results[0].BaseObject);
            var baseObject = (List<CompletionResult>)results[0].BaseObject;
            Assert.Equal(expectedSuggestions, baseObject.Count);
        }

        /// <summary>
        /// Test returns nothing if the command is not known.
        /// </summary>
        [Fact]
        public void NoSuggestionForUnknownCommandTest()
        {
            using var powerShell = PowerShellMock.GetConfiguredShell();
            var commandAst = PowerShellMock.CreateCommandAst("notknown");
            powerShell.AddCommand("Get-PredictiveText")
                .AddParameter("WordToComplete", "")
                .AddParameter("CommandAst", commandAst)
                .AddParameter("CursorPosition", commandAst.Extent.EndColumnNumber);

            var results = powerShell.Invoke();

            Assert.False(powerShell.HadErrors);
            Assert.Single(results);
            Assert.IsType<List<CompletionResult>>(results[0].BaseObject);
            var baseObject = (List<CompletionResult>)results[0].BaseObject;
            Assert.Empty(baseObject);
        }
    }
}
