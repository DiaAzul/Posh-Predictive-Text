

namespace PoshPredictiveText
{
    using System.Text.RegularExpressions;
    using static System.Net.Mime.MediaTypeNames;

    /// <summary>
    /// Functions which are applicable across helpers
    /// </summary>
    internal class CommonTasks
    {
        /// <summary>
        /// Encapsulate a string with a wrapper character (usually ' or ")
        /// if the string contains spaces.
        /// </summary>
        /// <param name="inputString">String to encapsulate.</param>
        /// <param name="wrapper">Character to wrap around string.</param>
        /// <returns>Encapsulated string if it contains spaces.</returns>
        internal static string EncapsulateIfSpaces(string inputString, char wrapper)
        {
            if (inputString.Contains(' '))
            {
                return string.Concat(wrapper, inputString, wrapper);
            }
            return inputString;
        }

        /// <summary>
        /// Removes single and double quotes from start and end of input string.
        /// </summary>
        /// <param name="inputString">String from which to remove quotes.</param>
        /// <returns>String with opening and closing quotes removed.</returns>
        internal static string Decapsulate(string inputString)
        {
            Regex stripper = new(@"^['""](.*)['""]$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = stripper.Match(inputString);

            return match.Groups.Count switch
            {
                2 => match.Groups[1].Value,
                _ => inputString,
            };
        }
    }
}
