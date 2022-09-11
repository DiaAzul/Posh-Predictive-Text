

namespace PoshPredictiveText.Test
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Test cmdlet to install predictive text.
    /// </summary>
    public class CmdletInstallPredictiveTextTests
    {
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
            using var powerShell = PowerShellMock.GetConfiguredShell();

            // Act
            powerShell.Commands.Clear();
            Collection<PSObject> results = powerShell.AddCommand("Install-PredictiveText")
                                                           .Invoke();

            // Test for cmdlet installed.
            // Note: The testing installs cmdlets individually, they are not installed from a module.
            // Therefore, we cannot test for a module, only the cmdlets.
            powerShell.Commands.Clear();
            Collection<PSObject> IsCmdLetListed = powerShell.AddCommand("Get-Command")
                                                            .AddParameter("Name", "Install-PredictiveText")
                                                            .Invoke();

            // Assert
            Assert.Empty(results);
            Assert.Single(IsCmdLetListed);
        }
    }
}
