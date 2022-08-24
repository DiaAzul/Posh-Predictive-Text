// TODO [ ][SYNTAXTREES] Implement configuration file to centralise constant for commands (e.g. resource locations).
// TODO [ ][SYNTAXTREES] Support aliasing commands and resource files.
// TODO [ ][SYNTAXTREES] Remove hard coded references to conda resources.

namespace ResolveArgument
{
    using System.Reflection;
    using System.Xml.Linq;
    using System.Management.Automation;
    using ResolveArgument;
    using System.Xml;

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

    /// <summary>
    /// Record within the command syntax tree.
    /// 
    /// Used to enumerate query results on the XML syntax tree.
    /// </summary>
    internal record SyntaxItem
    {
        internal string command = default!;
        internal string commandPath = default!;
        internal string type = default!;
        internal string argument = default!;
        internal string? alias;
        internal bool multipleUse = default!;
        internal string? parameter;
        internal bool? multipleParameters;
        internal string? toolTip;

        internal string AsString()
        {
            return $"{command}, {commandPath}, {type}, {argument}, {alias}, {multipleUse}, {parameter}, {multipleParameters}, {toolTip}";
        }

        /// <summary>
        /// Property return the result type for the Syntax item.
        /// </summary>
        internal CompletionResultType ResultType
        {
            get
            {
                return type switch
                {
                    "CMD" => CompletionResultType.Command,
                    "OPT" => CompletionResultType.ParameterName,
                    "PRM" => CompletionResultType.ParameterName,
                    "POS" => CompletionResultType.ParameterValue,
                    _ => CompletionResultType.ParameterValue,
                };
            }
        }
    }

    // [ ] [SYNTAXTREE] Add documentation to SyntaxTreeClass.
    internal static class SyntaxTrees
    {
        /// <summary>
        /// Each command has a syntax tree which sets out the possible combination of tokens
        /// on the command line. The trees are strored as XML resource files embeded within
        /// the application. These are loaded, parsed and converted to a list of Syntax items
        /// when a command requires tab-completions.
        /// </summary>
        private static readonly Dictionary<string, List<SyntaxItem>> syntaxTrees = new();

        /// <summary>
        /// Test that a syntax tree is loaded.
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to test.</param>
        /// <returns>True if syntax tree is loaded.</returns>  
        internal static bool Exists(string syntaxTreeName)
        {
            LOGGER.Write("Testing whether tree exists.");
            LOGGER.Write(syntaxTrees.ContainsKey(syntaxTreeName).ToString());
            return syntaxTrees.ContainsKey(syntaxTreeName);
        }

        internal static int Count(string syntaxTreeName)
        {
            return syntaxTrees[syntaxTreeName].Count;
        }

        internal static List<SyntaxItem> Get(string syntaxTreeName)
        {
            List<SyntaxItem> result = new();
            try
            {
                result = syntaxTrees[syntaxTreeName];

            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"Syntax Tree {syntaxTreeName} not found when getting list of tokens.");
            }

            return result;
        }

        internal static List<string> UniqueCommands(string syntaxTreeName)
        {
            List<string> uniqueCommands = new();

            if (Exists(syntaxTreeName))
            {
                uniqueCommands = syntaxTrees[syntaxTreeName]
                    .Select(item => item.command)
                    .Distinct()
                    .ToList();
            }
            else
            {
                LOGGER.Write($"Syntax Tree {syntaxTreeName} not found when getting unique commands.");
            }

            return uniqueCommands;
        }

        /// <summary>
        /// Loads the syntax tree for a named command into the dictionary of syntax trees.
        /// 
        /// The method reads the XML file embeded within the application, parses it
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to load.</param>
        internal static void Load(string syntaxTreeName)
        {
            XDocument? syntaxTreeInputFile = null;

            // Load XML File from assembly into XDocument.
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                // TODO [ ][SYNTAXTREES] Change syntax tree xml source according to the syntax tree name.
                var resourceStream = assembly.GetManifestResourceStream("Resolve_Argument.SyntaxTreeSpecs.CondaSyntaxTree.xml");

                if (resourceStream is null) throw new SyntaxTreeException($"File stream could not be opened {syntaxTreeName}.");

                using StreamReader reader = new(resourceStream);
                var xmlDoc = reader.ReadToEnd();
                syntaxTreeInputFile = XDocument.Parse(xmlDoc);

            }
            catch (FileNotFoundException ex)
            {
                throw new SyntaxTreeException($"File not found for {syntaxTreeName}.", ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new SyntaxTreeException($"Not a valid assembly for {syntaxTreeName}.", ex);
            }
            catch (FileLoadException ex)
            {
                throw new SyntaxTreeException($"File was found, could not load {syntaxTreeName}.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new SyntaxTreeException($"File stream could not be opened {syntaxTreeName}.", ex);
            }

            // Parse the XML document into a List.
            List<SyntaxItem> syntaxTree = new();
            XElement? root = syntaxTreeInputFile?.Root;

            try
            {
                if (root is not null)
                {
                    var syntaxTreeQuery = from item in root.Elements("item")
                                          select new SyntaxItem
                                          {
                                              command = AsString(item.Element("CMD")),
                                              commandPath = AsString(item.Element("PATH")),
                                              type = AsString(item.Element("TYP")),
                                              argument = AsNullableString(item.Element("ARG")),
                                              alias = AsNullableString(item.Element("AL")),
                                              multipleUse = AsBool(item.Element("MU")),
                                              parameter = AsNullableString(item.Element("PRM")),
                                              multipleParameters = AsNullableBool(item.Element("MP")),
                                              toolTip = AsNullableString(item.Element("TT"))
                                          };
                    if (syntaxTreeQuery is not null)
                    {

                        syntaxTree = syntaxTreeQuery.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SyntaxTreeException($"Unable to parse {syntaxTreeName}.", ex);
            }
            // If the syntax tree loaded successfully, add to the dictionary.
            if (syntaxTree.Any())
            {
                syntaxTrees[syntaxTreeName] = syntaxTree;
#if DEBUG
                LOGGER.Write($"Syntax tree {syntaxTreeName} saved.");
#endif
            }
            else
            {
                syntaxTrees.Remove(syntaxTreeName);
#if DEBUG
                LOGGER.Write($"Syntax tree {syntaxTreeName} not saved.");
#endif
            }
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a <c>string</c>.
        /// 
        /// Null values are converted to an empty string.
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <returns>Contents of node as string.</returns>
        internal static string AsString(XElement? element)
        {
            return  element?.Value.ToString() ?? "";
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a nullable <c>string</c>.
        /// 
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <returns>Contents of node as string.</returns>
        internal static string? AsNullableString(XElement? element)
        {
            return element?.Value.ToString();
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a <c>bool</c>.
        /// 
        /// <para>The method returns true if the content of the node matches the test pattern. The
        /// default matching pattern is <c>TRUE</c>.</para>
        /// 
        /// <para>If the node is null then the method return the <c>false </c> value.</para>
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <param name="trueValue">Test pattern for true value. Default <c>TRUE</c>.</param>
        /// <returns>True when the contents of the node match the test pattern. <c>false</c> if the node is null.</returns>
        internal static bool AsBool(XElement? element, string trueValue = "TRUE")
        {
            return element is not null && (element?.Value.ToString() ?? "") == trueValue;
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a nullable <c>bool</c>.
        /// 
        /// <para>The method returns true if the content of the node matches the test pattern. The
        /// default matching pattern is <c>TRUE</c>.</para>
        /// 
        /// <para>If the node is null then the method returns a null bool.</para>
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <param name="trueValue">Test pattern for true value. Default <c>TRUE</c>.</param>
        /// <returns>True when the contents of the node match the test pattern. Null if the node is null.</returns>
        internal static bool? AsNullableBool(XElement? element, string trueValue = "TRUE")
        {
            return element is not null ? (element?.Value.ToString() ?? "") == trueValue : null;
        }

        /// <summary>
        /// Gets the display string for a tooltip reference.
        /// </summary>
        /// <param name="syntaxTreeName">Syntax tree from which tooltip required.</param>
        /// <param name="toolTipRef">Tooltip reference used to identify display string.</param>
        /// <returns>Tooltip display text.</returns>
        internal static string Tooltip(string syntaxTreeName, string? toolTipRef)
        {
            // TODO [ ][SYNTAXTREES] Toolips method to get UI string. Add validation and remove hard coded reference.
            string? toolTip = null;
            if (toolTipRef is not null)
            {
                // TODO [ ][SYNTAXTREES] Try..Catch around tooltip resource manager.
                toolTip = Resolve_Argument.SyntaxTreeSpecs.CondaToolTips.ResourceManager.GetString(toolTipRef);
            }
            toolTip ??= "";
            return toolTip;
        }
    }
}
