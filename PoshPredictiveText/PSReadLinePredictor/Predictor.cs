
namespace PoshPredictiveText
{
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

            if (string.IsNullOrWhiteSpace(inputText)) return default;

            // If there is no abstract syntax tree return.
            if (context.InputAst is null) return default;

            // Tokenise the syntax tree.
            List<PredictiveSuggestion> predictiveSuggestions = new();
            using (TokeniserCache cachedTokeniser = new())
            {
                if (cachedTokeniser.Acquired)
                {
                    Visitor visitor = new();
                    context.InputAst.Visit(visitor);
                    Tokeniser enteredTokens = visitor.Tokeniser;
                    TokeniserCache.Stash(visitor.Tokeniser, _guid);

                    // If there is no base command, or the base command is not supported then return.
                    if (enteredTokens.BaseCommand is null) return default;
                    baseCommand = enteredTokens.BaseCommand;
                    if (!SyntaxTreesConfig.IsSupportedCommand(enteredTokens.BaseCommand)) return default;

                    string wordToComplete = enteredTokens.LastToken?.Value ?? "";
                    if (inputText[^1] == ' ')
                    {
                        wordToComplete = "";
                    }
                    string baseText = inputText[..(inputText.Length - wordToComplete.Length)];

                    var results = Resolver.Suggestions(wordToComplete, enteredTokens, cursorPosition);
                    if (results.Count == 0) return default;

                    foreach (Suggestion result in results)
                    {
                        PredictiveSuggestion suggestion = new(baseText + result.CompletionText, result.ToolTip);
                        predictiveSuggestions.Add(suggestion);
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
            StateMachineStateCache.Reset();
            using TokeniserCache cachedTokeniser = new();
            if (cachedTokeniser.Acquired)
            {
                TokeniserCache.Clear();
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