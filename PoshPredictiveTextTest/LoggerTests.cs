
using System.DirectoryServices;
using Xunit;

namespace PoshPredictiveText.Test
{
    // TODO [ ][TEST] Tools tests.
    /// <summary>
    /// Tests logging functionality
    /// </summary>
    public class LoggerTests
    {

        /// <summary>
        /// Get a temporary file with given extension type.
        /// </summary>
        /// <param name="extension">Extension type of the file.</param>
        /// <returns>Path to temporary file.</returns>
        private static string GetTempFilePathWithExtension(string extension)
        {
            var path = Path.GetTempPath();
            var fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), extension);
            return Path.Combine(path, fileName);
        }


        private void createTemporaryLogFile()
        {

        }

        /// <summary>
        /// Test that we can write a message to the output file.
        /// </summary>
        [Fact]
        public void WriteLogTest()
        {
            // Arrange

            // Act

            // Assert
            Assert.True(true);
        }
    }
}
