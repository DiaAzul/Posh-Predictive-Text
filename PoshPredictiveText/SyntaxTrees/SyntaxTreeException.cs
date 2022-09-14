
namespace PoshPredictiveText
{
    /// <summary>
    /// An exception raised if the syntax tree cannot be loaded.
    /// </summary>
    internal class SyntaxTreeException : Exception
    {
        internal SyntaxTreeException() { }

        internal SyntaxTreeException(string message)
            : base(message) { }

        internal SyntaxTreeException(string message, Exception inner)
            : base(message, inner) { }
    }
}
