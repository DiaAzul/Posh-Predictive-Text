
using PoshPredictiveText.SyntaxTreeSpecs;

namespace PoshPredictiveText
{
    /// <summary>
    /// Maintains a database of syntax trees for predicting text completion
    /// 
    /// Each command has a syntax tree which sets out the possible combination of tokens
    /// on the command line. The trees are strored as XML resource files embeded within
    /// the application. These are loaded, parsed and converted to a list of Syntax items
    /// when a command requires tab-completions.
    /// </summary>
    internal static class SyntaxTrees
    {
        private static readonly Dictionary<string, SyntaxTree> syntaxTrees = new();

        /// <summary>
        /// Get the syntax tree.
        /// 
        /// Load the tree if it is available and is not loaded.
        /// </summary>
        /// <param name="syntaxTreeName">Name of the syntax tree.</param>
        /// <returns></returns>
        internal static SyntaxTree? Tree(string? syntaxTreeName)
        {
            if (syntaxTreeName is null) return null;
            if (syntaxTrees.ContainsKey(syntaxTreeName))
            {
                return syntaxTrees[syntaxTreeName];
            }
            if (SyntaxTreesConfig.IsSupportedCommand(syntaxTreeName))
            {
                Add(syntaxTreeName);
                return syntaxTrees[syntaxTreeName];
            }
            return null;
        }

        /// <summary>
        /// Load and add a syntax tree to the database of syntax trees.
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree</param>
        /// <param name="syntaxTree">Syntax tree</param>
        /// <exception cref="SyntaxTreeException">Thrown if the syntax tree already
        /// exists in the database.</exception>
        internal static void Add(string syntaxTreeName)
        {
            if (syntaxTrees.ContainsKey(syntaxTreeName))
                throw new SyntaxTreeException($"Syntax Tree {syntaxTreeName} already exists.");

            try
            {
                syntaxTrees[syntaxTreeName] = new SyntaxTree(syntaxTreeName);
            }
            catch (SyntaxTreeException ex)
            {
                throw new SyntaxTreeException($"Unable to add {syntaxTreeName} to syntax tree database.", ex);
            }
        }

        /// <summary>
        /// Add a syntax tree to the database of syntax trees.
        /// </summary>
        /// <param name="syntaxTree">Syntax tree</param>
        internal static void Add(SyntaxTree syntaxTree)
        {
            if (syntaxTrees.ContainsKey(syntaxTree.Name))
                throw new SyntaxTreeException($"Syntax Tree {syntaxTree.Name} already exists.");
            syntaxTrees[syntaxTree.Name] = syntaxTree;
        }

        /// <summary>
        /// Removes a syntax tree from the database.
        /// </summary>
        /// <param name="syntaxTreeName">Syntax tree name to remove from database.</param>
        internal static void Remove(string syntaxTreeName)
        {
            if (syntaxTrees.ContainsKey(syntaxTreeName))
            {
                syntaxTrees.Remove(syntaxTreeName);
            }
        }

        /// <summary>
        /// Test that a syntax tree is loaded.
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to test.</param>
        /// <returns>True if syntax tree is loaded.</returns>  
        internal static bool Exists(string syntaxTreeName)
        {
            return syntaxTrees.ContainsKey(syntaxTreeName);
        }
    }
}
