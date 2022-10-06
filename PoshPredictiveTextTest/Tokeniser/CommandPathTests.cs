

namespace PoshPredictiveText.Test
{
    using Xunit;

    /// <summary>
    /// Test CommandPath class.
    /// </summary>
    public class CommandPathTests
    {
        /// <summary>
        /// Test CommandPath class.
        /// 
        /// Add commands to the class and test that the correct
        /// number of commands are recorded and the string output
        /// is correct.
        /// </summary>
        [Fact]
        public void CommandPathTest()
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
    }
}
