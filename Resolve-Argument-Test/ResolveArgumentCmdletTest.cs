

namespace Resolve_Argument.Tests
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using Xunit;

    public static class PSTestShell
    {
        /// <summary>
        /// Configures an instance of PowerShell containing the cmdlet to test.
        /// </summary>
        /// <returns>
        /// Instance of PowerShell containing cmdlet.
        /// </returns>
        public static System.Management.Automation.PowerShell GetConfiguredShell()
        {
            var sessionState = InitialSessionState.CreateDefault();

            // Add cmdlet to the shell instance.
            SessionStateCmdletEntry cmdletToTest = new("resolve-argument", typeof(ResolveArgumentCmdlet), null);
            sessionState.Commands.Add(cmdletToTest);

            // Create an instance of the shell.
            var testShellInstance = System.Management.Automation.PowerShell.Create(sessionState);
            return testShellInstance;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class ResolveArgumentCmdletTests
    {
        public class GetRepeatedString
        {
            public class WhenCalledWithValidParameters
            {
                [Fact]
                public void ReturnListOfSupportedCommands()
                {
                    // Arrange
                    using var powerShell = PSTestShell.GetConfiguredShell();
                    powerShell.AddCommand("resolve-argument");
                    powerShell.AddParameter("list");
                    var expectedResult = "Listicles.";
                    // Act
                    Collection<PSObject> results = powerShell.Invoke();
                    // Assert
                    Assert.Equal(results.First(), expectedResult);
                    Assert.True(results.Count == 1);
                }

                [Fact]
                public void Initialise()
                {
                    // Arrange
                    using var powerShell = PSTestShell.GetConfiguredShell();
                    powerShell.AddCommand("resolve-argument");
                    powerShell.AddParameter("Initialise");
                    var expectedResult = "Initialise.";
                    // Act
                    Collection<PSObject> results = powerShell.Invoke();
                    // Assert
                    Assert.Equal(results.First(), expectedResult);
                    Assert.True(results.Count == 1);
                }

                [Fact]
                public void ReturnInitialisationScript()
                {
                    // Arrange
                    using var powerShell = PSTestShell.GetConfiguredShell();
                    powerShell.AddCommand("resolve-argument");
                    powerShell.AddParameter("PrintScript");
                    var expectedResult = "Print.";
                    // Act
                    Collection<PSObject> results = powerShell.Invoke();
                    // Assert
                    Assert.Equal(results.First(), expectedResult);
                    Assert.True(results.Count == 1);
                }
            }
        }
    }
}

