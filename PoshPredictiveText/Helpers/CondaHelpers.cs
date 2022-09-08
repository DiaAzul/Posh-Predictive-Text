

namespace PoshPredictiveText.Helpers
{
    using PoshPredictiveText;
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Reflection;


    /// <summary>
    /// Attribute declaring the Parameter value that the method provides
    /// suggestions for.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterValueAttribute : Attribute
    {
        internal string Name { get; private set; }

        /// <summary>
        /// Save the name of the parameter.
        /// </summary>
        /// <param name="name">Name of the Parameter Value</param>
        public ParameterValueAttribute(string name)
        {
            Name = name;
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
        const string CONDA_SETTINGS_FOLDER = ".conda";
        const string CONDA_ENVIRONMENTS_FILE = "environments.txt";

        /// <summary>
        /// Returns suggested parameter values for a given named parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="wordToComplete">Word to complete</param>
        /// <returns>Suggested parameter values.</returns>
        /// <remarks>This method uses internal reflection to look up the
        /// appropriate method to be called. If the method is not found an
        /// empty suggestions list is returned.</remarks>
        internal static List<Suggestion> GetParamaterValues(string parameterName, string wordToComplete)
        {
            List<Suggestion> results = new();

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
                            object[] Parameters = new object[1];
                            Parameters[0] = wordToComplete;
                            var methodResult = methods.First().Invoke(null, Parameters);
                            if (methodResult != null)
                            {
                                results = (List<Suggestion>)methodResult;
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
        [ParameterValue("CONDAHELPERTESTFAIL")]
        internal static List<Suggestion> NullReturn(string wordToComplete)
        {
            return new List<Suggestion>();
        }

        /// <summary>
        /// Additional method to test lookup by attribute.
        /// </summary>
        /// <param name="wordToComplete"></param>
        /// <returns></returns>
        [ParameterValue("CONDAHELPERTEST")]
        [ParameterValue("CONDAHELPERTESTFAIL")]
        internal static List<Suggestion> Test(string wordToComplete)
        {
            var results = new List<Suggestion>()
            {
                new Suggestion()
                {
                    CompletionText = wordToComplete,
                    ListText = "TestSuccessful",
                    Type = CompletionResultType.ParameterValue,
                    ToolTip = ""
                }
            };
            return results;
        }

        /// <summary>
        /// Get a list of conda environments.
        /// </summary>
        /// <returns>List of conda environments.</returns>
        [ParameterValue("ENVIRONMENT")]
        internal static List<Suggestion> GetEnvironments(string wordToComplete)
        {
            string condaRoot = Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process) ?? "";

            // The environments are listed in ~\.conda\environments.txt
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string environmentsFile = Path.Combine(home, CONDA_SETTINGS_FOLDER, CONDA_ENVIRONMENTS_FILE);
            List<string> environments = File.ReadAllLines(environmentsFile).ToList();

            List<Suggestion> environmentSuggestions = new();
            foreach (var envPath in environments)
            {
                string envName;
                // Skip base environment, it's already added when the list was created.
                if (envPath == condaRoot)
                {
                    envName = ROOT_ENV_NAME;
                }
                else
                {
                    // If we are in root add the environment name, otherwise we need the full --prefix path.
                    if (envPath.StartsWith(condaRoot))
                    {
                        envName = Path.GetFileName(envPath);
                    }
                    else
                    {
                        envName = envPath;
                    }
                }
                if (envName.Contains(wordToComplete))
                {
                    Suggestion suggestion = new()
                    {
                        CompletionText =  $"\"{envName}\"",
                        ListText = envName,
                        Type = CompletionResultType.ParameterValue,
                        ToolTip = envPath
                    };

                    environmentSuggestions.Add(suggestion);
                }
            }
            return environmentSuggestions;
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
