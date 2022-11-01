
namespace PoshPredictiveText
{

    using PoshPredictiveText.SyntaxTreeSpecs;
    using System.Reflection;
    using System.Resources;
    using System.Xml.Linq;
    /// <summary>
    /// Each command has a syntax tree which sets out the possible combination of tokens
    /// on the command line. The trees are strored as XML resource files embeded within
    /// the application. These are loaded, parsed and converted to a list of Syntax items
    /// when a command requires tab-completions.
    /// </summary>
    internal class SyntaxTree
    {

        private List<SyntaxItem> syntaxItems = new();

        private readonly string syntaxTreeName;

        /// <summary>
        /// Create a new syntax tree.
        /// 
        /// Load configuration from file into the tree.
        /// </summary>
        /// <param name="name">Syntax tree name.</param>
        internal SyntaxTree(string name)
        {
            syntaxTreeName = name;
            Load();
        }

        /// <summary>
        /// Create a new syntax tree and load items from list.
        /// </summary>
        /// <param name="name">Syntax tree name.</param>
        /// <param name="newSyntaxItems">List of syntax items.</param>
        internal SyntaxTree(string name, List<SyntaxItem> newSyntaxItems)
        {
            syntaxTreeName = name;
            syntaxItems = newSyntaxItems;
        }

        /// <summary>
        /// Name of the syntax tree.
        /// </summary>
        internal string Name => syntaxTreeName;

        /// <summary>
        /// Count of items in the syntax tree.
        /// </summary>
        internal int Count => syntaxItems.Count;

        /// <summary>
        /// The list of items in the syntax tree.
        /// </summary>
        internal List<SyntaxItem> GetItems => syntaxItems;

        /// <summary>
        /// A list of unique commands in the syntax tree.
        /// </summary>
        internal List<string> UniqueCommands => syntaxItems
                                                .Select(syntaxItem => syntaxItem.Command)
                                                .Distinct()
                                                .ToList();

        private string? cachedCommandPath;
        private List<SyntaxItem>? cachedFilteredItems;
        /// <summary>
        /// Gets a list of syntax items filter by command path.
        /// </summary>
        /// <param name="commandPath">Command path to filter syntax items.</param>
        /// <returns>Filter list of syntax items.</returns>
        /// <remarks>This function is often called several times with the same command path,
        /// the last result is cached to minimise the number of times the linq query is called.
        /// </remarks>
        internal List<SyntaxItem> FilteredByCommandPath(string commandPath)
        {
            if (cachedFilteredItems is not null && cachedCommandPath == commandPath)
                return cachedFilteredItems;

            cachedCommandPath = commandPath;
            cachedFilteredItems = syntaxItems
                                    .Where(syntaxItem => syntaxItem.CommandPath == commandPath)
                                    .ToList();

            return cachedFilteredItems;
        }

        /// <summary>
        /// Get list of subcommands filtered by command path.
        /// </summary>
        /// <param name="commandPath">Command path.</param>
        /// <returns>Subcommands available at command path.</returns>
        internal List<SyntaxItem> SubCommands(string commandPath)
        {
            return syntaxItems
                .Where(syntaxItem => (syntaxItem.CommandPath == commandPath)
                                        && syntaxItem.IsCommand)
                .ToList();
        }

        /// <summary>
        /// Count of subcommands filtered by command path.
        /// </summary>
        /// <param name="commandPath">Command path.</param>
        /// <returns>Count of subcommands available at command path.</returns>
        internal int CountOfSubCommands(string commandPath)
        {
            return syntaxItems
                .Where(syntaxItem => (syntaxItem.CommandPath == commandPath)
                                        && syntaxItem.IsCommand)
                .Count();
        }

        internal List<SyntaxItem> ParametersAndOptions(string commandPath)
        {
            return syntaxItems
                .Where(syntaxItem => (syntaxItem.CommandPath == commandPath)
                                        && (syntaxItem.IsParameter || syntaxItem.IsOptionParameter))
                .ToList();
        }

        /// <summary>
        /// Returns a syntax item for the last entered parameter
        /// prior to the current token position. The last entered
        /// parameter syntax item holds details for the number of
        /// parameter values that can be entered after a parameter.
        /// </summary>
        /// <param name="commandPath">Command path</param>
        /// <param name="lastParameter"></param>
        /// <returns>Parameter syntax items.</returns>
        internal List<SyntaxItem> ParameterSyntaxItems(
                                                string commandPath,
                                                string lastParameter)
        {
            return FilteredByCommandPath(commandPath)
                .Where(syntaxItem => syntaxItem.Argument == lastParameter
                        | (syntaxItem.HasAlias && syntaxItem.Alias == lastParameter))
                .ToList();
        }

        /// <summary>
        /// List of positional value syntax items at command path
        /// </summary>
        /// <param name="commandPath">Command path.</param>
        /// <returns>List of positional value syntax items.</returns>
        internal List<SyntaxItem> PositionalValues(string commandPath)
        {
            return FilteredByCommandPath(commandPath)
                        .Where(syntaxItem => syntaxItem.IsPositionalParameter)
                        .ToList();
        }

        // TODO [HIGH][TOKENISER] Can this be moved to the tokeniser?
        /// <summary>
        /// Returns a list of available suggestions for the entered text.
        /// </summary>
        /// <param name="commandPath">Command path</param>
        /// <param name="commandComplete">True if the command is complete.</param>
        /// <param name="enteredTokens">Tokens entered on the command line.</param>
        /// <param name="wordToComplete">Word to complete.</param>
        /// <returns></returns>
        internal List<SyntaxItem> AvailableOptions(
            string commandPath,
            bool commandComplete,
            Tokeniser enteredTokens,
            string wordToComplete)
        {
            return FilteredByCommandPath(commandPath)
                     .Where(syntaxItem =>
                             !(syntaxItem.IsCommand && commandComplete)
                             && syntaxItem.Argument is not null
                             && syntaxItem.Argument.StartsWith(wordToComplete)
                             && enteredTokens.CanUse(syntaxItem))
                     .ToList();
        }

        /// <summary>
        /// Gets the display string for a tooltip reference.
        /// </summary>
        /// <param name="syntaxTreeName">Syntax tree from which tooltip required.</param>
        /// <param name="toolTipRef">Tooltip reference used to identify display string.</param>
        /// <returns>Tooltip display text.</returns>
        internal string Tooltip(string? toolTipRef)
        {
            if (toolTipRef is null) return "";

            string? baseName = SyntaxTreesConfig.ToolTips(syntaxTreeName);
            if (baseName == null) return "";

            string toolTip;
            try
            {
                var resourceManager = new ResourceManager(
                                            baseName,
                                            Assembly.GetExecutingAssembly());
                toolTip = resourceManager.GetString(toolTipRef) ?? "";
            }
            catch (ArgumentNullException)
            {
                toolTip = "";
            }

            return toolTip;
        }


        /// <summary>
        /// Loads the syntax tree for a named command into the dictionary of syntax trees
        /// from a Parquet file.
        /// </summary>
        private void LoadParquet()
        {
            // TODO [HIGH][SYNTAXTREE] Add load parquet file.
        }

        /// <summary>
        /// Loads the syntax tree for a named command into the dictionary of syntax trees.
        /// 
        /// The method reads the XML file embeded within the application, parses it
        /// </summary>
        private void Load()
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

                        syntaxItems = syntaxTreeQuery.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SyntaxTreeException($"Unable to parse {syntaxTreeName}.", ex);
            }
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a <c>string</c>.
        /// 
        /// Null values are converted to an empty string.
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <returns>Contents of node as string.</returns>
        private static string AsString(XElement? element)
        {
            return element?.Value.ToString() ?? "";
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a nullable <c>string</c>.
        /// 
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <returns>Contents of node as string.</returns>
        private static string? AsNullableString(XElement? element)
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
        private static bool AsBool(XElement? element, string trueValue = "TRUE")
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
        private static bool? AsNullableBool(XElement? element, string trueValue = "TRUE")
        {
            return element is not null ? (element?.Value.ToString() ?? "") == trueValue : null;
        }
    }
}
