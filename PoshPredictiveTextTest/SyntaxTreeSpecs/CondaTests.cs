
namespace PoshPredictiveText.SyntaxTreeSpecs.Test
{
    using Microsoft.Management.Infrastructure.Generic;
    using PoshPredictiveText.Test;
    using System.Text.RegularExpressions;
    using Xunit;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// Test conda syntax trees
    /// </summary>
    public class CondaTests
    {
        /// <summary>
        /// Basic test to return suggestions for conda
        /// 
        /// The inline data must have the following format:
        /// - inputString - text as entered on the command line, the final character
        /// must be a space if the previous word is complete.
        /// - expectedSuggestions - The number of expected suggestions.
        /// </summary>
        [Theory]
        [InlineData("conda ", 19)]
        [InlineData("conda env remove ", 9)]
        [InlineData("conda i", 3)]
        [InlineData("conda list --name --md5 ", 12)]
        [InlineData("conda activate -", 3)]

        public void CondaSuggestionsTest(string inputString, int expectedSuggestions)
        {
            // Arrange
            // WordToComplete. CommandAstVisitor. CursorPosition.
            string wordToComplete = ""; 
            if (inputString[inputString.Length - 1] != ' ')
                wordToComplete = inputString.Split(' ').ToList().Last();
                
            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            int cursorPosition = commandAst.Extent.EndOffset;

            // Act
            CommandAstVisitor enteredTokens = new();
            commandAst.Visit(enteredTokens);
            var suggestions = Resolver.Suggestions(wordToComplete, enteredTokens, cursorPosition);

            // Assert
            Assert.Equal(expectedSuggestions, suggestions.Count);
        }

        /// <summary>
        /// Test that we get suggestions when asking for parameter values.
        /// 
        /// Note: Unless we know the state of the conda environment we won't
        /// know in advance how many suggestions will be returned. Therefore, we
        /// check to see whether the base environment is listed in the suggestions.
        /// 
        /// However, the test is running within the Visual Studio process and doesn't have
        /// access to the PowerShell console environment variables. Therefore, when conda
        /// helper looks for the conda environment variable it cannot find the root for
        /// conda.
        /// 
        /// To overcome this limitation in the test, we  create a powerShell console and
        /// execute 'conda env list' and then select the line which records the
        /// location of the 'base' environment. We then regex the folder name for the
        /// base environment which is what conda helper  reports instead of the base environment.
        /// 
        /// We then execute the resolver and check the base environment folder appears
        /// in the suggestions.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="expectedSuggestion"></param>
        [InlineData("conda activate ", 1)]
        [Theory]
        public void CondaParameterValueTest(string inputString, int expectedSuggestion)
        {

            var powershell = PowerShellMock.GetConfiguredShell();
            powershell.AddScript("conda env list");
            powershell.AddCommand("select-string").AddParameter("Pattern", "base");
            var result = powershell.Invoke();
            Assert.Single(result);
            string firstLine = result.First().ToString();

            Regex rx = new Regex(@"^.*\\(.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matches = rx.Matches(firstLine);
            Assert.Single(matches);
            Assert.Equal(2, matches.First().Groups.Count);
            string reportedBaseFolder = matches.First().Groups[1].ToString();

            // Arrange
            // WordToComplete. CommandAstVisitor. CursorPosition.
            string wordToComplete = "";
            if (inputString[inputString.Length - 1] != ' ')
                wordToComplete = inputString.Split(' ').ToList().Last();

            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            int cursorPosition = commandAst.Extent.EndOffset;

            // Act
            CommandAstVisitor enteredTokens = new();
            commandAst.Visit(enteredTokens);
            var suggestions = Resolver.Suggestions(wordToComplete, enteredTokens, cursorPosition);

            // Assert - As we don't know conda environment all we can test for is
            // that suggestions returned but we don't know how many.
            // Assert.Equal(expectedSuggestions, suggestions.Count);
            Assert.IsType<List<Suggestion>>(suggestions);

            List<string> environments = new();
            foreach (var suggestion in suggestions)
            {
                environments.Add(suggestion.CompletionText);
            }
            Assert.Contains(reportedBaseFolder, environments);
        }
    }
}
