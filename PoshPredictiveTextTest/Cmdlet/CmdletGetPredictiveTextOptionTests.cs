
namespace PoshPredictiveText.Test.Cmdlets
{
    using PoshPredictiveText;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Test for the cmdlet.
    /// </summary>
    public class CmdletGetPredictiveTextOptionTests
    {
        /// <summary>
        /// Return list of commands when calling cmdlet with -list parameter.ge
        /// </summary>
        [Fact]
        public void ListOfSupportedCommandsTest()
        {
            // Arrange
            using var powerShell = PowerShellMock.GetConfiguredShell();
            powerShell.AddCommand("Get-PredictiveTextOption");
            powerShell.AddParameter("list");
            var expectedResult = UIStrings.LIST_COMMANDS;
            var expectedCommands = SyntaxTreesConfig.SupportedCommands();
            // Act
            Collection<PSObject> results = powerShell.Invoke();
            // Assert
            Assert.Single(results);

            List<string> lines = new();
            using (var reader = new StringReader(results.First().ToString()))
            {
                string? line;
                while ((line = reader.ReadLine()) is not null)
                    lines.Add(line);
            }

            Assert.Equal(2, lines.Count);
            Assert.Equal(expectedResult, lines[0]);
            Assert.Equal(expectedCommands, lines[1]);
        }

        /// <summary>
        /// Return the initialisation script when called with the -printscript parameter.
        /// 
        /// The stored script is modified when printed to remove the placeholder
        /// $cmdNames with actual commands handled by the text completer. Therefore,
        /// Test the first line of the script only.
        /// </summary>
        [Fact]
        public void PrintScriptTest()
        {
            // Arrange
            using var powerShell = PowerShellMock.GetConfiguredShell();
            powerShell.AddCommand("Get-PredictiveTextOption");
            powerShell.AddParameter("printscript");
            var expectedResult = CommonTestTools.GetFirstLine(UIStrings.REGISTER_COMMAND_SCRIPT)
                                            .Replace("$cmdNames", SyntaxTreesConfig.SupportedCommands());
            // Act
            Collection<PSObject> results = powerShell.Invoke();
            // Assert
            Assert.True(results.Count == 1);
            var firstLineOfResponse = CommonTestTools.GetFirstLine(results.First().ToString());
            Assert.Equal(firstLineOfResponse, expectedResult);
        }
    }
}
