// TODO [ ] Conda config -> Missing a POSITIONAL parameter in ?? conda env remove POSITIONAL (env or path).

namespace ResolveArgument
{
    internal record ConfigItem
    {
        internal string? Definition { get; init; }
        internal string? ToolTips { get; init; }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class SyntaxTreesConfig
    {
        internal const string RESOURCE_ROOT = "ResolveArgument.SyntaxTreeSpecs.";

        internal static readonly Dictionary<string, ConfigItem> config = new() {
            {"conda",  new ConfigItem {
                Definition = "CondaSyntaxTree.xml",
                ToolTips = "CondaToolTips" } },
            {"mamba", new ConfigItem {
                Definition = "",
                ToolTips = "" } }
        };
        internal static string? Definition(string syntaxTreeName)
        {
            string? returnValue = null;
            try
            {
                returnValue = RESOURCE_ROOT + config[syntaxTreeName].Definition;
            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"No config for {syntaxTreeName}");
            }
            return returnValue;
        }
        internal static string? ToolTips(string syntaxTreeName)
        {
            string? returnValue = null;
            try
            {
                returnValue = RESOURCE_ROOT + config[syntaxTreeName].ToolTips;
            }
            catch (KeyNotFoundException)
            {
                LOGGER.Write($"No config for {syntaxTreeName}");
            }
            return returnValue;
        }

    }
}
