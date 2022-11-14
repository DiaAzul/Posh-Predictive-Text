

namespace PoshPredictiveText.SyntaxTrees
{
    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, SyntaxTree> syntaxTrees = new();

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
                LOGGER.Write($"SYNTAX TREES: Returning existing {syntaxTreeName} syntax tree.");
                return syntaxTrees[syntaxTreeName];
            }
            if (SyntaxTreesConfig.IsSupportedCommand(syntaxTreeName))
            {
                LOGGER.Write($"SYNTAX TREES: Adding {syntaxTreeName} to parsed syntax trees.");

                SyntaxTree syntaxTree = new(syntaxTreeName);
                syntaxTrees.TryAdd(syntaxTreeName, syntaxTree);
                return syntaxTree;
            }
            throw new SyntaxTreeException($"SYNTAX TREES: Syntax tree {syntaxTreeName} didn't exist and couldn't be loaded.");
        }

        /// <summary>
        /// Add a syntax tree to the database of syntax trees.
        /// </summary>
        /// <param name="syntaxTree">Syntax tree</param>
        internal static void Add(SyntaxTree syntaxTree)
        {
            if(!syntaxTrees.TryAdd(syntaxTree.Name, syntaxTree))
            {
                throw new SyntaxTreeException($"SYNTAX TREES: Syntax Tree {syntaxTree.Name} already exists.");
            }
        }

        /// <summary>
        /// Removes a syntax tree from the database.
        /// </summary>
        /// <param name="syntaxTreeName">Syntax tree name to remove from database.</param>
        internal static void Remove(string syntaxTreeName)
        {
            if (syntaxTrees.ContainsKey(syntaxTreeName))
            {
                syntaxTrees.TryRemove(syntaxTreeName, out _);
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
