
namespace PoshPredictiveText.Test.Helpers
{
    using PoshPredictiveText;
    using Xunit;

    /// <summary>
    /// Test common functions.
    /// </summary>
    public class CommonFunctionTests
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

        /// <summary>
        /// Test extract command test.
        /// </summary>
        [Fact]
        public void ExtractCommandTest()
        {
            // Arrange
            string bareCommand = "conda";
            string executable = "conda.exe";
            string path = @"C:\mambaforge\scripts\conda.exe";
            string path2 = @"~/scripts/conda";

            // Act
            var bareResult = CommonTasks.ExtractCommand(bareCommand);
            var executableResult = CommonTasks.ExtractCommand(executable);
            var pathResult = CommonTasks.ExtractCommand(path);
            var path2Result = CommonTasks.ExtractCommand(path2);

            // Assert
            Assert.Equal(bareCommand, bareResult);
            Assert.Equal(bareCommand, executableResult);
            Assert.Equal(bareCommand, pathResult);
            Assert.Equal(bareCommand, path2Result);
        }
    }
}
