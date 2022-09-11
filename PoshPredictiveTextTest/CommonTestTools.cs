
namespace PoshPredictiveText.Test
{
    using Xunit;

    /// <summary>
    /// Methods useful across all tests.
    /// </summary>
    public class CommonTestTools
    {
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
            var result = CommonTestTools.GetFirstLine(multiLineString);
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
            Assert.Throws<ArgumentException>(() => CommonTestTools.GetFirstLine(emptyString));
        }
    }
}
