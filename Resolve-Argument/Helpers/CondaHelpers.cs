

namespace Resolve_Argument.Helpers
{
    using System;

    /// <summary>
    /// Attribute declaring the Parameter value that the method provides
    /// suggestions for.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    public class ParameterValueAttribute : System.Attribute
        {
            private string name;

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


        internal static List<string> GetParamaterValues(string parameterName)
        {
            List<string> results;

            // *************** Following to study concerning reflextion and calling helpers *****
            // Returns all currenlty loaded assemblies
            // returns all types defined in this assemblies
            // only yields classes
            // returns all methods defined in those classes
            // returns only methods that have the InvokeAttribute
            // We need to filter down to Paramter Value with a particular name parameterName.
            var methods = AppDomain.CurrentDomain.GetAssemblies() 
                .SelectMany(x => x.GetTypes()) 
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods()) 
                .Where(x => x.GetCustomAttributes(typeof(ParameterValueAttribute), false).FirstOrDefault() != null);
            

            foreach (var method in methods) // iterate through all found methods
            {
                // Instantiate the class (note, all methods are static, so shouldn't need to instatiate a class.
                var obj = Activator.CreateInstance(method.DeclaringType); 
                method.Invoke(obj, null); // invoke the method
            }

            // ************ End Study Section ****************************************************


            switch (parameterName)
            {
                case "ENVIRONMENT":
                    {
                        results = GetEnvironments(); break;
                    }
                default:
                    {
                        results = new List<string>();
                        break;
                    }
            }
            return results;
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
