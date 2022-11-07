

namespace PoshPredictiveText.Test.Tokeniser
{
    using PoshPredictiveText.SemanticParser;
    using Xunit;

    /// <summary>
    /// Test Path class.
    /// </summary>
    public class CommandPathTests
    {
        /// <summary>
        /// Test Path class.
        /// 
        /// Add commands to the class and test that the correct
        /// number of commands are recorded and the string output
        /// is correct.
        /// </summary>
        [Fact]
        public void PathTest()
        {
            // Arrange
            string command1 = "conda";
            string command2 = "env";
            string command3 = "list";
            string result = command1 + "." + command2 + "." + command3;

            // Act
            CommandPath commandPath = new(command1);
            commandPath.Add(command2);
            commandPath.Add(command3);

            // Assert
            Assert.Equal(3, commandPath.Count);
            Assert.Equal(result, commandPath.ToString());
        }

        /// <summary>
        /// Test ability to clone command path.
        /// </summary>
        [Fact]
        public void CloneCommandPathTest()
        {
            // Arrange
            string command1 = "conda";
            string command2 = "env";
            string command3 = "list";
            string result = command1 + "." + command2 + "." + command3;

            // Act
            CommandPath commandPath = new(command1);
            commandPath.Add(command2);
            CommandPath commandPath2 = new(commandPath);
            commandPath.Add(command3);

            // Assert
            Assert.Equal("conda.env", commandPath2.ToString());
            Assert.Equal(2, commandPath2.Count);
            Assert.Equal(result, commandPath.ToString());
            Assert.Equal(3, commandPath.Count);
        }
    }
}
