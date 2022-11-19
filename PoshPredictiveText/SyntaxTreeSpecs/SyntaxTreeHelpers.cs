

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
    internal static partial class SyntaxTreeHelpers
    {
        /// <summary>
        /// Returns suggested parameter values for a given named parameter.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="wordToComplete">Word to complete</param>
        /// <returns>Suggested parameter values.</returns>
        /// <remarks>This method uses internal reflection to look up the
        /// appropriate method to be called. If the method is not found an
        /// empty suggestions list is returned.</remarks>
        // TODO [BUG][SYNTAXTREEHELPER] Review return types for suggestions -> Syntax item is not appropriate.
        internal static List<Suggestion> GetParamaterValues(string parameterName, string wordToComplete)
        {
            List<Suggestion> results = new();

            // TODO [HIGH][SYNTAXTREEHELPER] Move conda tests to conda helpers.
            // Test conda is installed.
            //var testCondaRoot = Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process);
            //if (Environment.GetEnvironmentVariable(CONDA_ROOT, EnvironmentVariableTarget.Process) is null)
            //{
            //    return results;
            //}

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
    }
}
