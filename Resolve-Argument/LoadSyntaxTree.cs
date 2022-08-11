// TODO Implement configuration file to centralise constant for commands (e.g. resource locations).
// TODO Support aliasing commands and resource files.
// TODO Remove hard coded references to conda resources.

namespace ResolveArgument
{
    using System.Reflection;
    using System.Xml.Linq;
    internal class SyntaxTree
    {
        /// <summary>
        /// Loads the syntax tree for a named command into the dictionary of syntax trees.
        /// 
        /// The method reads the XML file embeded within the application, parses it
        /// </summary>
        /// <param name="syntaxTreeName">Name of syntax tree to load.</param>
        internal static List<SyntaxItem> Load(string syntaxTreeName)
        {
            // Load XML File from assembly into XDocument.
            XDocument? xSyntaxTree = null;

            Assembly assembly = Assembly.GetExecutingAssembly();
            try
            {
                // TODO: This needs to vary according to the syntax tree to be loaded.
                var resourceStream = assembly.GetManifestResourceStream("Resolve_Argument.SyntaxTrees.CondaSyntaxTree.xml");

                if (resourceStream != null)
                {
                    using StreamReader reader = new(resourceStream);
                    var xmlDoc = reader.ReadToEnd();
                    xSyntaxTree = XDocument.Parse(xmlDoc);
                }
            }
            catch (FileNotFoundException)
            {
                LOGGER.Write("File not found.");
            }
            catch (BadImageFormatException)
            {
                LOGGER.Write("File wrong format.");
            }
            catch (FileLoadException)
            {
                LOGGER.Write("File was found, could not load.");
            }

            // Parse the XML document into a List.
            List<SyntaxItem> syntaxTree = new();
            IEnumerable<SyntaxItem>? syntaxTreeQuery = null;
            XElement? root = xSyntaxTree?.Root;

            if (root != null)
            {
                try
                {
                    syntaxTreeQuery = from item in root.Elements("item")
                                      select new SyntaxItem
                                      {
                                          command = AsString(item.Element("CMD")),
                                          commandPath = AsString(item.Element("PATH")),
                                          type = AsString(item.Element("TYPE")),
                                          argument = AsNullableString(item.Element("ARG")),
                                          alias = AsNullableString(item.Element("AL")),
                                          multipleUse = AsNullableBool(item.Element("MU")),
                                          parameter = AsNullableString(item.Element("PRM")),
                                          multipleParameters = AsNullableBool(item.Element("MP")),
                                          toolTip = AsNullableString(item.Element("TT"))
                                      };
                }
                catch (Exception e)
                {
                    LOGGER.Write(e.ToString());
                }
            }

            if (syntaxTreeQuery != null)
            {
                try
                {
                    syntaxTree = syntaxTreeQuery.ToList();
                }
                catch (Exception e)
                {
                    LOGGER.Write(e.ToString());
                }
            }

            return syntaxTree;
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
            string elementAsString = "";
            if (element != null)
            {
                elementAsString = (string)element;
            }

            return elementAsString;
        }

        /// <summary>
        /// Convert nullable <c>XElement</c> node in an XML tree to a nullable <c>string</c>.
        /// 
        /// Null values are converted to an empty string.
        /// </summary>
        /// <param name="element">Node in XML tree.</param>
        /// <returns>Contents of node as string.</returns>
        internal static string? AsNullableString(XElement? element)
        {
            string? elementAsString = null;
            if (element != null)
            {
                elementAsString = (string)element;
            }

            return elementAsString;
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
            bool? elementAsNullableBool = null;
            if (element != null)
            {
                elementAsNullableBool = (string)element == trueValue;
            }

            return elementAsNullableBool;

        }

        /// <summary>
        /// Gets the display string for a tooltip reference.
        /// </summary>
        /// <param name="syntaxTreeName">Syntax tree from which tooltip required.</param>
        /// <param name="toolTipRef">Tooltip reference used to identify display string.</param>
        /// <returns>Tooltip display text.</returns>
        internal static string Tooltip(string syntaxTreeName, string? toolTipRef)
        {
            // TODO: This is a bodge, needs tidying up, error checking.
            string? toolTip = null;
            if (!(toolTipRef == null))
            {
                toolTip = Resolve_Argument.SyntaxTrees.CondaToolTips.ResourceManager.GetString(toolTipRef);
            }
            toolTip ??= "";
            return toolTip;
        }
    }
}
