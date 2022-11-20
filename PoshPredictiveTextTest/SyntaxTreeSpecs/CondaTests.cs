﻿
namespace PoshPredictiveText.Test.SyntaxTreeSpecs
{
    using PoshPredictiveText.SemanticParser;
    using PoshPredictiveText.Test;
    using PoshPredictiveText.SyntaxTrees;
    using Xunit;
    using System.Management.Automation;

    /// <summary>
    /// Test conda syntax trees
    /// </summary>
    public class CondaTests
    {
        /// <summary>
        /// Ensure that Conda is available within the environment.
        /// </summary>
        public CondaTests()
        {
            string? appveyor = Environment.GetEnvironmentVariable("APPVEYOR", EnvironmentVariableTarget.Process) ?? "false";
            bool isAppveyor = appveyor == "true";

            string conda = "conda";
            if (isAppveyor)
            {
                string? condaRoot = Environment.GetEnvironmentVariable("CONDA_INSTALL_LOCN", EnvironmentVariableTarget.Process);
                if (condaRoot is not null)
                {
                    conda = condaRoot + @"\Scripts\conda.exe";
                }
            }
            using var powershell = PowerShellMock.GetConfiguredShell();
            // Cannot guarantee running on windows or the location of conda or whether it is on the path.
            powershell.AddScript($"(& \"{conda}\" \"shell.powershell\" \"hook\") | Out-String | Invoke-Expression");
            var profile = powershell.Invoke();
            Assert.False(powershell.HadErrors, $"Unable to configure PowerShell with conda.Appveyor({isAppveyor}), Conda executable: {conda}. Check path for Miniconda in specification for build.");
        }

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
        [InlineData("conda env remove ", 10)]
        [InlineData("conda i", 3)]
        [InlineData("conda list --name --md5 ", 11)]
        [InlineData("conda activate -", 3)]
        [InlineData("conda config --file ", 0)]

        public void CondaSuggestionsTest(string inputString, int expectedSuggestions)
        {
            // Arrange
            // WordToComplete. CommandAstVisitor. CursorPosition.
            string wordToComplete = "";
            if (inputString[^1] != ' ')
                wordToComplete = inputString.Split(' ').ToList().Last();

            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            int cursorPosition = commandAst.Extent.EndOffset;

            // Act
            Visitor visitor = new();
            commandAst.Visit(visitor);
            if (wordToComplete == "")
            {
                visitor.BlankVisit("", commandAst.Extent.StartColumnNumber, commandAst.Extent.EndColumnNumber);
            }
            var semanticCLI = visitor.SemanticCLI;
            List<Suggestion> suggestions = new();
            if (semanticCLI?.LastToken?.Suggestions is not null)
            {
                suggestions = semanticCLI.LastToken.Suggestions
                    .Select(suggestion => new Suggestion {
                        CompletionText = suggestion.CompletionText,
                        ListText = suggestion.ListText,
                        Type = suggestion.Type,
                        ToolTip = semanticCLI.SyntaxTree?.Tooltip(suggestion.ToolTip) ?? ""
                    })
                    .ToList();
            }
            // Assert
            Assert.Equal(expectedSuggestions, suggestions.Count);
        }

        /// <summary>
        /// Test that we get suggestions when asking for parameter values.
        /// </summary>
        /// <param name="inputString"></param>
        [InlineData("conda activate ")]
        [Theory]
        public void GetCondaEnvironmentSuggestions(string inputString)
        {
            // Arrange
            // WordToComplete. CommandAstVisitor. CursorPosition.
            string wordToComplete = "";
            if (inputString[^1] != ' ')
                wordToComplete = inputString.Split(' ').ToList().Last();

            var commandAst = PowerShellMock.CreateCommandAst(inputString);
            int cursorPosition = commandAst.Extent.EndOffset;

            // Act
            Visitor visitor = new();
            commandAst.Visit(visitor);
            if (wordToComplete == "")
            {
                visitor.BlankVisit("", commandAst.Extent.StartColumnNumber, commandAst.Extent.EndColumnNumber);
            }
            var semanticCLI = visitor.SemanticCLI;
            List<Suggestion> suggestions = new();
            if (semanticCLI?.LastToken?.Suggestions is not null)
            {
                suggestions = semanticCLI.LastToken.Suggestions
                    .Select(suggestion => new Suggestion
                    {
                        CompletionText = suggestion.CompletionText,
                        ListText = suggestion.ListText,
                        Type = suggestion.Type,
                        ToolTip = semanticCLI.SyntaxTree?.Tooltip(suggestion.ToolTip) ?? ""
                    })
                    .ToList();
            }

            // Assert - As we don't know conda environment all we can test for is
            // that suggestions returned but we don't know how many.
            // Assert.Equal(expectedSuggestions, suggestions.Count);
            Assert.IsType<List<Suggestion>>(suggestions);

            List<string> environments = new();
            foreach (var suggestion in suggestions)
            {
                environments.Add(suggestion.CompletionText);
            }
            // TODO [LOW][APPVEYOR] Test Conda environments in appveyor.
            string? appveyor = Environment.GetEnvironmentVariable("APPVEYOR", EnvironmentVariableTarget.Process) ?? "false";
            bool isAppveyor = appveyor == "true";
            if (!isAppveyor)
                Assert.Contains("base", environments);
        }

        /// <summary>
        /// Test to load and inspect conda syntax tree.
        /// </summary>
        [Fact]
        public void LoadCondaSyntaxTree()
        {
            // Arrange
            string syntaxTreeName = "conda";

            // Act
            var syntaxTree = SyntaxTrees.Tree(syntaxTreeName);

            // Assert.
            Assert.IsType<SyntaxTree>(syntaxTree);
            Assert.NotEqual(0, syntaxTree.Count);

            // Act
            var syntaxItems = syntaxTree.SubCommands("conda");
            var firstItem = syntaxItems.First();
            var parameterSet = firstItem.ParameterSet;

            Assert.Equal(17, syntaxItems.Count);
            Assert.IsType<List<string>>(parameterSet);
            Assert.Single(parameterSet);
            Assert.Equal("1", parameterSet.First());
            Assert.IsType<SyntaxItem>(firstItem);
        }
    }
}
