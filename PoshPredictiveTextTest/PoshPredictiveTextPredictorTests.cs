
namespace PoshPredictiveText.Test
{
    using System.Management.Automation.Subsystem.Prediction;
    using System.Threading;
    using Xunit;

    /// <summary>
    /// Posh Predictive Text Predictor Tests.
    /// </summary>
    public class PoshPredictiveTextPredictorTests
    {
        /// <summary>
        /// Tests the predictive text predictor.
        /// </summary>
        /// <param name="inputString">String entered on command line.</param>
        /// <param name="expectedSuggestions">Expected number of suggestions.</param>
        [Theory]
        [InlineData("conda ", 19)]
        [InlineData("conda env remove ", 9)]
        [InlineData("conda i", 3)]
        [InlineData("conda list --name --md5 ", 12)]
        [InlineData("conda activate -", 3)]
        public void PoshPredictiveTextPredictorTest(string inputString, int expectedSuggestions)
        {
            // Arrange
            string guid = Guid.NewGuid().ToString();
            var predictor = new PoshPredictiveTextPredictor(guid);

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
    }
}
