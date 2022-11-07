
namespace PoshPredictiveText.Test
{
    using PoshPredictiveText;
    using Xunit;

    /// <summary>
    /// Tests method to fetch string from the UIstrings resource.
    /// </summary>
    public class UIstringTests
    {
        /// <summary>
        /// Test that a call to fetch a UI string from the
        /// UI String resource returns a string value.
        /// </summary>

        [Fact]
        public void UIstringTest()
        {
            // Arrange
            string key = "VERSION";

            // Act
            var result = UIstring.Resource(key);

            // Assert
            Assert.IsType<string>(result);
        }
    }
}
