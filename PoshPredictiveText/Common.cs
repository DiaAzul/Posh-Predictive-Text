
namespace PoshPredictiveText
{
    using System.Text.RegularExpressions;

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
            return inputString.Contains(' ') switch
            {
                true => string.Concat(wrapper, inputString, wrapper),
                false => inputString,
            };
        }

        /// <summary>
        /// Removes single and double quotes from start and end of input string.
        /// </summary>
        /// <param name="inputString">String from which to remove quotes.</param>
        /// <returns>String with opening and closing quotes removed.</returns>
        internal static string Decapsulate(string inputString)
        {
            Regex stripper = new(@"^['""](.*)['""]$",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = stripper.Match(inputString);

            return match.Groups.Count switch
            {
                2 => match.Groups[1].Value,
                _ => inputString,
            };
        }

        /// <summary>
        /// Extract the command name from a string.
        /// 
        /// Commands could be entered on the command line using its name (conda),
        /// the name with its exeuctable file type (conda.exe) or 
        /// with the full path to the executable (C:\mambaforge\scripts\conda.exe).
        /// </summary>
        /// <param name="inputString">String containing command</param>
        /// <returns>Command</returns>
        internal static string ExtractCommand(string inputString)
        {
            Regex extract = new(@"[\/]?([^\/\\]*?)(?:\..{3})?$",
                                RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = extract.Match(inputString);

            return match.Groups.Count switch
            {
                2 => match.Groups[1].Value,
                _ => inputString,
            };
        }
    }
}
