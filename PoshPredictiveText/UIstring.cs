

namespace PoshPredictiveText
{
    using System.Reflection;
    using System.Resources;

    internal class UIstring
    {
        /// <summary>
        /// Gets a resource string for the UIString resource file.
        /// </summary>
        /// <param name="resourceId">Name of the resource.</param>
        /// <returns>Value of the resource or empty value if the resource does not exist.</returns>
        /// <exception cref="SyntaxTreeException">Raised if the UIstring Resource file does not exist.</exception>
        internal static string Resource(string resourceId)
        {
            const string BASE_NAME = "PoshPredictiveText.UIStrings";
            var resourceManager = new ResourceManager(BASE_NAME, Assembly.GetExecutingAssembly());
            string returnString;
            try
            {
                returnString = resourceManager.GetString(resourceId) ?? "";
            }
            catch (MissingManifestResourceException ex)
            {
                throw new SyntaxTreeException("Missing UI Resource file.", ex);
            }

            return returnString;
        }
    }
}
