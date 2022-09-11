
namespace PoshPredictiveText.Test
{
    using System.Management.Automation.Language;

    /// <summary>
    /// Abstract syntax tree visitor to capture the CommandAst
    /// </summary>
    public class GetCommandAst : AstVisitor
    {
        /// <summary>
        /// Hold the last found commandAst, null if no commandAst found.
        /// </summary>
        public CommandAst? commandAst;

        /// <summary>
        /// Action when visiting a commandAst node.
        /// </summary>
        /// <param name="commandAst">Node in the abstract syntax tree.</param>
        /// <returns>Continue processing.</returns>
        public override AstVisitAction VisitCommand(CommandAst commandAst)
        {
            this.commandAst = commandAst;
            return AstVisitAction.Continue;
        }
    }
}
