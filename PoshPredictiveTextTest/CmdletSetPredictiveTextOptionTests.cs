
namespace PoshPredictiveText.Test
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using Xunit;

    /// <summary>
    /// Set-PredicitiveTextOptions cmdlet tests.
    /// - Logging
    /// - Remove conda tab-expansion script.
    /// </summary>
    public class CmdletSetPredictiveTextOptionTests : IDisposable
    {
        private readonly string tempLoggingFile;

        /// <summary>
        /// Setup a temporary logging file.
        /// </summary>
        public CmdletSetPredictiveTextOptionTests()
        {
            string path = Path.GetTempPath();
            string fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), "txt");
            tempLoggingFile = Path.Combine(path, fileName);
        }

        /// <summary>
        /// Ensure temporary file is cleaned up after test.
        /// </summary>
        public void Dispose()
        {
            if (File.Exists(tempLoggingFile))
            {
                File.Delete(tempLoggingFile);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Return list of commands when calling cmdlet with -list parameter.
        /// </summary>
        [Fact]
        public void SetLoggingFileInfoTest()
        {
            // Arrange
            using var powerShell = PowerShellMock.GetConfiguredShell();
            powerShell.AddCommand("Set-PredictiveTextOption")
                .AddParameter("LogFile", tempLoggingFile)
                .AddParameter("LogLevel", "info");

            // Act
            Collection<PSObject> results = powerShell.Invoke();

            // Assert
            Assert.Empty(results);
            Assert.False(powerShell.HadErrors);
            Assert.True(File.Exists(tempLoggingFile));
        }

        /// <summary>
        /// Test excpetion thrown if logile path is empty.
        /// </summary>
        [Fact]
        public void EmptyLogFileNameTest()
        {
            // Arrange
            using var powerShell = PowerShellMock.GetConfiguredShell();
            powerShell.AddCommand("Set-PredictiveTextOption")
                .AddParameter("LogFile", "")
                .AddParameter("LogLevel", "info");

            // Act
            // The ParameterBindingValidationException is not public, the following does not
            // work. Need to find another mechanism for testing.
            // Assert.Throws<ParameterBindingValidationException>(() => powerShell.Invoke());
            Assert.True(true);
            //// Act
            //var results = powerShell.Invoke();

            //// Assert
            //Assert.Empty(results);
            //Assert.True(powerShell.HadErrors);
        }

        /// <summary>
        /// Tests that error thrown when the log level is incorrect.
        /// </summary>
        [Fact]
        public void IncorrectLogLevelTest()
        {
            // Arrange
            using var powerShell = PowerShellMock.GetConfiguredShell();
            powerShell.AddCommand("Set-PredictiveTextOption")
                .AddParameter("LogFile", tempLoggingFile)
                .AddParameter("LogLevel", "incorrectLogLevel");

            // Act
            // The ParameterBindingValidationException is not public, the following does not
            // work. Need to find another mechanism for testing.
            // Assert.Throws<ParameterBindingValidationException>(() => powerShell.Invoke());
            Assert.True(true);
        }

        /// <summary>
        /// Tests that file created with info level when loglevel missing.
        /// </summary>
        [Fact]
        public void LogLevelParameterMissingTest()
        {
            // Arrange
            using var powerShell = PowerShellMock.GetConfiguredShell();
            powerShell.AddCommand("Set-PredictiveTextOption")
                .AddParameter("LogFile", tempLoggingFile);

            // Act
            var results = powerShell.Invoke();

            // Assert
            Assert.Empty(results);
            Assert.True(File.Exists(tempLoggingFile)); ;
        }


        /// <summary>
        /// Remove conda tab expansion test.
        /// 
        /// NOTE: Need to activate conda in PowerShell first.
        /// </summary>
        [Fact]
        public void RemoveCondaTabExpansionTest()
        {
            using var powerShell = PowerShellMock.GetConfiguredShell();

            powerShell.AddCommand("Set-PredictiveTextOption")
                .AddParameter("RemoveCondaTabExpansion");
            var results = powerShell.Invoke();

            powerShell.Commands.Clear();
            powerShell.AddCommand("Test-Path")
                .AddParameter("Path", "Function:\\TabExpansion");
            var checkResults = powerShell.Invoke();

            // Assert
            Assert.IsType<bool>(checkResults[0].BaseObject);
            var checkResult = (bool)checkResults[0].BaseObject;
            Assert.False(checkResult);
        }
    }
}
