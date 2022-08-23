

namespace Resolve_Argument.Helpers
{
    using System;

    /// <summary>
    /// Conda helpers to provide parameter values completions.
    /// </summary>
    internal static class CondaHelpers
    {
        // Constants defined within conda application.
        const string CONDA_ROOT = "_CONDA_ROOT";
        const string ROOT_ENV_NAME = "base";

        /// <summary>
        /// Get a list of conda environments.
        /// </summary>
        /// <returns>List of conda environments.</returns>
        internal static List<string> GetEnvironments()
        {
            List<string> condaEnvironments = new()
            {
                ROOT_ENV_NAME
            };

            // Get the path to the conda root.
            string? conda_root = Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process);
            if (conda_root is not null)
            {
                condaEnvironments.AddRange(
                    Directory.GetDirectories(conda_root + @"\envs\")
                    .Select(d => new DirectoryInfo(d).Name));
            }
            return condaEnvironments;
        }
    }
}
