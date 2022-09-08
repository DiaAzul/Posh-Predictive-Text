
namespace PoshPredictiveText.Test
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    using Xunit;

    /// <summary>
    /// Tests for Syntax trees configuration.
    /// </summary>
    public class SyntaxTreeConfigTests
    {
        /// <summary>
        /// Runs consistency check method within Syntax tree config class.
        /// 
        /// The test is in the executable class so that it can access private data.
        /// </summary>
        [Fact]
        public void CheckConfigurationTest()
        {
            SyntaxTreesConfig.CheckConsistency();
        }

        // Following are implicitly tested during the configuration test.
        // Command From Alias
        // Definition
        // ToolTips
    }
}
