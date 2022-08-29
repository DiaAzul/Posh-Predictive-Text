
namespace ResolveArgument
{
    using System.Management.Automation;
    using System.Reflection;
    using System.Resources;
    using System.Xml.Linq;

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
            get { return Type == "OPT";  }
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
                    .Select(item => item.Command)
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
                var resourcePath = SyntaxTreesConfig.Definition(syntaxTreeName);
                if (resourcePath is null)
                    throw new SyntaxTreeException($"Definition file not found for {syntaxTreeName}.");
                var resourceStream = assembly.GetManifestResourceStream(resourcePath);

                if (resourceStream is null) throw new SyntaxTreeException($"File stream could not be opened {syntaxTreeName}.");

                using StreamReader reader = new(resourceStream);
                var xmlDoc = reader.ReadToEnd();
                syntaxTreeInputFile = XDocument.Parse(xmlDoc);

            }
            catch (FileNotFoundException ex)
            {
                throw new SyntaxTreeException($"Definition file not found for {syntaxTreeName}.", ex);
            }
            catch (BadImageFormatException ex)
            {
                throw new SyntaxTreeException($"Not a valid assembly for {syntaxTreeName}.", ex);
            }
            catch (FileLoadException ex)
            {
                throw new SyntaxTreeException($"Definition file was found, could not load {syntaxTreeName}.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new SyntaxTreeException($"Definition file stream could not be opened {syntaxTreeName}.", ex);
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
                                              Command = AsString(item.Element("CMD")),
                                              CommandPath = AsString(item.Element("PATH")),
                                              Type = AsString(item.Element("TYP")),
                                              Argument = AsNullableString(item.Element("ARG")),
                                              Alias = AsNullableString(item.Element("AL")),
                                              MultipleUse = AsBool(item.Element("MU")),
                                              Parameter = AsNullableString(item.Element("PRM")),
                                              MultipleParameterValues = AsNullableBool(item.Element("MP")),
                                              ToolTip = AsNullableString(item.Element("TT"))
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
            return element?.Value.ToString() ?? "";
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
            if (toolTipRef == null) return "";

            string? baseName = SyntaxTreesConfig.ToolTips(syntaxTreeName);
            if (baseName == null) return "";

            string toolTip;
            try
            {
                var resourceManager = new ResourceManager(baseName, Assembly.GetExecutingAssembly());
                toolTip = resourceManager.GetString(toolTipRef) ?? "";
            }
            catch (ArgumentNullException)
            {
                toolTip = "";
            }

            return toolTip;
        }
    }
}
