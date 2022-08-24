

namespace ResolveArgument
{
    using System;
    using System.Net.NetworkInformation;
    using System.Reflection;


    /// <summary>
    /// Attribute declaring the Parameter value that the method provides
    /// suggestions for.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterValueAttribute : Attribute
    {
        internal string name { get; private set; }

        /// <summary>
        /// Save the name of the parameter.
        /// </summary>
        /// <param name="name">Name of the Parameter Value</param>
        public ParameterValueAttribute(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// Conda helpers to provide parameter values completions.
    /// </summary>
    internal static class CondaHelpers
    {
        // Constants defined within conda application.
        const string CONDA_ROOT = "_CONDA_ROOT";
        const string ROOT_ENV_NAME = "base";

        /// <summary>
        /// Returns suggested parameter values for a given named parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>Suggested parameter values.</returns>
        /// <remarks>This method uses internal reflection to look up the
        /// appropriate method to be called. If the method is not found an
        /// empty suggestions list is returned.</remarks>
        internal static List<string> GetParamaterValues(string parameterName)
        {
            List<string> results = new();

            var methods = typeof(CondaHelpers).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(method =>
                method.GetCustomAttributes(typeof(ParameterValueAttribute), false).FirstOrDefault() != null
                && method.GetCustomAttributes<ParameterValueAttribute>()
                .ToList()
                .Contains(new ParameterValueAttribute(parameterName)));

            switch (methods.Count())
            {
                case 1:
                    {
                        try
                        {
                            var methodResult = methods.First().Invoke(null, null);
                            if (methodResult != null)
                            {
                                results = (List<string>)methodResult;
                            }
                        }
                        catch (Exception ex) 
                        {
                            throw new SyntaxTreeException($"Exception when invoking {parameterName} helper.", ex);
                        }
                        break;
                    }
                case > 1:
                    {
                        throw new SyntaxTreeException($"Multiple methods defined for {parameterName}.");
                    }
                default: { break; }
            }

            return results;
        }

        /// <summary>
        /// Return an empty list of suggestions for ParameterValues
        /// that are not implemented.
        /// </summary>
        /// <returns>Empty list of suggestions.</returns>
        [ParameterValue("CHANNEL")]
        [ParameterValue("CWD")]
        [ParameterValue("DESCRIBE")]
        [ParameterValue("ENV")]
        [ParameterValue("ENV_KEY")]
        [ParameterValue("ENV_KEY_VALUE")]
        [ParameterValue("EXECUTABLE_CALL")]
        [ParameterValue("FILE")]
        [ParameterValue("KEY")]
        [ParameterValue("KEY_TO_REMOVE")]
        [ParameterValue("KEY_VALUE")]
        [ParameterValue("LIST_KEY_VALUE")]
        [ParameterValue("PACKAGE_NAME")]
        [ParameterValue("PACKAGE_SPEC")]
        [ParameterValue("PATH")]
        [ParameterValue("PKG_BUILD")]
        [ParameterValue("PKG_NAME")]
        [ParameterValue("PKG_VERSION")]
        [ParameterValue("REGEX")]
        [ParameterValue("REMOTE_DEFINITION")]
        [ParameterValue("REPODATA_FNS")]
        [ParameterValue("REVISION")]
        [ParameterValue("SHELLS")]
        [ParameterValue("SHOW")]
        [ParameterValue("SUBDIR")]
        [ParameterValue("TEMPFILES")]
        internal static List<string> NullReturn()
        {
            return new List<string>();
        }

        /// <summary>
        /// Get a list of conda environments.
        /// </summary>
        /// <returns>List of conda environments.</returns>
        [ParameterValue("ENVIRONMENT")]
        internal static List<string> GetEnvironments()
        {
            List<string> condaEnvironments = new()
            {
                ROOT_ENV_NAME
            };

            // Get the path to the conda root.
            // TODO [ ][CONDAHELPER] Find how conda locates environments created with --prefix.
            // There is a list of environments in ~\.conda\environments.txt
            // First row is base, subsequent lines we need to pull the final folder name (may not be envs).
            string? conda_root = Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process);
            if (conda_root is not null)
            {
                condaEnvironments.AddRange(
                    Directory.GetDirectories(conda_root + @"\envs\")
                    .Select(d => new DirectoryInfo(d).Name));
            }
            return condaEnvironments;
        }

        /// <summary>
        /// Return a list of experimental solvers.
        /// </summary>
        /// <returns>List of suggested solvers.</returns>
        [ParameterValue("SOLVER")]
        internal static List<string> ExperimentalSolvers()
        {
            List<string> solvers = new() { "classic", "libmamba", "libmamba-draft" };
            return solvers;
        }
    }
}
