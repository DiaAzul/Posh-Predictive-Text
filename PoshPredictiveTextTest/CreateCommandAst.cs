
namespace PoshPredictiveText.Test
{
    using System.Management.Automation.Language;
    using Xunit;

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

    /// <summary>
    /// Methods to support testing of the Abstract Syntax Tree.
    /// </summary>
    public class AstHelper
    {
        /// <summary>
        /// Helper command to generate a commandAst class given a string of commands as if
        /// they were entered at the command prompt.
        /// </summary>
        public static CommandAst CreateCommandAst(string promptText)
        {
            // Generate a script block abstract syntax tree from the provided prompt text.
            Token[] tokens = new Token[0];
            ParseError[] errors = new ParseError[0];
            ScriptBlockAst scriptBlock = Parser.ParseInput(promptText, out tokens, out errors);
            Assert.Empty(errors);

            // Extract the command abstract syntax tree from the script block abstract syntax tree.
            var commandAst = new GetCommandAst();
            scriptBlock.Visit(commandAst);
            Assert.NotNull(commandAst.commandAst);

            return commandAst.commandAst;
        }

        /// <summary>
        /// Test the create CommandAst function.
        /// </summary>
        [Fact]
        public void CreateCommandAstTest()
        {
            // Arrange
            string testString = "conda env list";

            // Act
            CommandAst ast = CreateCommandAst(testString);
            var outputString = ast.ToString();
            var elementCount = ast.CommandElements.Count;

            // Assert
            Assert.Equal(3, elementCount);
            Assert.Equal(testString, outputString);
        }
    }
}
