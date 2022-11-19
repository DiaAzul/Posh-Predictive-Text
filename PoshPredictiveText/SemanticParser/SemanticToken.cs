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

        /// <summary>
        /// Get the text shown on the command line
        /// </summary>
        internal string Value { get; init; } = default!;

        /// <summary>
        /// Get the abstract syntax type for the command line token.
        /// </summary>
        internal Type AstType { get; init; } = default!;

        // Get the lower extent of the text on the command line.
        internal int LowerExtent { get; init; } = 0;

        /// <summary>
        /// Get the upper extent of the text on the command line.
        /// </summary>
        internal int UpperExtent { get; init; } = 0;

        // ** The following are mutable values.**
        /// <summary>
        /// emantic type of the token.
        /// </summary>
        internal TokenType? SemanticType { get; set; } = null;

        /// <summary>
        /// True if the token is an exact match for a syntax item.
        /// </summary>
        internal bool IsExactMatch { get; set; } = false;

        /// <summary>
        /// Get the parameter value name.
        /// </summary>
        internal string? ParameterValueName { get; set; } = null;

        /// <summary>
        /// Get the minium number of parameter values for a parameter.
        /// </summary>
        internal int? MinimumParameterValues { get; set; } = null;

        /// <summary>
        /// Get the maximum number of parameter values for a parameter.
        /// </summary>
        internal int? MaximumParameterValues { get; set; } = null;

        // Get list of suggestions for this token (if partial word).
        internal List<SyntaxItem>? SuggestedSyntaxItems { get; set; } = null;

        // Get parameter set membership of the token.
        internal List<string>? ParameterSet { get; set; } = null;

        /// <summary>
        /// True if the semantic token is a command.
        /// </summary>
        internal bool IsCommand => SemanticType == TokenType.Command;

        /// <summary>
        /// True if the semantic token is a parameter.
        /// </summary>
        internal bool IsParameter => SemanticType == TokenType.Parameter;

        /// <summary>
        /// True if the semantic token is a positional value.
        /// </summary>
        internal bool IsPositionalValue => SemanticType == TokenType.PositionalValue;

        /// <summary>
        /// True if the semantic token is a parameter value.
        /// </summary>
        internal bool IsParameterValue => SemanticType == TokenType.ParameterValue;

        /// <summary>
        /// Load from syntax item.
        /// 
        /// Load token type, positional index, minumum and maximum parameter
        /// values from the syntax item.
        /// </summary>
        /// <param name="syntaxItem"></param>
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

            ParameterValueName = syntaxItem.Value;
            MinimumParameterValues = syntaxItem.MinCount;
            MaximumParameterValues = syntaxItem.MaxCount;
        }
    }
}