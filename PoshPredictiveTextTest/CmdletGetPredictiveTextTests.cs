

namespace PoshPredictiveText.Test
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
        [Fact]
        public void CondaSuggestionsTest()
        {
            using var powerShell = PowerShellMock.GetConfiguredShell();
            var commandAst = PowerShellMock.CreateCommandAst("conda");
            powerShell.AddCommand("Get-PredictiveText")
                .AddParameter("WordToComplete", "")
                .AddParameter("CommandAst", commandAst)
                .AddParameter("CursorPosition", commandAst.Extent.EndColumnNumber);

            var results = powerShell.Invoke();

            Assert.False(powerShell.HadErrors);
            Assert.Single(results);
            Assert.IsType<List<CompletionResult>>(results[0].BaseObject);
            var baseObject = (List<CompletionResult>)results[0].BaseObject;
            Assert.Equal(19, baseObject.Count);
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
