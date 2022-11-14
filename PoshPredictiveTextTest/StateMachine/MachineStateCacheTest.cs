

namespace PoshPredictiveText.Test.StateMachine
{
    using PoshPredictiveText.SemanticParser;
    using Xunit;

    /// <summary>
    /// Test the machine state cache.
    /// </summary>
    public class MachineStateCacheTest
    {

        /// <summary>
        /// Basic test to add item to the cache and retrieve
        /// it using Get adn TryGet methods.
        /// </summary>
        [Fact]
        public void BasicAddGetTest()
        {
            // Arrange
            string key1 = "state1";
            MachineState state1 = new();

            // Act
            MachineStateCache.Add(key1, state1);

            var outState1 = MachineStateCache.Get(key1);

            var success = MachineStateCache.TryGetValue(key1, out MachineState tryState1);

            //Assert
            Assert.IsType<MachineState>(outState1);
            Assert.IsType<MachineState>(tryState1);
            Assert.True(success);
        }

        /// <summary>
        /// Basic test to add item to the cache and retrieve
        /// it using Get adn TryGet methods.
        /// </summary>
        [Fact]
        public void ChangePropertyOfCachedItemTest()
        {
            // Arrange
            string key1 = "state1";
            MachineState state1 = new()
            {
                CommandPath = new CommandPath("conda")
            };

            // Act
            MachineStateCache.Add(key1, state1.Copy());
            state1.CommandPath = new CommandPath("Changed");
            var outState1 = MachineStateCache.Get(key1);
            // Update key-value.
            MachineStateCache.Add(key1, state1);
            var success = MachineStateCache.TryGetValue(key1, out MachineState tryState1);

            //Assert
            Assert.IsType<MachineState>(outState1);
            Assert.IsType<MachineState>(tryState1);
            Assert.True(success);
        }
    }
}
