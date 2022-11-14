
namespace PoshPredictiveText.SyntaxTrees
{
    using System.Management.Automation;

    /// <summary>
    /// The categorical type of a syntax item. 
    /// </summary>
    enum SyntaxItemType
    {
        COMMAND,
        POSITIONAL,
        PARAMETER,
        REDIRECTION,
    }

    /// <summary>
    /// Syntax item defining the semantics of arguments within a command.
    /// </summary>
    internal record SyntaxItem
    {
        public string Command { get; init; } = default!;
        public string Path { get; init; } = default!;
        public SyntaxItemType ItemType { get; init; } = default!;
        public string Name { get; init; } = default!;
        public string? Alias { get; init; }
        public List<string> ParameterSet { get; init; } = default!;
        public int? MaxUses { get; init; }
        public string? Value { get; init; }
        public List<string>? Choices { get; init; }
        public int? MinCount { get; init; }
        public int? MaxCount { get; init; }
        public string? ToolTip { get; init; }

        /// <summary>
        /// True if the syntax item is a command.
        /// </summary>
        internal bool IsCommand => ItemType == SyntaxItemType.COMMAND;

        /// <summary>
        /// True if the syntax item is an option parameter.
        /// </summary>
        internal bool IsOptionParameter => ItemType == SyntaxItemType.PARAMETER && Value is null;

        /// <summary>
        /// True if the syntax item is a parameter that takes values.
        /// </summary>
        internal bool IsParameter => ItemType == SyntaxItemType.PARAMETER && Value is not null;

        /// <summary>
        /// True if the syntax item is a positional parameter.
        /// </summary>
        internal bool IsPositional => ItemType == SyntaxItemType.POSITIONAL && Value is not null;

        /// <summary>
        /// Get the index of a positional parameter.
        /// 
        /// Returns null if the syntax item is not positional, the index is not
        /// preceded by `*` or the index cannot be parsed.
        /// </summary>
        internal int? PositionalIndex
        {
            get
            {
                if (ItemType != SyntaxItemType.POSITIONAL) return null;
                if (Name[0] != '*') return null;
                if (Int32.TryParse(Name[1..], out var n)) return n;
                return null;
            }
        }

        /// <summary>
        /// True if the syntax item has choices for positional or parameter values.
        /// </summary>
        internal bool HasChoices =>  Choices is not null;

        /// <summary>
        /// Gets the list of choices for the parameter or positional value.
        /// 
        /// If there are no choices return an empty string.
        /// </summary>
        internal List<string> GetChoices => Choices ?? new List<string>();

        /// <summary>
        /// Get a list of intersecting parameter sets. 
        /// 
        /// Returns an empty string if no sets match returns an empty list.
        /// </summary>
        /// <param name="choices">List of parameter sets to match with syntax item parameter sets.</param>
        /// <returns>Intersecting parameter sets.</returns>
        internal List<string> GetIntersectingParameterSets(List<string> parameterSets)
        {
            return ParameterSet?.Intersect(parameterSets).ToList() ?? new List<string>(); 
        }

        /// <summary>
        /// True if the syntax item has an alias.
        /// </summary>
        internal bool HasAlias  => Alias != null;

        /// <summary>
        /// Get the completion result type for the Syntax item.
        /// </summary>
        internal CompletionResultType ResultType
        {
            get
            {
                return ItemType switch
                {
                    SyntaxItemType.COMMAND => CompletionResultType.Command,
                    SyntaxItemType.PARAMETER => CompletionResultType.ParameterName,
                    SyntaxItemType.REDIRECTION => CompletionResultType.ParameterName,
                    SyntaxItemType.POSITIONAL => CompletionResultType.ParameterValue,
                    _ => CompletionResultType.ParameterValue,
                };
            }
        }

        /// <summary>
        /// True if the syntax item accepts multiple parameter values.
        /// </summary>
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
