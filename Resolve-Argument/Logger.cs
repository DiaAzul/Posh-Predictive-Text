
namespace ResolveArgument
{
    /// <summary>
    /// Print log messages with timestamp to file.
    /// </summary>
    internal static class LOGGER
    {
        // TODO Make logging an option with a definable output logfile
        // -logfile
        private const string LOGFILE = @"C:\templogfiles\logfile.txt";
        internal static void Write(string text)
        {
            string timestamp = DateTime.Now.ToString("s");
            string outputText = $"[{timestamp}] {text}";

            if (!File.Exists(LOGFILE))
            {
                using StreamWriter sw = File.CreateText(LOGFILE);
                sw.WriteLine(outputText);
            }
            else
            {
                using StreamWriter sw = File.AppendText(LOGFILE);
                sw.WriteLine(outputText);
            }
        }
    }
}
