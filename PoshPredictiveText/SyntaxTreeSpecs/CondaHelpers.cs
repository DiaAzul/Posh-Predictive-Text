

namespace PoshPredictiveText.SyntaxTreeSpecs
{
    using PoshPredictiveText;
    using PoshPredictiveText.SyntaxTrees;
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
        internal string Command { get; private set; }

        /// <summary>
        /// Save the name of the parameter.
        /// </summary>
        /// <param name="name">Name of the Parameter Value</param>
        public ParameterValueAttribute(string command, string name)
        {
            Command = command;
            Name = name;
        }
    }

    /// <summary>
    /// Conda helpers to provide parameter values completions.
    /// </summary>
    internal static class SyntaxTreeHelpers
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

            // Test conda is installed.
            var testCondaRoot = Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process);
            if (Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process) is null)
            {
                return results;
            }

            var methods = typeof(SyntaxTreeHelpers).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Where(method =>
                method.GetCustomAttributes(typeof(ParameterValueAttribute), false).FirstOrDefault() != null
                && method.GetCustomAttributes<ParameterValueAttribute>()
                .ToList()
                .Contains(new ParameterValueAttribute("conda", parameterName)));

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
        [ParameterValue("conda", "CHANNEL")]
        [ParameterValue("conda", "CWD")]
        [ParameterValue("conda", "DESCRIBE")]
        [ParameterValue("conda", "ENV")]
        [ParameterValue("conda", "ENV_KEY")]
        [ParameterValue("conda", "ENV_KEY_VALUE")]
        [ParameterValue("conda", "EXECUTABLE_CALL")]
        [ParameterValue("conda", "FILE")]
        [ParameterValue("conda", "KEY")]
        [ParameterValue("conda", "KEY_TO_REMOVE")]
        [ParameterValue("conda", "KEY_VALUE")]
        [ParameterValue("conda", "LIST_KEY_VALUE")]
        [ParameterValue("conda", "PACKAGE_NAME")]
        [ParameterValue("conda", "PACKAGE_SPEC")]
        [ParameterValue("conda", "PATH")]
        [ParameterValue("conda", "PKG_BUILD")]
        [ParameterValue("conda", "PKG_NAME")]
        [ParameterValue("conda", "PKG_VERSION")]
        [ParameterValue("conda", "REGEX")]
        [ParameterValue("conda", "REMOTE_DEFINITION")]
        [ParameterValue("conda", "REPODATA_FNS")]
        [ParameterValue("conda", "REVISION")]
        [ParameterValue("conda", "SHELLS")]
        [ParameterValue("conda", "SHOW")]
        [ParameterValue("conda", "SUBDIR")]
        [ParameterValue("conda", "TEMPFILES")]
        [ParameterValue("conda", "CONDAHELPERTESTFAIL")]
        internal static List<Suggestion> NullReturn(string wordToComplete)
        {
            return new List<Suggestion>();
        }

        /// <summary>
        /// Additional method to test lookup by attribute.
        /// </summary>
        /// <param name="wordToComplete"></param>
        /// <returns></returns>
        [ParameterValue("conda", "CONDAHELPERTEST")]
        [ParameterValue("conda", "CONDAHELPERTESTFAIL")]
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
        [ParameterValue("conda", "ENVIRONMENT")]
        internal static List<Suggestion> GetEnvironments(string wordToComplete)
        {
            List<Suggestion> environmentSuggestions = new();
            string condaRoot = Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process) ?? "";
            // The environments are listed in ~\.conda\environments.txt
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string environmentsFile = Path.Combine(home, CONDA_SETTINGS_FOLDER, CONDA_ENVIRONMENTS_FILE);

            if (!File.Exists(environmentsFile)) return environmentSuggestions;
            List<string> environments = File.ReadAllLines(environmentsFile).ToList();

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
                if (envName.StartsWith(wordToComplete))
                {
                    Suggestion suggestion = new()
                    {
                        CompletionText = CommonTasks.EncapsulateIfSpaces(envName, '\''),
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
        [ParameterValue("conda", "SOLVER")]
        internal static List<string> ExperimentalSolvers()
        {
            List<string> solvers = new() { "classic", "libmamba", "libmamba-draft" };
            return solvers;
        }
    }
}
