
namespace PoshPredictiveText.PSReadLinePredictor
{
    using PoshPredictiveText.SemanticParser;
    using PoshPredictiveText.SyntaxTrees;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Management.Automation;
    using System.Management.Automation.Subsystem;
    using System.Management.Automation.Subsystem.Prediction;
    using System.Threading;

    /// <summary>
    /// PSReadLine plugin providing predictive text capabilities.
    /// </summary>
    public class Predictor : ICommandPredictor
    {
        private readonly Guid _guid;

        private string? baseCommand = null;

        /// <summary>
        /// Initialise a new guid.
        /// </summary>
        /// <param name="guid">Guid passed from PSReadLine</param>
        internal Predictor(string guid)
        {
            _guid = new Guid(guid);
        }

        /// <summary>
        /// Gets the unique identifier for a subsystem implementation.
        /// </summary>
        public Guid Id => _guid;

        /// <summary>
        /// Gets the name of a command for which suggestion are being made.
        /// </summary>
        public string Name
        {
            get
            {
                if (baseCommand is null) return "Predictive Text";
                string capitaliseFirstLetter = string.Concat(baseCommand[0].ToString().ToUpper(), baseCommand.AsSpan(1));
                return capitaliseFirstLetter;
            }
        }

        /// <summary>
        /// Gets the description of a subsystem implementation.
        /// </summary>
        public string Description => "PowerShell tab-expansion of arguments for popular command line tools.";

        /// <summary>
        /// Create a suggestions package with the one suggestion using suggestion text and no tooltip.
        /// </summary>
        /// <param name="suggestionText">Text to returned as a suggestion.</param>
        /// <returns>Suggestion package with one predictive suggestion.</returns>
        private static SuggestionPackage SimpleSuggestionPackage(string suggestionText)
        {
            return new(new List<PredictiveSuggestion>() { new PredictiveSuggestion(suggestionText, "") });
        }

        /// <summary>
        /// Get the predictive suggestions. It indicates the start of a suggestion rendering session.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="context">The <see cref="PredictionContext"/> object to be used for prediction.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the prediction.</param>
        /// <returns>An instance of <see cref="SuggestionPackage"/>.</returns>
        public SuggestionPackage GetSuggestion(
            PredictionClient client,
            PredictionContext context,
            CancellationToken cancellationToken)
        {
            // If nothing on the command line return.            
            string inputText = context.InputAst.Extent.Text;
            var cursorPosition = context.CursorPosition.Offset;

            if (string.IsNullOrWhiteSpace(inputText) || context.InputAst is null)
            {
                return SimpleSuggestionPackage(inputText);
            }

            // Tokenise the syntax tree.
            List<PredictiveSuggestion> predictiveSuggestions = new();
            using (SemanticCLICache semanticCLICache = new())
            {
                if (semanticCLICache.Acquired)
                {
                    Visitor visitor = new();
                    context.InputAst.Visit(visitor);
                    SemanticCLI semanticCLI = visitor.SemanticCLI;

                    string wordToComplete = semanticCLI.LastToken?.Value ?? "";
                    if (inputText[^1] == ' ')
                    {
                        wordToComplete = "";
                        visitor.BlankVisit("", inputText.Length, inputText.Length);
                        semanticCLI = visitor.SemanticCLI;
                    }
                    string baseText = inputText[..(inputText.Length - wordToComplete.Length)];

                    SemanticCLICache.Stash(visitor.SemanticCLI, context.InputAst.ToString());

                    // If there is no base command, or the base command is not supported then return.
                    baseCommand = semanticCLI.BaseCommand;
                    if (baseCommand is null || !SyntaxTreesConfig.IsSupportedCommand(baseCommand))
                    {
                        return SimpleSuggestionPackage(inputText);
                    }

                    try
                    {
                        if (semanticCLI?.LastToken?.Suggestions is not null)
                        {
                            predictiveSuggestions = semanticCLI.LastToken.Suggestions
                                .Select(suggestion => new PredictiveSuggestion(
                                    suggestion: baseText + suggestion.CompletionText,
                                    toolTip: semanticCLI.SyntaxTree?.Tooltip(suggestion.ToolTip) ?? ""
                                ))
                                .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Write(ex.ToString(), LOGGER.LOGLEVEL.ERROR);
                    }

                    LOGGER.Write($"PREDICTOR: Suggesting {predictiveSuggestions.Count} completions");

                    if (predictiveSuggestions.Count == 0)
                    {
                        return SimpleSuggestionPackage(inputText);
                    }
                }
            }
            SuggestionPackage suggestionPackage = new(predictiveSuggestions);
            return suggestionPackage;
        }

        #region "interface methods for processing feedback"

        /// <summary>
        /// Gets a value indicating whether the predictor accepts a specific kind of feedback.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="feedback">A specific type of feedback.</param>
        /// <returns>True or false, to indicate whether the specific feedback is accepted.</returns>
        [ExcludeFromCodeCoverage]
        public bool CanAcceptFeedback(PredictionClient client, PredictorFeedbackKind feedback) => true;

        /// <summary>
        /// One or more suggestions provided by the predictor were displayed to the user.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="session">The mini-session where the displayed suggestions came from.</param>
        /// <param name="countOrIndex">
        /// When the value is greater than 0, it's the number of displayed suggestions from the list
        /// returned in <paramref name="session"/>, starting from the index 0. When the value is
        /// less than or equal to 0, it means a single suggestion from the list got displayed, and
        /// the index is the absolute value.
        /// </param>
        [ExcludeFromCodeCoverage]
        public void OnSuggestionDisplayed(PredictionClient client, uint session, int countOrIndex) { }

        /// <summary>
        /// The suggestion provided by the predictor was accepted.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="session">Represents the mini-session where the accepted suggestion came from.</param>
        /// <param name="acceptedSuggestion">The accepted suggestion text.</param>
        [ExcludeFromCodeCoverage]
        public void OnSuggestionAccepted(PredictionClient client, uint session, string acceptedSuggestion) { }

        /// <summary>
        /// A command line was accepted to execute.
        /// The predictor can start processing early as needed with the latest history.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="history">History command lines provided as references for prediction.</param>
        [ExcludeFromCodeCoverage]
        public void OnCommandLineAccepted(PredictionClient client, IReadOnlyList<string> history) { }

        /// <summary>
        /// A command line was done execution.
        /// </summary>
        /// <param name="client">Represents the client that initiates the call.</param>
        /// <param name="commandLine">The last accepted command line.</param>
        /// <param name="success">Shows whether the execution was successful.</param>
        [ExcludeFromCodeCoverage]
        public void OnCommandLineExecuted(PredictionClient client, string commandLine, bool success)
        {
            // Reset the cache once the command is executed.
            using SemanticCLICache cachedTokeniser = new();
            if (cachedTokeniser.Acquired)
            {
                SemanticCLICache.Clear();
            }
        }

        #endregion;
    }

    /// <summary>
    /// Register the predictor on module loading and unregister it on module un-loading.
    /// </summary>
    public class Init : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        private const string Identifier = "55c982c1-00c3-4005-90fb-010d008fd3bd";

        /// <summary>
        /// Gets called when assembly is loaded.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void OnImport()
        {
            var predictor = new Predictor(Identifier);
            SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, predictor);
        }

        /// <summary>
        /// Gets called when the binary module is unloaded.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, new Guid(Identifier));
        }
    }
}