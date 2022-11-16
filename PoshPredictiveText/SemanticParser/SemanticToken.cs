using PoshPredictiveText.SyntaxTrees;

namespace PoshPredictiveText.SemanticParser
{
    /// <summary>
    /// The token holds semantic information for items entered
    /// on the command line.
    /// </summary>
    internal record SemanticToken
    {
        /// <summary>
        /// Identifies the type of the token.
        /// </summary>
        internal enum TokenType
        {
            Command,
            Constant,
            Parameter,
            ParameterValue,
            PositionalValue,
            Redirection,
            Separator,
            Space,
            StringConstant
        }

        // Value recorded on the command line
        internal string Value { get; init; } = default!;

        // Abstract syntax type recorded for command line.
        internal Type AstType { get; init; } = default!;

        // Start position of text.
        internal int LowerExtent { get; init; } = 0;

        // End position of text.
        internal int UpperExtent { get; init; } = 0;

        // ** The following are mutable values.**
        // Semantic type of the token.
        internal TokenType? SemanticType { get; set; } = null;

        // If a positional value what is the positional index
        internal int? PositonalIndex { get; set; } = null;

        // True if the token is an exact match for a syntax item.
        internal bool IsExactMatch { get; set; } = false;

        // Parameter Value name (matches syntax tree helper).
        internal string? ParameterValueName { get; set; } = null;

        // Minium and maximum number of parameter values for a parameter.
        internal int? MinimumParameterValues { get; set; } = null;
        internal int? MaximumParameterValues { get; set; } = null;

        // List of suggestions for this token (if partial word).
        internal List<SyntaxItem>? SuggestedSyntaxItems { get; set; } = null;

        // Parameter sets membership of the token.
        internal List<string>? ParameterSet { get; set; } = null;

        /// <summary>
        /// True if the semantic token is a command.
        /// </summary>
        internal bool IsCommand
        {
            get { return SemanticType == TokenType.Command; }
        }

        /// <summary>
        /// True if the semantic token is a parameter.
        /// </summary>
        internal bool IsParameter
        {
            get { return SemanticType == TokenType.Parameter; }
        }

        /// <summary>
        /// True if the semantic token is a positional value.
        /// </summary>
        internal bool IsPositionalValue
        {
            get { return SemanticType == TokenType.PositionalValue; }
        }

        internal void LoadFromSyntaxItem(SyntaxItem syntaxItem)
        {
            SemanticType = syntaxItem.ItemType switch
            {
                SyntaxItemType.COMMAND => (TokenType?)TokenType.Command,
                SyntaxItemType.PARAMETER => (TokenType?)TokenType.Parameter,
                SyntaxItemType.POSITIONAL => (TokenType?)TokenType.PositionalValue,
                SyntaxItemType.REDIRECTION => (TokenType?)TokenType.Redirection,
                _ => null,
            };

            PositonalIndex = syntaxItem.PositionalIndex;
            ParameterValueName = syntaxItem.Value;
            MinimumParameterValues = syntaxItem.MinCount;
            MaximumParameterValues = syntaxItem.MaxCount;
        }
    }
}