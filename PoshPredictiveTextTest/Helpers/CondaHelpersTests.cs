
namespace PoshPredictiveText.Test.Helpers
{
    using PoshPredictiveText.Helpers;
    using Xunit;

    /// <summary>
    /// Conda Helpers tests
    /// </summary>
    public class CondaHelpersTests
    {
        /// <summary>
        /// Ensure that Conda is available within the environment.
        /// </summary>
        public CondaHelpersTests()
        {
            string? appveyor = Environment.GetEnvironmentVariable("APPVEYOR", EnvironmentVariableTarget.Process) ?? "false";
            bool isAppveyor = appveyor == "true";

            // Assumes that conda is on the path.
            string conda = "conda";
            // Unless we are running in appveyor, in which case we need to find the conda installation location.
            if (isAppveyor)
            {
                string? condaRoot = Environment.GetEnvironmentVariable("CONDA_INSTALL_LOCN", EnvironmentVariableTarget.Process);
                if (condaRoot is not null)
                {
                    conda = condaRoot + @"\Scripts\conda.exe";
                }
            }
            // Get a configured shell.
            using var powershell = PowerShellMock.GetConfiguredShell();
            // Initialise conda in the powershell environment.
            powershell.AddScript($"(& \"{conda}\" \"shell.powershell\" \"hook\") | Out-String | Invoke-Expression");
            var profile = powershell.Invoke();
            Assert.False(powershell.HadErrors, $"Unable to configure PowerShell with conda.Appveyor({isAppveyor}), Conda executable: {conda}. Check path for Miniconda in specification for build.");
        }

        /// <summary>
        /// SOLVER keyword tests.
        /// </summary>
        [Fact]
        public void ExperimentalSolversTest()
        {
            // Arrange

            // Act
            var solvers = CondaHelpers.ExperimentalSolvers();

            // Assert
            Assert.IsType<List<string>>(solvers);
        }

        /// <summary>
        /// Tests that conda environments are returned.
        /// 
        /// NOTE: This is a stub: The test needs implementing.
        /// Need to consider how a conda environment could be created
        /// in a test environment so that this could be tested in a
        /// controlled manner.
        /// </summary>
        [Fact]
        public void EnvironmentsTest()
        {
            // Arrange

            // Act

            // Assert
            Assert.True(true);
        }

        /// <summary>
        /// Test Get Parameter Values
        /// 
        /// Note: A method is created within the CondaHelper class which
        /// is called to generate a successful test. A second attribute to
        /// identify the fail condition is applied to both the default and
        /// test methods within the class.
        /// </summary>
        [Fact]
        public void GetParameterValuesTest()
        {
            // Arrange
            string successTest = "CONDAHELPERTEST";
            string failTest = "CONDAHELPERTESTFAIL";
            string wordToComplete = "WORD";

            // Act
            var success = CondaHelpers.GetParamaterValues(successTest, wordToComplete);

            // Assert
            Assert.Single(success);
            Assert.Equal("TestSuccessful", success[0].ListText);
            Assert.Equal(wordToComplete, success[0].CompletionText);

            Assert.Throws<SyntaxTreeException>(
                () => CondaHelpers.GetParamaterValues(failTest, wordToComplete));
        }
    }
}
