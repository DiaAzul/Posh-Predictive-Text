
namespace PoshPredictiveText
{
    using System.Management.Automation;

    /// <summary>
    /// Record within the command syntax tree.
    /// 
    /// Used to enumerate query results on the XML syntax tree.
    /// </summary>
    internal record SyntaxItem
    {
        internal string Command { get; init; } = default!;
        internal string CommandPath { get; init; } = default!;
        internal string Type { get; init; } = default!;
        internal string? Argument { get; init; }
        internal string? Alias { get; init; }
        internal bool MultipleUse { get; init; } = default!;
        internal string? Parameter { get; init; }
        internal bool? MultipleParameterValues { get; init; }
        internal string? ToolTip { get; init; }

        /// <summary>
        /// Returns true if the syntax item is a command.
        /// </summary>
        internal bool IsCommand
        {
            get { return Type == "CMD"; }
        }

        /// <summary>
        /// Returns true if the syntax item is an option parameter.
        /// </summary>
        internal bool IsOptionParameter
        {
            get { return Type == "OPT"; }
        }

        /// <summary>
        /// Returns true if the syntax item is a parameter that takes values.
        /// </summary>
        internal bool IsParameter
        {
            get { return Type == "PRM"; }
        }

        /// <summary>
        /// Returns true if the syntax item is a positional parameter.
        /// </summary>
        internal bool IsPositionalParameter
        {
            get { return Type == "POS"; }
        }

        internal bool isRedirection
        {
            get { return Type == "RED";  }
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
                    "CMD" => CompletionResultType.Command,
                    "OPT" => CompletionResultType.ParameterName,
                    "PRM" => CompletionResultType.ParameterName,
                    "POS" => CompletionResultType.ParameterValue,
                    "RED" => CompletionResultType.ParameterName,
                    _ => CompletionResultType.ParameterValue,
                };
            }
        }
    }
}
