

namespace ResolveArgument
{
    internal record ConfigItem
    {
        internal string? Definition { get; init; }
        internal string? Tooltip { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class SyntaxTreesConfig
    {
        internal const string DEFINITION_PATH = "Resolve_Argument.SyntaxTreeSpecs.";
        internal const string RESOURCE_PATH = ""; 

        internal static readonly Dictionary<string, ConfigItem> config = new() {
            {"conda",  new ConfigItem {
                Definition = "CondaSyntaxTree.xml",
                Tooltip = "CondaToolTips" } },
            {"mamba", new ConfigItem {
                Definition = "",
                Tooltip = "" } }
        };
        internal static string? Definition(string syntaxTreeName)
        {
            string? returnValue = null;
            try
            {
                returnValue = DEFINITION_PATH + config[syntaxTreeName].Definition;
            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"No config for {syntaxTreeName}");
            }
            return returnValue;
        }
    }
}
