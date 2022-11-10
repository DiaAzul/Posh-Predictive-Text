
namespace PoshPredictiveText.SyntaxTrees
{
    using System.Management.Automation;

    /// <summary>
    /// The categorical type of a syntax item. 
    /// </summary>
    // TODO [HIGH][SYNTAXITEM] Remove CHOICE and use presence of choices to indicate choice.
    enum SyntaxItemType
    {
        COMMAND,
        POSITIONAL,
        PARAMETER,
        REDIRECTION,
    }

    /// <summary>
    /// Record within the command syntax tree.
    /// 
    /// Used to enumerate query results on the XML syntax tree.
    /// </summary>
    internal record SyntaxItem
    {
        public string Command { get; init; } = default!;
        public string Path { get; init; } = default!;
        public SyntaxItemType Type { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Alias { get; init; }
        public List<string> ParameterSets { get; init; } = default!;
        public int? MaxUses { get; init; }
        public string? Value { get; init; }
        public List<string>? Choices { get; init; }
        public int? MinCount { get; init; }
        public int? MaxCount { get; init; }
        public string? ToolTip { get; init; }

        /// <summary>
        /// Returns true if the syntax item is a command.
        /// </summary>
        internal bool IsCommand
        {
            get { return Type == SyntaxItemType.COMMAND; }
        }

        /// <summary>
        /// Returns true if the syntax item is an option parameter.
        /// </summary>
        internal bool IsOptionParameter
        {
            get { return Type == SyntaxItemType.PARAMETER && Value is null; }
        }

        /// <summary>
        /// Returns true if the syntax item is a parameter that takes values.
        /// </summary>
        internal bool IsParameter
        {
            get { return Type == SyntaxItemType.PARAMETER && Value is not null; }
        }

        /// <summary>
        /// Returns true if the syntax item is a positional parameter.
        /// </summary>
        internal bool IsPositionalParameter
        {
            get { return Type == SyntaxItemType.POSITIONAL && Value is not null; }
        }

        /// <summary>
        /// Returns true if the syntax item has an alias.
        /// </summary>
        internal bool HasAlias
        {
            get { return Alias != null; }
        }

        /// <summary>
        /// Property return the result type for the Syntax item.
        /// </summary>
        internal CompletionResultType ResultType
        {
            get
            {
                return Type switch
                {
                    SyntaxItemType.COMMAND => CompletionResultType.Command,
                    SyntaxItemType.PARAMETER => CompletionResultType.ParameterName,
                    SyntaxItemType.REDIRECTION => CompletionResultType.ParameterName,
                    SyntaxItemType.POSITIONAL => CompletionResultType.ParameterValue,
                    _ => CompletionResultType.ParameterValue,
                };
            }
        }

        internal bool AcceptsMultipleParameterValues
        {
            get
            {
                if (MinCount is null) return false;
                return MinCount > 1;
            }
        }
    }
}
