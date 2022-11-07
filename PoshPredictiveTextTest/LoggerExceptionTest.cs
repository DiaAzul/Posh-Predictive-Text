
namespace PoshPredictiveText.Test
{
    using PoshPredictiveText;
    using Xunit;

    /// <summary>
    /// Test Logger Exception
    /// </summary>
    public class LoggerExceptionTest
    {
        /// <summary>
        /// Test with no argument
        /// </summary>
        [Fact]
        public void TestNoArgument()
        {
            Assert.ThrowsAsync<LoggerException>(() => throw new LoggerException());
        }

        /// <summary>
        /// Test exception with string.
        /// </summary>
        [Fact]
        public void TestWithString()
        {
            Assert.ThrowsAsync<LoggerException>(() => throw new LoggerException("Error."));
        }

        /// <summary>
        /// Test rethrows exception with string.
        /// </summary>
        [Fact]
        public void TestRethrowsWithString()
        {
            Assert.ThrowsAsync<LoggerException>(() => throw new LoggerException("Error", new ArgumentException()));
        }
    }
}
