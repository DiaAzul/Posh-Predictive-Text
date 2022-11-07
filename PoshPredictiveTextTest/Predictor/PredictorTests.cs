
namespace PoshPredictiveText.Test.Predictor
{
    using PoshPredictiveText.PSReadLinePredictor;
    using System.Management.Automation.Subsystem.Prediction;
    using System.Threading;
    using Xunit;

    /// <summary>
    /// Posh Predictive Text Predictor Tests.
    /// </summary>
    public class PredictorTests
    {
        /// <summary>
        /// Tests the predictive text predictor.
        /// </summary>
        /// <param name="inputString">String entered on command line.</param>
        /// <param name="expectedSuggestions">Expected number of suggestions.</param>
        [Theory]
        [InlineData("conda ", 19)]
        [InlineData("conda env remove ", 10)]
        [InlineData("conda i", 3)]
        [InlineData("conda list --name --md5 ", 13)]
        [InlineData("conda activate -", 3)]
        public void PoshPredictiveTextPredictorTest(string inputString, int expectedSuggestions)
        {
            // Arrange
            string guid = Guid.NewGuid().ToString();
            var predictor = new Predictor(guid);

            PredictionClient client = new("test", PredictionClientKind.Terminal);
            PredictionContext context = PredictionContext.Create(inputString);
            CancellationToken cancellationToken = new(false);

            // Act
            var suggestionsPackage = predictor.GetSuggestion(client, context, cancellationToken);
            var suggestions = suggestionsPackage.SuggestionEntries;

            // Assert
            Assert.NotNull(suggestions);
            Assert.Equal(expectedSuggestions, suggestions.Count);
        }

        /// <summary>
        /// Test the properties of the predictor.
        /// </summary>
        [Fact]
        public void PropertiesNoCommandTest()
        {
            // Arrange
            string guid = Guid.NewGuid().ToString();
            var predictor = new Predictor(guid);

            // Act
            var id = predictor.Id.ToString();
            var name = predictor.Name.ToString();
            var description = predictor.Description.ToString();

            // Assert
            Assert.Equal(guid, id);
            Assert.Equal("Predictive Text", name);
            Assert.Equal("PowerShell tab-expansion of arguments for popular command line tools.", description);
        }

        /// <summary>
        /// Test the properties of the predictor when a command has been entered.
        /// The name property should be the name of the command.
        /// </summary>
        [Fact]
        public void PropertiesWithCommandTest()
        {
            // Arrange
            string guid = Guid.NewGuid().ToString();
            var predictor = new Predictor(guid);

            PredictionClient client = new("test", PredictionClientKind.Terminal);
            PredictionContext context = PredictionContext.Create("conda env list");
            CancellationToken cancellationToken = new(false);

            // Act
            // Get suggestions to set name property to conda.
            var _ = predictor.GetSuggestion(client, context, cancellationToken);

            var id = predictor.Id.ToString();
            var name = predictor.Name.ToString();
            var description = predictor.Description.ToString();

            // Assert
            Assert.Equal(guid, id);
            Assert.Equal("Conda", name);
            Assert.Equal("PowerShell tab-expansion of arguments for popular command line tools.", description);
        }
    }
}
