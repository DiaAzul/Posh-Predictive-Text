
namespace PoshPredictiveText.SemanticParser
{
    using System.Management.Automation;
    /// <summary>
    /// Suggested response.
    /// 
    /// Data structure returned by the completion class and used by the calling
    /// class to generate specific data structures for the calling application.
    /// </summary>
    internal record Suggestion
    {
        internal string CompletionText { get; init; } = "";
        internal string ListText { get; init; } = "";
        internal CompletionResultType Type { get; init; } = CompletionResultType.ParameterValue;
        internal string ToolTip { get; init; } = "";
    }
}
