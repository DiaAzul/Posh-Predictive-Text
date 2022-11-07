
namespace PoshPredictiveText.Test.SyntaxTrees
{
    using PoshPredictiveText.SyntaxTrees;
    using Xunit;

    /// <summary>
    /// Test Syntax Tree Exception
    /// </summary>
    public class SyntaxTreeExceptionTests
    {
        /// <summary>
        /// Test with no argument
        /// </summary>
        [Fact]
        public void TestNoArgument()
        {
            Assert.ThrowsAsync<SyntaxTreeException>(() => throw new SyntaxTreeException());
        }

        /// <summary>
        /// Test exception with string.
        /// </summary>
        [Fact]
        public void TestWithString()
        {
            Assert.ThrowsAsync<SyntaxTreeException>(() => throw new SyntaxTreeException("Error."));
        }

        /// <summary>
        /// Test rethrows exception with string.
        /// </summary>
        [Fact]
        public void TestRethrowsWithString()
        {
            Assert.ThrowsAsync<SyntaxTreeException>(() => throw new SyntaxTreeException("Error", new ArgumentException()));
        }
    }
}
