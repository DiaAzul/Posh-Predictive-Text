namespace PoshPredictiveText
{
    /// <summary>
    /// The token holds semantic information for items entered
    /// on the command line.
    /// </summary>
    internal record Token
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
            Separator
        }

        internal string Value { get; init; } = default!;
        internal Type AstType { get; init; } = default!;
        internal int LowerExtent { get; init; } = 0;
        internal int UpperExtent { get; init; } = 0;
        // The following are mutable values.
        internal TokenType? SemanticType { get; set; } = null;
        internal string? ParameterValueName { get; set; } = null;
        internal int MaximumParameterValues { get; set; } = 0;

        /// <summary>
        /// True if the token is a command.
        /// </summary>
        internal bool IsCommand
        {
            get { return SemanticType == TokenType.Command; }
        }

        /// <summary>
        /// True if the token is a parameter.
        /// </summary>
        internal bool IsParameter
        {
            get { return SemanticType == TokenType.Parameter; }
        }
    }
}