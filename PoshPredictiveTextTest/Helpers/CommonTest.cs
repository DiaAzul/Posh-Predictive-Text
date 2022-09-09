
namespace PoshPredictiveText.Test.Helpers
{
    using System.Web;
    using Xunit;

    /// <summary>
    /// Test common functions.
    /// </summary>
    public class CommonTest
    {
        /// <summary>
        /// Test encapsulating a string in quotation marks
        /// if it contains spaces.
        /// </summary>
        [Fact]
        public void EncapsulateIfSpacesTest()
        {
            // Arrange
            string noSpaces = "ThisTextHasNoSpaces";
            string spaces = "This Text Has Spaces";
            string encapsulatedText = String.Concat("\'", spaces, "\'");

            // Act
            var noSpacesEncapsulated = CommonTasks.EncapsulateIfSpaces(noSpaces, '\'');
            var spacesEncapsulated = CommonTasks.EncapsulateIfSpaces(spaces, '\'');
            var strippedEncapsulatedText = CommonTasks.Decapsulate(encapsulatedText);

            // Assert
            Assert.Equal(noSpaces, noSpacesEncapsulated);
            Assert.Equal(encapsulatedText, spacesEncapsulated);
            Assert.Equal(spaces, strippedEncapsulatedText);
        }
    }
}
