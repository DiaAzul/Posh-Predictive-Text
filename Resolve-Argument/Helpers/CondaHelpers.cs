

namespace Resolve_Argument.Helpers
{
    using ResolveArgument;

    /// <summary>
    /// Helper classes to provide additional completion suggestion for conda commands.
    /// </summary>
    internal static class CondaHelpers
    {
        /// <summary>
        /// Get a list of conda environments available on this computer.
        /// </summary>
        /// <returns>List of conda environments.</returns>
        internal static List<string> GetEnvironments()
        {
            List<string> environments = new();
            var psOutput = CommandShell.QueryPowerShell("conda", new List<string>() { "env", "list" });
            foreach (var line in psOutput)
            {
                if (line.Length >  0 && line[0] != '#')
                {
                    var env = line.Split(' ');
                    if (env.Length >1)
                    {
                        environments.Add(env[0]);
                    }
                }
            }
            return environments;
        }
    }
}
