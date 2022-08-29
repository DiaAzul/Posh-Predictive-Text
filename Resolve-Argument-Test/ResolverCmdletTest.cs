

namespace ResolveArgument.Tests
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using Xunit;

    /// <summary>
    /// Create PowerShell shells for testing.
    /// </summary>
    public static class PSTestHelpers
    {
        /// <summary>
        /// Configures an instance of PowerShell containing the cmdlet to test.
        /// </summary>
        /// <returns>
        /// Instance of PowerShell containing cmdlet.
        /// </returns>
        public static PowerShell GetConfiguredShell()
        {
            var sessionState = InitialSessionState.CreateDefault();

            // Add cmdlet to the shell instance.
            SessionStateCmdletEntry cmdletToTest = new("resolve-argument", typeof(ResolverCmdlet), null);
            sessionState.Commands.Add(cmdletToTest);

            // Create an instance of the shell.
            var testShellInstance = PowerShell.Create(sessionState);
            return testShellInstance;
        }

        /// <summary>
        /// Returns the first line of a multi-line string.
        /// </summary>
        /// <param name="multiLineString">Multi-line string.</param>
        /// <returns>First line of multi-line string.</returns>
        public static string GetFirstLine(string multiLineString)
        {
            string firstLine = string.Empty;
            using (var reader = new StringReader(multiLineString))
            {
                string? line = reader.ReadLine();
                if (line == null) throw new ArgumentException("Empty string in test.");
                firstLine = line;
            }
            return firstLine;
        }
    }

    /// <summary>
    /// Test for the cmdlet.
    /// </summary>
    public class ResolveArgumentCmdletTests
    {
        /// <summary>
        /// Test parameters when calling the cmdlet.
        /// 
        /// -list : list of supported command.
        /// -initialise : Return initialisation script.
        /// -printscript : Print the initialisation script.
        /// -help : Return help text
        /// </summary>
        public class WhenCalledWithValidParameters
        {
            /// <summary>
            /// Test the get first line from multi-line string function.
            /// </summary>
            [Fact]
            public void TestFirstLineFromMultiLineString()
            {
                // Arrange
                string multiLineString = "First line\n\rSecond Line";
                string firstLine = "First line";
                // Act
                var result = PSTestHelpers.GetFirstLine(multiLineString);
                // Assert
                Assert.Equal(firstLine, result);
            }

            /// <summary>
            /// Confirm that an empty string thows an error in fuction to GetFirstLine
            /// of a multi-line string.
            /// </summary>
            [Fact]
            public void TestEmpyStringThrowsException()
            {
                // Arrange
                string emptyString = string.Empty;
                // Act & Assert
                Assert.Throws<ArgumentException>(() => PSTestHelpers.GetFirstLine(emptyString));
            }

            /// <summary>
            /// Return list of commands when calling cmdlet with -list parameter.
            /// </summary>
            [Fact]
            public void ReturnListOfSupportedCommands()
            {
                // Arrange
                using var powerShell = PSTestHelpers.GetConfiguredShell();
                powerShell.AddCommand("resolve-argument");
                powerShell.AddParameter("list");
                var expectedResult = UIStrings.LIST_OF_COMMANDS;
                // Act
                Collection<PSObject> results = powerShell.Invoke();
                // Assert
                Assert.Equal(results.First(), expectedResult);
                Assert.True(results.Count == 1);
            }

            /// <summary>
            /// Return the initialisation script when called with -initialise parameter.
            /// 
            /// The stored script is modified when printed to remove the placeholder
            /// $cmdNames with actual commands handled by the text completer. Therefore,
            /// Test the first line of the script only.
            /// </summary>
            [Fact]
            public void Initialise()
            {
                // Arrange
                using var powerShell = PSTestHelpers.GetConfiguredShell();
                powerShell.AddCommand("resolve-argument");
                powerShell.AddParameter("Initialise");
                var expectedResult = PSTestHelpers.GetFirstLine(UIStrings.REGISTER_COMMAND_SCRIPT);
                // Act
                Collection<PSObject> results = powerShell.Invoke();
                // Assert
                Assert.True(results.Count == 1);
                var firstLineOfResponse = PSTestHelpers.GetFirstLine(results.First().ToString());
                Assert.Equal(firstLineOfResponse, expectedResult);
            }

            /// <summary>
            /// Return the initialisation script when called with the -printscript parameter.
            /// 
            /// The stored script is modified when printed to remove the placeholder
            /// $cmdNames with actual commands handled by the text completer. Therefore,
            /// Test the first line of the script only.
            /// </summary>
            [Fact]
            public void ReturnInitialisationScript()
            {
                // Arrange
                using var powerShell = PSTestHelpers.GetConfiguredShell();
                powerShell.AddCommand("resolve-argument");
                powerShell.AddParameter("printscript");
                var expectedResult = PSTestHelpers.GetFirstLine(UIStrings.REGISTER_COMMAND_SCRIPT);
                // Act
                Collection<PSObject> results = powerShell.Invoke();
                // Assert
                Assert.True(results.Count == 1);
                var firstLineOfResponse = PSTestHelpers.GetFirstLine(results.First().ToString());
                Assert.Equal(firstLineOfResponse, expectedResult);
            }
        }
    }
}

