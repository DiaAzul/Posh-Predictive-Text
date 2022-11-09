
namespace PoshPredictiveText.SyntaxTrees
{
    using Parquet;
    using Parquet.Data.Rows;
    using PoshPredictiveText.SemanticParser;
    using PoshPredictiveText.SyntaxTreeSpecs;
    using PoshPredictiveText.SyntaxTreeHelpers;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Threading.Tasks;
    /// <summary>
    /// Each command has a syntax tree which sets out the possible combination of tokens
    /// on the command line. The trees are strored as XML resource files embeded within
    /// the application. These are loaded, parsed and converted to a list of Syntax items
    /// when a command requires tab-completions.
    /// </summary>
    internal class SyntaxTree
    {
        private readonly List<SyntaxItem> syntaxItems = new();

        private readonly string syntaxTreeName;

        /// <summary>
        /// Create a new syntax tree.
        /// 
        /// Load configuration from file into the tree.
        /// </summary>
        /// <param name="name">Syntax tree name.</param>
        internal SyntaxTree(string name)
        {
            LOGGER.Write($"SYNTAX TREE: Loading {name}. Items={syntaxItems.Count}.");
            syntaxTreeName = name;
            try
            {
                ThreadAffinitiveSynchronizationContext.RunSynchronized(LoadAsync);
                LOGGER.Write($"SYNTAX TREE: Loaded {name}. Items={syntaxItems.Count}.");
            }
            catch (Exception ex)
            {
                LOGGER.Write("SYNTAX TREE: Error running LoadAsync synchronously.");
                LOGGER.Write(ex.Message);
            }
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
                                    .Where(syntaxItem => syntaxItem.Path == commandPath)
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
                .Where(syntaxItem => (syntaxItem.Path == commandPath)
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
                .Where(syntaxItem => (syntaxItem.Path == commandPath)
                                        && syntaxItem.IsCommand)
                .Count();
        }

        internal List<SyntaxItem> ParametersAndOptions(string commandPath)
        {
            return syntaxItems
                .Where(syntaxItem => (syntaxItem.Path == commandPath)
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
                .Where(syntaxItem => syntaxItem.Name == lastParameter
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
                             && syntaxItem.Name is not null
                             && syntaxItem.Name.StartsWith(wordToComplete)
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
        private async Task LoadAsync()
        {
            LOGGER.Write("SYNTAX TREE: Async loading start.");
            Table? syntaxTreeParquetTable;
            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                string? resourcePath = SyntaxTreesConfig.ParquetDefinition(syntaxTreeName);
                LOGGER.Write($"SYNTAX TREE: Resource path: {resourcePath}");
                if (resourcePath is null)
                    throw new SyntaxTreeException($"Definition file not found for {syntaxTreeName}.");

                Stream? resourceStream = assembly.GetManifestResourceStream(resourcePath);
                if (resourceStream is null) throw new SyntaxTreeException($"File stream could not be opened {syntaxTreeName}.");

                using ParquetReader parquetReader = await ParquetReader.CreateAsync(resourceStream);
                syntaxTreeParquetTable = await parquetReader.ReadAsTableAsync();
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

            if (syntaxTreeParquetTable is null) throw new SyntaxTreeException($"Unable to load Parquet to table: {syntaxTreeName}.");

            // Check contents of the table (format should be tested in testing).
            // Need to convert to list, with 
            try
            {
                foreach (var row in syntaxTreeParquetTable)
                {
                    List<string> sets = new();
                    if (!row.IsNullAt(5))
                    {
                        var set_object = row[5];
                        if (set_object is IEnumerable enumerable)
                        {
                            var enumerator = enumerable.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                sets.Add((string)enumerator.Current);
                            }
                        }
                    };

                    List<string> choices = new();
                    if (!row.IsNullAt(8))
                    {
                        var choice_object = row[8];
                        if (choice_object is IEnumerable enumerable)
                        {
                            var enumerator = enumerable.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                choices.Add((string)enumerator.Current);
                            }
                        }
                    };

                    // Build list for Set, Choices (if choices exists).
                    if (Enum.TryParse((string)row[2], out SyntaxItemType type))
                    {
                        SyntaxItem syntaxItem = new()
                        {
                            Command = (string)row[0],
                            Path = (string)row[1],
                            Type = type,
                            Name = (string)row[3],
                            Alias = (string?)row[4],
                            Sets = sets,
                            MaxUses = (int?)row[6],
                            Value = (string?)row[7],
                            Choices = choices,
                            MinCount = (int?)row[9],
                            MaxCount = (int?)row[10],
                            ToolTip = (string?)row[11],
                        };
                        syntaxItems.Add(syntaxItem);
                    }
                    else
                    {
                        throw new SyntaxTreeException("Error parsing type in Syntax Tree");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SyntaxTreeException($"Unable to parse {syntaxTreeName}.", ex);
            }
        }
    }
}
