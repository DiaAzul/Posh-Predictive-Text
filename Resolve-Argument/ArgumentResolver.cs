
namespace ResolveArgument
{
    using System.Linq;
    using System.Xml.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Reflection;

    /// <summary>
    /// Process conda arguments and return suggested tab-completions.
    /// </summary>
    internal class ArgumentResolver
    {
        private static XDocument? syntaxTree;

        internal static void LoadSyntaxTree()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                syntaxTree = new XDocument(assembly.GetManifestResourceStream("ResolveArgument.SyntaxTrees.CondaSyntaxTree.xml"));
            }
            catch (System.IO.FileLoadException)
            {
                LOGGER.Write("File was found, could not load.");
            }
            catch (System.IO.FileNotFoundException)
            {
                LOGGER.Write("File not found.");
            }
            catch (System.BadImageFormatException)
            {
                LOGGER.Write("File wrong format.");
            }
        }

     internal static List<CompletionResult> Suggestions(string WordToComplete, CommandAstVisitor commandTokens, int CursorPosition)
        {
            if (syntaxTree == null)
            {
                LOGGER.Write("Loading Syntax Tree.");
                LoadSyntaxTree();
                LOGGER.Write("Syntax Tree Loaded.");
            }

            List<CompletionResult> suggestions = new();

            // TODO Resolve where we are in the token tree. First token (Link search for exact matches). -> Next options
            // Search next options -> if no returns then previous search has the options available.

            CompletionResult response = new(
                WordToComplete + "a",
                WordToComplete + "a",
                CompletionResultType.ParameterName, //Need to change this for ast.
                "ToolTip");

            CompletionResult response2 = new(
                WordToComplete + "b",
                WordToComplete + "b",
                CompletionResultType.ParameterName, //Need to change this for ast.
                "ToolTip");

            suggestions.Add(response);
            suggestions.Add(response2);

            LOGGER.Write($"Providing suggestions...{WordToComplete}a, b.");

            return suggestions;
        }

    }
}
